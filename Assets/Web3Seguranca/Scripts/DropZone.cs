using UnityEngine;
using UnityEngine.Events;

namespace Web3Seguranca
{
    public enum ZoneType { SalvarConfirmar, Lixeira }

    /// <summary>
    /// Área de entrega (trigger). Coloque um BoxCollider marcado como "Is Trigger"
    /// cobrindo a boca da lixeira / a bandeja verde.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DropZone : MonoBehaviour
    {
        public ZoneType tipo = ZoneType.Lixeira;

        [Header("Encaixe")]
        [Tooltip("Onde o card fica depois de entregue (ex.: dentro da lixeira, sobre a bandeja).")]
        [SerializeField] Transform pontoDeEncaixe;
        [SerializeField] float espacamentoEntreCards = 0.012f;
        [Tooltip("1 = mantém o tamanho; 0.5 = encolhe (bom para a lixeira).")]
        [SerializeField, Range(0.05f, 1f)] float escalaFinal = 1f;

        [Header("Feedback local")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip somAcerto;
        [SerializeField] AudioClip somErro;
        [SerializeField] ParticleSystem efeitoAcerto;
        [SerializeField] ParticleSystem efeitoErro;

        public UnityEvent<MessageCard> onCardCorreto;
        public UnityEvent<MessageCard> onCardErrado;

        int quantidade;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        public void Receber(MessageCard card)
        {
            if (card == null || card.Resolvido) return;

            bool ehGolpe = card.Dados.tipo == MessageKind.Golpe;
            bool correto = (tipo == ZoneType.Lixeira) == ehGolpe;

            Transform alvo = pontoDeEncaixe ? pontoDeEncaixe : transform;
            Vector3 pos = alvo.position + Vector3.up * espacamentoEntreCards * quantidade;
            quantidade++;

            card.Resolver(correto, pos, alvo.rotation, escalaFinal);

            if (audioSource)
            {
                var clip = correto ? somAcerto : somErro;
                if (clip) audioSource.PlayOneShot(clip);
            }
            var fx = correto ? efeitoAcerto : efeitoErro;
            if (fx) fx.Play();

            (correto ? onCardCorreto : onCardErrado)?.Invoke(card);
        }

        public void Limpar() => quantidade = 0;
    }
}