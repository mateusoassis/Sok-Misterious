# VictoryUI — Documentação

## Visão Geral
`VictoryUI` é o painel exibido quando o jogador completa um nível.  
Ele **pausa o PlayerMover**, mostra botões de **Next**, **Restart** e **Level Select**, e sempre se esconde ao trocar de cena ou nível.

## Responsabilidades
- Exibir o painel de vitória (`Show()`).
- Ocultar o painel com segurança (`Hide()` e `HideImmediate()`).
- Travar e destravar o `PlayerMover`.
- Conectar os botões aos métodos corretos.
- Escutar mudanças de cena e o evento `OnLevelLoaded`.

## Fluxo Interno
- No `Awake()` configura singleton, liga botões e registra o listener de troca de cena.
- No `OnEnable()` oculta imediatamente e escuta `GameEvents.OnLevelLoaded` para sempre esconder ao carregar um novo nível.
- `Show()` ativa o panel e desabilita o PlayerMover.
- `Hide()` desativa o panel e reabilita o PlayerMover.
- Cada botão chama:
  - **Next** → `WinChecker.GoNext()` ou fallback para `LevelManager.LoadNext()`
  - **Restart** → `WinChecker.Restart()` ou fallback para `LevelManager.Reload()`
  - **Select** → carrega `"01_LevelSelect"`

## Observações Importantes
- `SceneManager.activeSceneChanged` garante que o painel nunca fique preso ao mudar de cena.
- O painel começa SEMPRE oculto.
- Se o `WinChecker` não estiver presente na cena, o comportamento usa fallback seguro.