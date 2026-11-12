using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Toca um som (meme) quando o jogador faz Grab no quadro.
// O quadro NAO sai da parede: usa XR Simple Interactable.
[RequireComponent(typeof(XRSimpleInteractable))]
public class QuadroSom : MonoBehaviour
{
    [Header("Som")]
    public AudioClip somMeme;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Tempo minimo (segundos) entre um som e outro, para nao virar bagunca.")]
    public float intervaloMinimo = 1f;

    [Header("Efeito visual")]
    public bool pulsarAoTocar = true;
    [Tooltip("Quanto o quadro cresce na pulsada (0.08 = 8%).")]
    public float forcaPulso = 0.08f;
    public float duracaoPulso = 0.3f;

    AudioSource fonte;
    XRSimpleInteractable interactable;
    Vector3 escalaOriginal;
    float ultimoToque = -10f;
    Coroutine pulsoAtual;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        escalaOriginal = transform.localScale;

        fonte = GetComponent<AudioSource>();
        if (fonte == null) fonte = gameObject.AddComponent<AudioSource>();
        fonte.playOnAwake = false;
        fonte.loop = false;
        fonte.spatialBlend = 0.8f;   // quase 3D: parece vir do quadro, mas ainda se ouve de longe
        fonte.minDistance = 4f;
        fonte.maxDistance = 20f;
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(AoAgarrar);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(AoAgarrar);
    }

    void AoAgarrar(SelectEnterEventArgs args)
    {
        if (somMeme == null) return;
        if (Time.time - ultimoToque < intervaloMinimo) return;
        ultimoToque = Time.time;

        fonte.clip = somMeme;
        fonte.volume = volume;
        fonte.Play(); // se ja estava tocando, recomeca do inicio

        if (pulsarAoTocar)
        {
            if (pulsoAtual != null) StopCoroutine(pulsoAtual);
            pulsoAtual = StartCoroutine(Pulsar());
        }
    }

    IEnumerator Pulsar()
    {
        float t = 0f;
        while (t < duracaoPulso)
        {
            t += Time.deltaTime;
            float p = Mathf.Sin((t / duracaoPulso) * Mathf.PI); // sobe e desce
            transform.localScale = escalaOriginal * (1f + forcaPulso * p);
            yield return null;
        }
        transform.localScale = escalaOriginal;
        pulsoAtual = null;
    }
}