using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Web3Seguranca
{
    /// <summary>
    /// "A sala comemora": confete, luzes trocando de cor, som de vitória e animações.
    /// Tudo é opcional — preencha só o que existir na sua cena.
    /// </summary>
    public class RoomCelebration : MonoBehaviour
    {
        [Header("Vitória")]
        [SerializeField] ParticleSystem[] confetes;
        [SerializeField] Light[] luzes;
        [SerializeField] float duracaoFesta = 6f;
        [SerializeField] float velocidadeCores = 0.6f;
        [SerializeField] float intensidadeFesta = 2.5f;
        [SerializeField] Animator[] animadores;
        [SerializeField] string triggerVitoria = "Comemorar";

        [Header("Erro")]
        [SerializeField] Color corErro = new Color(1f, 0.2f, 0.15f);
        [SerializeField] int piscadasErro = 3;

        [Header("Áudio")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip somVitoria;
        [SerializeField] AudioClip somErro;

        public UnityEvent onComemorar;
        public UnityEvent onErro;

        Color[] coresOriginais;
        float[] intensidadesOriginais;
        Coroutine efeito;

        void Awake()
        {
            coresOriginais = new Color[luzes.Length];
            intensidadesOriginais = new float[luzes.Length];
            for (int i = 0; i < luzes.Length; i++)
            {
                if (!luzes[i]) continue;
                coresOriginais[i] = luzes[i].color;
                intensidadesOriginais[i] = luzes[i].intensity;
            }
        }

        public void Comemorar()
        {
            Parar();
            foreach (var p in confetes) if (p) p.Play();
            foreach (var a in animadores) if (a) a.SetTrigger(triggerVitoria);
            if (audioSource && somVitoria) audioSource.PlayOneShot(somVitoria);
            efeito = StartCoroutine(FestaDeLuzes());
            onComemorar?.Invoke();
        }

        public void SinalizarErro()
        {
            Parar();
            if (audioSource && somErro) audioSource.PlayOneShot(somErro);
            efeito = StartCoroutine(PiscarErro());
            onErro?.Invoke();
        }

        public void Resetar()
        {
            Parar();
            foreach (var p in confetes) if (p) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        IEnumerator FestaDeLuzes()
        {
            for (float t = 0f; t < duracaoFesta; t += Time.deltaTime)
            {
                for (int i = 0; i < luzes.Length; i++)
                {
                    if (!luzes[i]) continue;
                    float matiz = Mathf.Repeat(t * velocidadeCores + i / (float)luzes.Length, 1f);
                    luzes[i].color = Color.HSVToRGB(matiz, 0.8f, 1f);
                    luzes[i].intensity = intensidadeFesta;
                }
                yield return null;
            }
            RestaurarLuzes();
            efeito = null;
        }

        IEnumerator PiscarErro()
        {
            for (int n = 0; n < piscadasErro; n++)
            {
                foreach (var l in luzes) if (l) l.color = corErro;
                yield return new WaitForSeconds(0.25f);
                RestaurarLuzes();
                yield return new WaitForSeconds(0.25f);
            }
            efeito = null;
        }

        void Parar()
        {
            if (efeito != null) { StopCoroutine(efeito); efeito = null; }
            RestaurarLuzes();
        }

        void RestaurarLuzes()
        {
            if (coresOriginais == null) return;
            for (int i = 0; i < luzes.Length; i++)
            {
                if (!luzes[i]) continue;
                luzes[i].color = coresOriginais[i];
                luzes[i].intensity = intensidadesOriginais[i];
            }
        }
    }
}
