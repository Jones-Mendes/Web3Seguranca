using System.Collections.Generic;
using UnityEngine;

namespace Web3Seguranca
{
    public enum MessageKind { Seguro, Golpe }

    [System.Serializable]
    public class MessageData
    {
        public string titulo;
        [TextArea(2, 4)] public string corpo;
        public MessageKind tipo;
        [Tooltip("Mostrado no painel depois que o jogador separa o card.")]
        [TextArea(2, 4)] public string explicacao;

        public MessageData(string titulo, string corpo, MessageKind tipo, string explicacao)
        {
            this.titulo = titulo; this.corpo = corpo; this.tipo = tipo; this.explicacao = explicacao;
        }
    }

    /// <summary>
    /// Crie em: Assets > Create > Web3 Seguranca > Banco de Mensagens.
    /// O asset já nasce preenchido com as mensagens padrão (edite à vontade no Inspector).
    /// </summary>
    [CreateAssetMenu(menuName = "Web3 Seguranca/Banco de Mensagens", fileName = "BancoDeMensagens")]
    public class MessageBank : ScriptableObject
    {
        public List<MessageData> mensagens = new List<MessageData>();

        void Reset() => mensagens = MensagensPadrao();

        [ContextMenu("Restaurar mensagens padrão")]
        void Restaurar() => mensagens = MensagensPadrao();

        static MessageData G(string t, string c, string e) => new MessageData(t, c, MessageKind.Golpe, e);
        static MessageData S(string t, string c, string e) => new MessageData(t, c, MessageKind.Seguro, e);

        public static List<MessageData> MensagensPadrao() => new List<MessageData>
        {
            // ---------- GOLPES ----------
            G("URGENTE:",
              "Envie sua frase de recuperação (12 palavras) para o suporte!",
              "Nenhum suporte legítimo pede a frase de recuperação. Quem tem a frase tem a carteira — e não existe ninguém para desfazer o roubo."),
            G("AIRDROP GRÁTIS!",
              "Conecte sua carteira e ganhe 1000 tokens agora!",
              "Prêmio que você não esperava + 'conecte sua carteira' costuma levar a um site que pede aprovações maliciosas e esvazia a carteira."),
            G("CARTEIRA BLOQUEADA",
              "Atividade suspeita detectada. Valide sua carteira em metamask-verificar.io em até 1 hora.",
              "Urgência + link estranho = golpe. Carteiras não 'bloqueiam' e nunca pedem validação por link."),
            G("MODERADOR (DM)",
              "Oi! Sou admin do Discord, vi seu problema. Preencha este formulário com sua seed que eu resolvo.",
              "Moderadores de verdade não chamam no privado oferecendo ajuda. Esse é um dos golpes mais comuns em comunidades."),
            G("DOBRE SEUS ETH",
              "Envie 0,1 ETH para este endereço e receba 0,2 ETH de volta em minutos!",
              "Ninguém devolve o dobro. Transação enviada na blockchain não volta."),
            G("REEMBOLSO DISPONÍVEL",
              "Assine esta aprovação ILIMITADA de USDT para receber seu reembolso.",
              "Aprovação ilimitada dá ao contrato permissão para gastar TODOS esses tokens da sua carteira."),
            G("MINT EXCLUSIVO",
              "Último NFT da coleção! Mint grátis em 5 minutos: bit.ly/nft-free",
              "Link encurtado esconde o destino real, e a pressa existe para você não pensar antes de clicar."),
            G("ATUALIZAÇÃO OBRIGATÓRIA",
              "Baixe a nova versão da carteira por este link ou perderá seus fundos.",
              "Atualize apenas pela loja oficial ou pelo site oficial que você mesmo digitou."),

            // ---------- SEGURAS ----------
            S("TRANSAÇÃO REAL:",
              "Confirmar envio de 0,05 ETH (iniciado por você).",
              "Você iniciou a ação e sabe o destino. Mesmo assim, confira endereço e valor antes de confirmar."),
            S("LEMBRETE DE SEGURANÇA",
              "Guarde sua frase de recuperação offline, em papel, longe de fotos e da nuvem.",
              "Boa prática: a frase nunca deve ficar em dispositivos conectados nem ser digitada em sites."),
            S("REVISE PERMISSÕES",
              "Dica: revise de tempos em tempos as permissões de tokens que você concedeu e revogue as que não usa.",
              "Revogar aprovações antigas reduz o estrago caso algum contrato seja comprometido."),
            S("PROTEJA SUA CONTA",
              "Ative a autenticação em dois fatores (2FA) por aplicativo na sua corretora.",
              "2FA por aplicativo protege bem mais que senha sozinha ou código por SMS."),
            S("CONFIRA O ENDEREÇO",
              "Antes de enviar, compare o endereço inteiro, não só o começo e o fim.",
              "Golpistas criam endereços parecidos com os seus contatos (address poisoning)."),
            S("SITE OFICIAL",
              "Acesse a corretora digitando o endereço oficial ou pelos seus favoritos salvos.",
              "Evita cair em sites clonados vindos de anúncios, e-mails ou mensagens."),
        };
    }
}
