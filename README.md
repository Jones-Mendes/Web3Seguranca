# Desafio 10 — Segurança e Golpes (VR)

**Curso de Web 3.0 • Desconfiar antes de clicar**

Experiência em realidade virtual para **Meta Quest** em que o visitante recebe 3 mensagens e precisa separar, com as mãos, o que é **golpe** (vai para a **Lixeira**) do que é **seguro** (vai para **Salvar/Confirmar**). Na Web 3.0 não existe suporte que desfaça uma transação — separar mensagens com as mãos treina o hábito de desconfiar antes de agir.

## Como funciona

- A cada rodada são sorteadas **3 mensagens** de um banco com **14** (8 golpes e 6 seguras), sempre com pelo menos um de cada tipo.
- Ao entregar um card, a moldura fica **verde** (acerto) ou **vermelha** (erro) e o painel mostra a **explicação**.
- Acertou as 3? **A sala comemora** com confete, luzes e som. Errou? O painel mostra o que era cada mensagem.
- Uma nova rodada começa automaticamente.

## Requisitos

| Item | Versão |
| --- | --- |
| Unity | **6.3 LTS (6000.3.13f1)** com módulos Android e Web |
| Render Pipeline | URP |
| XR Interaction Toolkit | 3.3.2 |
| XR Hands | 1.7.3 |
| OpenXR | 1.16.1 |
| Óculos | Meta Quest 2 / 3 / 3S (rastreamento de mãos) |

## Como abrir

1. Clone o repositório.
2. No Unity Hub: **Add > Add project from disk** e escolha a pasta.
3. Abra com a Unity **6000.3.13f1** (a primeira abertura demora, pois a pasta `Library` é recriada).
4. Abra a cena `Assets/Scenes/Desafio10.unity`.

Para testar no editor sem óculos, deixe o **XR Device Simulator** ativo na cena e a plataforma em **Windows**.

## Build para o Meta Quest

1. Desative o **XR Device Simulator** na cena.
2. **File > Build Profiles > Android > Switch Platform** (ASTC).
3. Player Settings: Minimum API **32**, **IL2CPP**, **ARM64**.
4. Com o Quest conectado (modo desenvolvedor), **Build And Run**.
5. Depois, volte a plataforma para **Windows** para continuar editando.

## Estrutura

```
Assets/Scenes/Desafio10.unity        cena VR
Assets/Scenes/Desafio10_Web.unity    cena da versão web (mouse) — em desenvolvimento
Assets/Web3Seguranca/
  Scripts/    MessageBank, MessageCard, DropZone, ChallengeManager, RoomCelebration, ArrastarComMouse
  Editor/     VerificarInstalacao (menu Web3 Seguranca > Verificar instalação)
  Data/       BancoDeMensagens (mensagens e explicações — editável no Inspector)
  Prefabs/    MessageCard
  Materials/  Audio/  Imagens/
```

## Editar as mensagens

Selecione `Assets/Web3Seguranca/Data/BancoDeMensagens` e altere a lista no Inspector (Título, Corpo, Tipo e Explicação). Não é preciso mexer em código.

## Equipe

Carlos Delfino • Josias Batista • Vivianne Moreira • Jones Mendes