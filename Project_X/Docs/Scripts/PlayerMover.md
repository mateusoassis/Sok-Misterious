# PlayerMover

Controla toda a **lógica de movimento em grid** do jogador em Sokoban:

- Leitura de input (teclado + gamepad) via **Input System**.
- Movimento 4-direcional (sem diagonal), 1 input = 1 célula.
- **Repeat opcional** ao segurar direção.
- Empurrar caixas (com `BoxIdentifier` na mesma Layer sólida).
- **UNDO completo** (player + caixa), com histórico detalhado.
- Integração com:
  - `PlayerLerpMotion` (movimento suave)
  - `PlayerAnimatorBridge` (animação)
  - `BoxLerpMotion` / `BoxGoalState`
  - `GameEvents`
  - `LevelManager` / Bounds

---

## 🎯 Visão Geral

O `PlayerMover` é o “cérebro” do player dentro do grid:

- Mantém a posição lógica em **coordenadas de célula** (`gridPos`).
- Converte entre **grid ↔ mundo** com base em `cellSize`.
- Decide se o movimento é válido (paredes, bounds, caixas, empurrão).
- Registra cada movimento em um **histórico de UNDO**.
- Coordena a animação de movimento e empurrão via outros componentes.

Ele NÃO desenha, NÃO toca som, e NÃO sabe nada de UI — só regras de movimento e estado.

---

## ⚙ Campos Principais (Inspector)

### Grid
- `cellSize`  
  - 1.0 = 1 unidade do Unity por tile.
  - Se o tile for 32px e PPU = 32, deixe 1.

### Repeat
- `enableRepeat`  
  - Se `true`, segurar direção repete passos automaticamente.
- `firstRepeatDelay`  
  - Delay inicial antes do primeiro repeat (ao segurar).
- `repeatInterval`  
  - Intervalo entre passos durante o repeat.

### Colisão e Push
- `solidMask`  
  - LayerMask que bloqueia o player (paredes, caixas).
  - Caixas **devem** estar nessa Layer também.

### Animator
- `motion : PlayerLerpMotion`  
  - Responsável pelo movimento suave do player.
- `bridge : PlayerAnimatorBridge`  
  - Atualiza parâmetros do Animator e flip do sprite.

---

## 🔁 Input (teclado + gamepad)

Criado via código com o **Input System**:

- Movimento (`moveAction`, tipo `Vector2`):
  - W / ↑ = cima
  - S / ↓ = baixo
  - A / ← = esquerda
  - D / → = direita
  - `Gamepad` dpad
  - `Gamepad` left stick

- Undo (`undoAction`, tipo Button):
  - Z (teclado)
  - Y / Button North (gamepad)

Ambos são habilitados em `Awake()` e desabilitados em `OnDestroy()`.

---

## 🧠 Lógica de Update

### 1. UNDO tem prioridade
No início do `Update()`:

1. Se `undoAction.WasPressedThisFrame()`:
   - Se player ou qualquer caixa ainda estiverem em LERP → ignora.
   - Chama `UndoLast()`.
   - Limpa repeat (`heldDir = zero`) e reseta `nextRepeatTime`.

### 2. Bloqueio durante animação
Se `motion.IsMoving == true`, o script:
- Ignora novo input.
- Limpa `heldDir` para evitar repeat armado.

### 3. Leitura de direção 4-way
A partir de `moveAction.ReadValue<Vector2>()`:

- Se `|x| > |y|` → prioriza horizontal.
- Senão, usa vertical (cima/baixo).
- Resultado é um `Vector2Int` em uma de 4 direções: `(1,0), (-1,0), (0,1), (0,-1)`.

### 4. Sem Repeat
Se `enableRepeat == false`:
- 1 toque = 1 passo.
- Move somente quando:
  - `rawDir != zero` **e**
  - antes `heldDir == zero` (evita repetir durante segurar).

### 5. Com Repeat
Se `enableRepeat == true`:
- Detecta se a direção acabou de mudar (ou acabou de ser pressionada).
- Se `justPressed` → move, arma `nextRepeatTime = now + firstRepeatDelay`.
- Se mantida e `Time.time >= nextRepeatTime` → repete passos a cada `repeatInterval`.

---

## 📦 Empurrar Caixas (Push)

A lógica está em `TryStep(dir)`:

1. Calcula célula alvo: `target = gridPos + dir`.
2. Usa `LevelManager.Instance` para **bloquear saída dos Bounds** (`InsideBounds`).
3. Se `solidMask` estiver configurado:
   - Faz `Physics2D.OverlapPoint(worldTarget, solidMask)` no centro da célula alvo.
   - Se não houver nada → passo simples.
   - Se houver algo:
     - Tenta pegar `BoxIdentifier` no collider ou no parent.
     - Se não for caixa → bloqueia movimento.
     - Se for caixa:
       - Calcula `boxTarget = boxGrid + dir`.
       - Checa `Bounds` de novo para a célula atrás.
       - Checa colisão na célula de trás com `OverlapPointAll`, ignorando a própria box.

4. Se puder empurrar:
   - Registra `MoveRecord` no histórico (posição do player e da box).
   - Move o player via `PlayerLerpMotion.MoveBy`.
   - Move a box via `BoxLerpMotion.MoveTo` (ou teleporta caso não tenha esse script).
   - Atualiza `BoxGoalState` da caixa de acordo com `IsGoalCell`.
   - Chama:
     - `GameEvents.RaiseMove()`
     - `GameEvents.RaisePush()`
     - `GameEvents.RaiseGoalsMaybeChanged()`
   - Aciona animação de empurrar: `bridge.PulsePush(0.15f)`.

---

## 👣 Passo Simples (sem caixa)

Quando não há colisão com sólido na célula alvo:

- Cria `MoveRecord` somente com `playerFrom`/`playerTo`.
- Atualiza `gridPos`.
- Dispara `PlayerLerpMotion.MoveBy(dir)` com callback para `bridge.SetMoving(false)`.
- Chama `GameEvents.RaiseMove()`.

---

## ↩️ Sistema de UNDO

### Estrutura `MoveRecord`

```csharp
private struct MoveRecord
{
    public Vector2Int playerFrom, playerTo;
    public BoxIdentifier box; // null se não houve push
    public Vector2Int boxFrom, boxTo;
}
```

- Armazena:
  - De/Para do player.
  - Caixa envolvida (se houver), com sua posição anterior e nova.

### Lista de histórico

```csharp
private readonly List<MoveRecord> history = new List<MoveRecord>(256);
```

- Capacidade inicial 256 (bom pra puzzles longos).
- Um registro por passo (com ou sem caixa).

### `UndoLast()`

- Recupera o último `MoveRecord`.
- Remove da lista.
- Reposiciona o player para `playerFrom` (cancelando qualquer LERP ativo).
- Se havia box:
  - Reposiciona a box para `boxFrom` (também via `BoxLerpMotion.CancelAndSnap` quando possível).
  - Atualiza `BoxGoalState` da box.
- Faz `Physics2D.SyncTransforms()` para garantir consistência física.
- Dispara:
  - `GameEvents.RaiseUndo()`
  - `GameEvents.RaiseGoalsMaybeChanged()`
- Reseta repeat: `heldDir = zero`, `nextRepeatTime = now + firstRepeatDelay`.

### `CanUndoNow()`

- Retorna `false` se:
  - `PlayerMover` está desabilitado (ex.: pausa).
  - Player está em LERP.
  - Qualquer `BoxLerpMotion` ainda está movendo.
  - Não há histórico.

### `TryUndoFromUI()`

- Pensado para botão de UI de undo:
  - Checa `CanUndoNow()`.
  - Chama `UndoLast()`.
  - Reseta repeat.

---

## 🎯 Integração com outros sistemas

- **LevelManager**
  - Usa `LevelManager.Instance.CurrentBounds` + `InsideBounds()` para evitar sair do tabuleiro.
- **BoxLerpMotion**
  - Anima empurrão de caixas.
- **BoxGoalState + GoalIdentifier**
  - `IsGoalCell()` detecta goals via física ou fallback por posição.
  - Atualiza highlight/estado de goal ao final do empurrão e do undo.
- **PlayerLerpMotion**
  - Responsável pelo movimento suave do player, chamado pelo `PlayerMover`.
- **PlayerAnimatorBridge**
  - `SetDirection()` e `SetMoving()` são acionados em `TryStep`/movimento.
  - `PulsePush()` usado como feedback ao empurrar.
- **GameEvents**
  - `RaiseMove`, `RaisePush`, `RaiseUndo`, `RaiseGoalsMaybeChanged` alimentam HUD, achievements, telemetria etc.

---

## 🧮 Utilitários de Grid

- `SnapToGrid()`  
  - Converte posição atual do player em `gridPos` usando `WorldToGrid`.
  - Reposiciona o transform para o centro exato da célula.

- `WorldToGrid(Vector3 worldPos)`  
  - Usa `cellSize` para converter mundo → grid, com arredondamento.

- `GridToWorld(Vector2Int gp)`  
  - Usa `cellSize` para converter grid → mundo, preservando `z` atual.

---

## 🧱 Gizmos (Editor)

`OnDrawGizmosSelected()` desenha um pequeno **wire cube** em torno da posição do player quando selecionado, útil como debug visual do tile atual.

---

## ✅ Resumo

O `PlayerMover` é o núcleo da jogabilidade Sokoban:

- Input → Lógica de grid → Colisão/Push → Lerp/Animação → Histórico de Undo → Eventos globais.

Serve como ponto central para qualquer lógica de:

- Contagem de passos
- Achievements (“sem undo”, “sem empurrar caixa errada”)
- HUD (ícones de undo, número de movimentos etc.)

Ideal como “single source of truth” para movimento do jogador no seu projeto.