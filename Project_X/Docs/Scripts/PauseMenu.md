# PauseMenu (UI / Fluxo de Jogo)

Script responsável pelo **menu de pausa** do jogo. Ele centraliza a lógica de abrir/fechar o pause, travar o input do jogador, pausar o tempo (opcional) e navegar para _Restart_ ou _Level Select_.

---

## Visão geral

O `PauseMenu` faz:

- Exibir/ocultar o painel de pause (via `CanvasGroup`).
- Pausar e retomar o jogo usando `Time.timeScale` (opcional).
- Desabilitar/habilitar o `PlayerMover` enquanto o jogo está pausado.
- Redirecionar para:
  - **Resume** → volta ao jogo atual.
  - **Restart** → recarrega o nível atual via `LevelManager.Reload()`.
  - **Level Select** → carrega a cena `01_LevelSelect`.
- Fechar automaticamente ao carregar um novo nível (escuta `GameEvents.OnLevelLoaded`).
- Controlar foco de UI para gamepad/teclado via `EventSystem`.

---

## Campos principais

### Singleton / Estado

- `public static PauseMenu Instance`  
  Acesso global ao menu de pausa (um por cena).
- `public static bool IsPaused`  
  Helper estático para saber se o pause está visível.

### Referências de UI

- `CanvasGroup panel`  
  Painel principal do menu de pausa (controla alpha, interatividade e raycasts).
- `Button btnResume`  
  Botão de **Retomar**.
- `Button btnRestart`  
  Botão de **Reiniciar nível**.
- `Button btnLevelSelect`  
  Botão de **Voltar para Level Select**.

### Opções

- `bool useTimeScalePause`  
  Se `true`, o tempo é pausado com `Time.timeScale = 0f` quando o menu está aberto e volta para `1f` ao fechar.

### Seleção / Navegação

- `GameObject firstSelected`  
  Objeto de UI que recebe o foco inicial quando o pause é aberto (ex.: botão Resume).
- `GameObject lastSelectedBeforePause`  
  Guarda o objeto selecionado antes de pausar, para restaurar após o `Hide()`.

### Input

- `InputAction pauseAction`  
  Ação do **Input System** que escuta:
  - `Esc` no teclado
  - `Start` no gamepad

### Outros

- `PlayerMover cachedPlayer`  
  Cache temporário do player para travar o componente enquanto pausado.
- `bool isVisible`  
  Flag interna para saber se o menu está aberto.
- `System.Action onLevelLoadedHandler`  
  Delegate usado para registrar/remover o listener de `GameEvents.OnLevelLoaded`.

---

## Fluxo de funcionamento

### Awake

1. Garante o **singleton**:
   - Se já existir outro `PauseMenu`, dá `Destroy(this.gameObject)`.
   - Se não, define `Instance = this`.
2. Cria e configura `pauseAction`:
   - Bind para `<Keyboard>/escape`
   - Bind para `<Gamepad>/start`
   - Chama `pauseAction.Enable()`.
3. Se `panel` não estiver setado no inspector, tenta buscar um `CanvasGroup` filho.
4. Registra listeners nos botões:
   - `btnResume` → `OnClickResume`
   - `btnRestart` → `OnClickRestart`
   - `btnLevelSelect` → `OnClickSelect`
5. Registra `onLevelLoadedHandler = Hide` em `GameEvents.OnLevelLoaded`.
6. Garante que o menu comece **invisível** chamando `HideImmediate()`.

### OnDestroy

- Desabilita `pauseAction`.
- Remove o handler de `GameEvents.OnLevelLoaded`.
- Se estiver usando `Time.timeScale` para pausar, garante que volte a `1f`.
- Zera o `Instance` se este objeto for o singleton atual.

### Update

- Se `pauseAction.WasPressedThisFrame()`:
  - Chama `Toggle()`.
  - `Toggle()` decide entre `Show()` e `Hide()`.

---

## Métodos públicos principais

### `public void Toggle()`

- Se está visível → chama `Hide()`.
- Se está escondido → chama `Show()`.

### `public void Show()`

- Garante que exista `panel` e que ainda não esteja visível.
- Marca `isVisible = true`.
- Ativa UI:
  - `panel.alpha = 1f`
  - `panel.interactable = true`
  - `panel.blocksRaycasts = true`
- Salva o `currentSelectedGameObject` do `EventSystem` e foca em `firstSelected` (se existir).
- Procura um `PlayerMover` na cena e o desabilita (bloqueia input).
- Se `useTimeScalePause` for `true`, define `Time.timeScale = 0f`.

### `public void Hide()`

- Garante que exista `panel` e que esteja visível.
- Marca `isVisible = false`.
- Se `useTimeScalePause` for `true`, volta `Time.timeScale = 1f`.
- Esconde UI:
  - `panel.alpha = 0f`
  - `panel.interactable = false`
  - `panel.blocksRaycasts = false`
- Restaura o foco do `EventSystem` para `lastSelectedBeforePause` (se existir) e limpa essa referência.
- Reativa o `PlayerMover` em `cachedPlayer` e zera o cache.

### `private void HideImmediate()`

- Versão usada para o **estado inicial**:
  - Garante `isVisible = false`.
  - Garante `Time.timeScale = 1f` (se estiver usando pausa por time scale).
  - Zera alpha/interatividade do painel.
  - Zera `cachedPlayer`.

---

## Botões do menu

- `OnClickResume()`  
  Chama `Hide()` e volta ao jogo normalmente.

- `OnClickRestart()`  
  - Chama `Hide()` primeiro.
  - Usa `LevelManager.Instance?.Reload()` para recarregar o nível atual.

- `OnClickSelect()`  
  - Chama `Hide()`.
  - Carrega a cena `"01_LevelSelect"` usando `SceneManager.LoadScene("01_LevelSelect", LoadSceneMode.Single)`.

---

## Dependências e requisitos

Para o `PauseMenu` funcionar corretamente, a cena precisa de:

- Um `EventSystem` ativo.
- Um `CanvasGroup` para o painel de pause.
- Botões de UI conectados aos métodos públicos:
  - `OnClickResume`
  - `OnClickRestart`
  - `OnClickSelect`
- Um `PlayerMover` presente durante o gameplay (para ser desabilitado/habilitado).
- Um `LevelManager` que controla o nível atual.
- `GameEvents.RaiseLevelLoaded()` sendo chamado pelo `LevelManager` após carregar ou recarregar o nível.

---

## Customização rápida

- **Desativar pausa por TimeScale**  
  Desmarcar `useTimeScalePause` caso você queira que o jogo continue "rodando" mesmo com o menu aberto (por exemplo, só pause de input, não de simulação).
- **Configurar primeiro botão selecionado**  
  Arrastar o `Button Resume` para `firstSelected` no inspector para melhorar UX com controle/teclado.
- **Alterar cena de Level Select**  
  Trocar a string `"01_LevelSelect"` em `OnClickSelect()` se o nome da cena for outro.