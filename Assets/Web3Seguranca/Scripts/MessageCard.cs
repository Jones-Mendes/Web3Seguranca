using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRI 3.x (no XRI 2.x, apague esta linha)

namespace Web3Seguranca
{
    /// <summary>
    /// Card de mensagem que o jogador pega com a mão (gesto Grab) e joga numa DropZone.
    /// Solto fora de uma zona, volta sozinho para a mesa.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class MessageCard : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] TMP_Text tituloText;
        [SerializeField] TMP_Text corpoText;
        [Tooltip("Renderer da moldura/borda do card, que muda de cor no resultado.")]
        [SerializeField] Renderer moldura;
        [SerializeField] Color corNeutra = new Color(0.55f, 0.45f, 0.95f);
        [SerializeField] Color corAcerto = new Color(0.20f, 0.90f, 0.40f);
        [SerializeField] Color corErro   = new Color(0.95f, 0.25f, 0.20f);

        [Header("Comportamento")]
        [SerializeField] float tempoParaVoltar = 2.5f;
        [SerializeField] float distanciaDeQueda = 2f;
        [SerializeField] float duracaoEncaixe = 0.35f;
        [Tooltip("Se o card ficar parado longe da mesa por este tempo (empurrado, derrubado), ele volta.")]
        [SerializeField] float tempoLongeParaVoltar = 3f;
        [Tooltip("Distância (m) da posição original a partir da qual o card é considerado 'fora do lugar'.")]
        [SerializeField] float distanciaMaxDeCasa = 0.4f;

        [Header("Som")]
        [Tooltip("Toca quando o raio do controle (ou a mão) passa por cima do card.")]
        [SerializeField] AudioClip somHover;
        [SerializeField, Range(0f, 1f)] float volumeHover = 0.5f;

        public MessageData Dados { get; private set; }
        public bool Resolvido { get; private set; }
        public bool Acertou { get; private set; }
        public event System.Action<MessageCard> OnResolvido;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP
        static readonly int ColorId     = Shader.PropertyToID("_Color");     // Built-in

        XRGrabInteractable grab;
        Rigidbody rb;
        MaterialPropertyBlock mpb;
        Pose casa;
        DropZone zonaAtual;
        Coroutine retorno;
        float tempoLonge;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
            mpb = new MaterialPropertyBlock();
        }

        void Start()
        {
            // Card colocado direto na cena (teste, sem ChallengeManager):
            // a "casa" passa a ser a posição em que ele começou.
            if (Dados == null) casa = new Pose(transform.position, transform.rotation);
        }

        void OnEnable()
        {
            grab.selectEntered.AddListener(AoPegar);
            grab.selectExited.AddListener(AoSoltar);
            grab.firstHoverEntered.AddListener(AoPassarPorCima);
        }

        void OnDisable()
        {
            grab.selectEntered.RemoveListener(AoPegar);
            grab.selectExited.RemoveListener(AoSoltar);
            grab.firstHoverEntered.RemoveListener(AoPassarPorCima);
        }

        void AoPassarPorCima(HoverEnterEventArgs _)
        {
            if (somHover && !Resolvido)
                AudioSource.PlayClipAtPoint(somHover, transform.position, volumeHover);
        }

        public void Configurar(MessageData dados, Pose posicaoNaMesa)
        {
            Dados = dados;
            casa = posicaoNaMesa;
            Resolvido = false;
            if (tituloText) tituloText.text = dados.titulo;
            if (corpoText) corpoText.text = dados.corpo;
            PintarMoldura(corNeutra);
            VoltarParaCasa();
        }

        void Update()
        {
            if (Resolvido || grab.isSelected) { tempoLonge = 0f; return; }

            // Caiu muito abaixo da mesa (atravessou o chão etc.): volta na hora.
            if (transform.position.y < casa.position.y - distanciaDeQueda) { VoltarParaCasa(); return; }

            // Ficou longe da mesa sem ninguém segurando (foi empurrado, derrubado
            // ou solto fora das zonas): depois de alguns segundos, volta.
            if (Vector3.Distance(transform.position, casa.position) > distanciaMaxDeCasa)
            {
                tempoLonge += Time.deltaTime;
                if (tempoLonge >= tempoLongeParaVoltar) VoltarParaCasa();
            }
            else tempoLonge = 0f;
        }

        void AoPegar(SelectEnterEventArgs _)
        {
            if (retorno != null) { StopCoroutine(retorno); retorno = null; }
        }

        void AoSoltar(SelectExitEventArgs _)
        {
            if (Resolvido) return;
            if (zonaAtual) zonaAtual.Receber(this);          // soltou em cima da zona
            else retorno = StartCoroutine(VoltarDepois());    // pode ter sido arremessado
        }

        void OnTriggerEnter(Collider other)
        {
            if (Resolvido) return;
            var zona = other.GetComponentInParent<DropZone>();
            if (!zona) return;
            zonaAtual = zona;
            if (!grab.isSelected) zona.Receber(this);         // arremessado direto na zona
        }

        void OnTriggerExit(Collider other)
        {
            var zona = other.GetComponentInParent<DropZone>();
            if (zona && zona == zonaAtual) zonaAtual = null;
        }

        IEnumerator VoltarDepois()
        {
            yield return new WaitForSeconds(tempoParaVoltar);
            retorno = null;
            if (!Resolvido && !grab.isSelected) VoltarParaCasa();
        }

        void VoltarParaCasa()
        {
            tempoLonge = 0f;
            ZerarVelocidade();
            transform.SetPositionAndRotation(casa.position, casa.rotation);
        }

        /// <summary>Chamado pela DropZone quando o card é entregue.</summary>
        public void Resolver(bool acertou, Vector3 posicao, Quaternion rotacao, float escalaFinal)
        {
            if (Resolvido) return;
            Resolvido = true;
            Acertou = acertou;
            if (retorno != null) { StopCoroutine(retorno); retorno = null; }

            ZerarVelocidade();
            rb.isKinematic = true;
            grab.enabled = false;

            PintarMoldura(acertou ? corAcerto : corErro);
            StartCoroutine(Encaixar(posicao, rotacao, escalaFinal));
            OnResolvido?.Invoke(this);
        }

        IEnumerator Encaixar(Vector3 pos, Quaternion rot, float escalaFinal)
        {
            Vector3 p0 = transform.position;
            Quaternion r0 = transform.rotation;
            Vector3 s0 = transform.localScale;
            Vector3 s1 = s0 * escalaFinal;
            for (float t = 0f; t < 1f; )
            {
                t += Time.deltaTime / Mathf.Max(0.01f, duracaoEncaixe);
                float k = Mathf.SmoothStep(0f, 1f, t);
                transform.SetPositionAndRotation(Vector3.Lerp(p0, pos, k), Quaternion.Slerp(r0, rot, k));
                transform.localScale = Vector3.Lerp(s0, s1, k);
                yield return null;
            }
        }

        void ZerarVelocidade()
        {
            if (rb.isKinematic) return;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }

        void PintarMoldura(Color cor)
        {
            if (!moldura) return;
            moldura.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, cor);
            mpb.SetColor(ColorId, cor);
            moldura.SetPropertyBlock(mpb);
        }
    }
}