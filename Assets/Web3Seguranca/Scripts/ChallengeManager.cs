using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Web3Seguranca
{
    /// <summary>
    /// Desafio 10 — Segurança e Golpes.
    /// Sorteia 3 mensagens, espera o visitante separar todas e, se acertar tudo, a sala comemora.
    /// </summary>
    public class ChallengeManager : MonoBehaviour
    {
        [Header("Conteúdo")]
        [SerializeField] MessageBank banco;
        [SerializeField] MessageCard prefabDoCard;
        [Tooltip("Um ponto por card (3 pontos = 3 mensagens). Posicione sobre a mesa.")]
        [SerializeField] Transform[] pontosDeSpawn = new Transform[3];
        [SerializeField, Min(0)] int minimoDeGolpes = 1;
        [SerializeField, Min(0)] int minimoDeSeguras = 1;

        [Header("Cena")]
        [SerializeField] DropZone[] zonas;
        [SerializeField] RoomCelebration sala;
        [SerializeField] TMP_Text painelFeedback;
        [SerializeField] TMP_Text placar;

        [Header("Fluxo")]
        [SerializeField] bool reiniciarAutomaticamente = true;
        [SerializeField] float esperaParaNovaRodada = 10f;

        [Header("Eventos")]
        public UnityEvent onRodadaIniciada;
        public UnityEvent onVitoria;
        public UnityEvent onDerrota;

        const string Verde = "#3CE36B";
        const string Vermelho = "#FF5A4A";
        const string Amarelo = "#F5B942";

        readonly List<MessageCard> cards = new List<MessageCard>();
        int vitorias, rodadas;
        Coroutine reinicio;

        void Start() => NovaRodada();

        /// <summary>Pode ser ligado a um botão "Jogar de novo" no painel.</summary>
        public void NovaRodada()
        {
            if (reinicio != null) { StopCoroutine(reinicio); reinicio = null; }

            foreach (var c in cards) if (c) Destroy(c.gameObject);
            cards.Clear();
            foreach (var z in zonas) if (z) z.Limpar();
            if (sala) sala.Resetar();

            var selecao = SortearMensagens(pontosDeSpawn.Length);
            int n = Mathf.Min(selecao.Count, pontosDeSpawn.Length);
            for (int i = 0; i < n; i++)
            {
                var p = pontosDeSpawn[i];
                var card = Instantiate(prefabDoCard, p.position, p.rotation);
                card.Configurar(selecao[i], new Pose(p.position, p.rotation));
                card.OnResolvido += AoResolver;
                cards.Add(card);
            }

            Mostrar($"<color={Amarelo}>Leia cada mensagem com calma.</color>\n" +
                    "Jogue os GOLPES na lixeira e coloque as mensagens SEGURAS em Salvar/Confirmar.");
            AtualizarPlacar();
            onRodadaIniciada?.Invoke();
        }

        List<MessageData> SortearMensagens(int total)
        {
            var golpes  = Embaralhar(banco.mensagens.Where(m => m.tipo == MessageKind.Golpe));
            var seguras = Embaralhar(banco.mensagens.Where(m => m.tipo == MessageKind.Seguro));

            var escolhidas = new List<MessageData>();
            escolhidas.AddRange(golpes.Take(minimoDeGolpes));
            escolhidas.AddRange(seguras.Take(minimoDeSeguras));

            var resto = Embaralhar(golpes.Skip(minimoDeGolpes).Concat(seguras.Skip(minimoDeSeguras)));
            escolhidas.AddRange(resto.Take(Mathf.Max(0, total - escolhidas.Count)));

            return Embaralhar(escolhidas).Take(total).ToList();
        }

        void AoResolver(MessageCard card)
        {
            int feitos = cards.Count(c => c.Resolvido);
            string titulo = card.Acertou
                ? $"<color={Verde}><b>ACERTOU!</b></color>"
                : $"<color={Vermelho}><b>CUIDADO!</b></color> Essa era {(card.Dados.tipo == MessageKind.Golpe ? "um GOLPE" : "SEGURA")}.";

            Mostrar($"{titulo}\n{card.Dados.explicacao}\n\n<size=80%>({feitos}/{cards.Count} mensagens separadas)</size>");

            if (feitos == cards.Count) StartCoroutine(FinalizarComPausa());
        }

        IEnumerator FinalizarComPausa()
        {
            yield return new WaitForSeconds(2.5f); // dá tempo de ler a explicação do último card
            rodadas++;
            bool tudoCerto = cards.All(c => c.Acertou);

            if (tudoCerto)
            {
                vitorias++;
                Mostrar($"<color={Verde}><size=130%><b>PARABÉNS!</b></size></color>\n" +
                        "Você separou tudo certo. Lembre: na Web 3.0 não existe suporte que desfaça uma transação. " +
                        "Desconfiar antes de clicar é a sua melhor proteção.");
                if (sala) sala.Comemorar();
                onVitoria?.Invoke();
            }
            else
            {
                var sb = new StringBuilder($"<color={Vermelho}><b>Quase!</b></color> Reveja:\n");
                foreach (var c in cards.Where(c => !c.Acertou))
                {
                    string era = c.Dados.tipo == MessageKind.Golpe ? "GOLPE" : "SEGURA";
                    sb.Append($"\n- <b>{c.Dados.titulo}</b> era {era}. {c.Dados.explicacao}");
                }
                Mostrar(sb.ToString());
                if (sala) sala.SinalizarErro();
                onDerrota?.Invoke();
            }

            AtualizarPlacar();
            if (reiniciarAutomaticamente) reinicio = StartCoroutine(ReiniciarDepois());
        }

        IEnumerator ReiniciarDepois()
        {
            yield return new WaitForSeconds(esperaParaNovaRodada);
            reinicio = null;
            NovaRodada();
        }

        void Mostrar(string texto)
        {
            if (painelFeedback) painelFeedback.text = texto;
        }

        void AtualizarPlacar()
        {
            if (placar) placar.text = $"Rodadas: {rodadas}   Acertos totais: {vitorias}";
        }

        static List<T> Embaralhar<T>(IEnumerable<T> itens)
        {
            var l = itens.ToList();
            for (int i = l.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (l[i], l[j]) = (l[j], l[i]);
            }
            return l;
        }
    }
}
