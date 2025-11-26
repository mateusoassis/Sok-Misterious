## README Variante 1

# Sokoban (Unity 6000.0.58f1 · Xbox · 720p · Pixel Art)

Um puzzle de grade 1×1 no estilo **Sokoban**, com foco em UX limpa, progresso simples por **nível mais alto desbloqueado** e telemetria básica pensada para **conquistas (Xbox)** no futuro.

> **Estado atual (MVP jogável)**  
> - Player com **movimento em grid** (lerp), **push de caixa**, **UNDO**, **restart**, **pause**, **win-check** e **level select** com progresso salvo.  
> - **HUD** com Moves/Pushes/Goals restantes.  
> - **Dev Tools** (overlay + hotkeys) e **validador de nível** (bounds, boxes/goals, fora de grid).  
> - **Validação em lote** de todos os levels por menu de Editor (sem snap).  
> - Sprites placeholders (pato pai, patinho, travesseiro/goal). FlipX no player.

---

## Tabela de Conteúdos
- [Plataforma & Resolução](#plataforma--resolução)
- [Loop de Jogo](#loop-de-jogo)
- [Controles](#controles)
- [HUD & UI](#hud--ui)
- [Arquitetura](#arquitetura)
  - [Scenes](#scenes)
  - [Estrutura de Projeto](#estrutura-de-projeto)
  - [Sistemas](#sistemas)
- [Dados & Salvamento](#dados--salvamento)
- [Validador de Levels](#validador-de-levels)
- [Ferramentas de Dev](#ferramentas-de-dev)
- [Arte & Placeholders](#arte--placeholders)
- [Como Rodar](#como-rodar)
- [Como Editar / Criar Fases](#como-editar--criar-fases-mini-tutorial-para-gd)
- [Roadmap](#roadmap)
- [To-Do (curto prazo)](#to-do-curto-prazo)
- [Issues Conhecidas](#issues-conhecidas)
- [Padrões & Notas](#padrões--notas)

---

## Plataforma & Resolução
- **Plataforma alvo**: **Xbox** (PC durante dev).
- **Resolução base**: **1280×720 (720p)**, pixel art.
- **Grid**: 1 unidade = 1 célula (ex.: 32px por unidade, PPU 32).

## Loop de Jogo
1. **Selecionar nível** (apenas os desbloqueados).
2. **Empurrar caixas** até cobrir todos os **Goals**.
3. Ao vencer:
   - **Desbloqueia** o próximo nível (progresso = maior índice alcançado).
   - Mostra **Victory Panel** com **Next** / **Restart**.
4. **HUD** rastreia **Moves**, **Pushes** e **Goals restantes** (por nível).
5. **UNDO** volta o último passo/push (teleporte lógico, sem animar volta).

> **Sem estrelas/performance** por nível. Métricas são pensadas para futuras **conquistas** (Xbox).

## Controles
- **Mover**: WASD / Setas / D-Pad / Left Stick (4 direções).
- **Empurrar**: andar contra a caixa (se célula atrás estiver livre).
- **Undo**: `Z` (teclado) / **Y** (botão Norte).
- **Pause**: `Esc` / **Start`.
- **Victory**: **C** (Next) / **X** (Restart).

**Dev (Editor/Dev build)**
- **F1**: alterna overlay Dev.
- **F5**: reload (nível atual).
- **F6**: próximo nível.
- **F7**: nível anterior.
- **F8**: force win.

## HUD & UI
- **HUD** (in-game): `Moves`, `Pushes`, `Goals restantes`.
- **Pause Panel**: `Resume`, `Restart`, `Level Select`.
- **Victory Panel**: `Next`, `Restart`.
- **Debug buttons** (somente dev): _Reset Highest_ / _Reset PlayerPrefs_.

---

## Arquitetura

### Scenes
```
00_MainMenu
01_LevelSelect
02_Game
```

### Estrutura de Projeto
```
Assets/
│
├── Animation/
│   └── Player/
│
├── Art/
│   ├── Characters/
│   ├── Fonts/
│   ├── Placeholders/
│   ├── Tiles/
│   └── UI/
│
├── Audio/
│   ├── Music/
│   └── SFX/
│
├── Data/
│
├── Editor/
│
├── Levels/
│
├── Prefabs/
│   ├── Levels/
│   ├── Player/
│   ├── Tiles/
│   └── Scenes/
│
├── Presets/
│
├── Scripts/
│   ├── Boot/            ← inicialização, LaunchArgs, Loaders
│   ├── Core/            ← sistemas centrais (LevelList, LevelManager, GameEvents…)
│   ├── Data/            ← estruturas de dados, LevelStats, SaveManager…
│   ├── Gameplay/        ← PlayerMover, Box, Goals, WinChecker…
│   ├── Systems/         ← Achievements, DeviTools, Snapper…
│   └── TestStuff/       ← scripts auxiliares, debug, ferramentas internas
│       └── UI/
│
│── Tiles/
│
└── UI/ (não existe como pasta, UI fica espalhada entre Prefabs + Scripts/UI)
```

### Sistemas

#### LevelList
- Asset/Scriptable com array de `levelPrefab` (+ `displayName`).  
- O `LevelManager` lê daqui para `LoadLevel(i)`/`LoadNext()`.

#### LevelManager
- `LoadLevel(i)`: instancia o prefab do nível, resolve **Bounds** (BoxCollider2D) e foca a câmera.
- Mantém `currentLevel` e `currentIndex`.
- `Reload()` e `LoadNext()`.
- Em `02_Game`, se nada pendente, carrega `index=0`.
- **Evento**: `GameEvents.RaiseLevelLoaded()`.

#### LaunchArgs
- `PendingLevel` (int?) para transicionar do **Level Select** para `02_Game`.

#### SaveManager
- PlayerPrefs key: `"highestUnlocked"`.
- Métodos:
  - `GetHighestUnlockedIndex()`
  - `UnlockUpTo(index)`
  - `ResetAll()` / `ResetHighest()` (debug)
- Convenção: índice 0 = primeiro nível.

#### GameEvents (estático)
- `OnMove`, `OnPush`, `OnUndo`, `OnLevelLoaded`, `OnGoalsMaybeChanged`.
- `GetGoalsLeft` (provider setado pelo `WinChecker`).
- Helpers `Raise*()`.

#### LevelRunTracker
- Contadores por nível: `moves`, `pushes`, `undos`, `restarts`, `timeSec`.
- Zera ao `LevelLoaded`.
- Exposto no overlay.

#### AchievementsDebugSink
- Placeholder para futuras integrações (Xbox). Hoje só loga em `LevelLoaded`/`Victory`.

#### WinChecker
- Observa **Goals** (via `GoalIdentifier`) e **Boxes** (`BoxIdentifier`) usando `OverlapPoint`.
- Usa `solidMask` (mesma do Player) para garantir que só uma **Box** conta no goal.
- Ao win:
  - `SaveManager.UnlockUpTo(currentIndex + 1)`,
  - `VictoryUI.Show()`,
  - `GameEvents.RaiseGoalsMaybeChanged()`.

#### PlayerMover
- Grid 1×1, **lerp** com `PlayerLerpMotion`.
- Push de caixa se célula atrás estiver livre; respeita **Bounds**.
- **Undo** com histórico (`MoveRecord` para player + box).
- Repeat opcional com `firstRepeatDelay`/`repeatInterval`.
- Integra com `GameEvents` (Move/Push/Undo) e **HUD**.

#### PlayerLerpMotion
- Corotina `MoveBy(dir, onDone)` com duração configurável (ex.: `0.20s`).
- `IsMoving`, `CancelAndSnap(pos)` para Undo.

#### PlayerAnimatorBridge
- Seta `isMoving`, `isPushing`, `dirX/dirY`.
- **FlipX** opcional, com “memorizar última direção horizontal”.

#### BoxIdentifier / BoxLerpMotion / BoxGoalState
- `BoxGoalState.SetOnGoal(bool)` troca **sprite** do patinho quando em goal.
- `BoxLerpMotion` anima a caixa ao ser empurrada.

> **Nota**: o sub de highlight “UnderSprite” foi aposentado (usamos troca de sprite).

#### HUDController
- Escuta eventos e mostra `Moves`, `Pushes`, `Goals restantes` (`GetGoalsLeft`).  
- Zera contadores visuais em `OnLevelLoaded`.

#### VictoryUI / PauseMenu
- Canvases com `CanvasGroup` (show/hide) sem bloquear input global.
- Victory: `Show()`/`Hide()`, Next/Restart.
- Pause: `Toggle()` com Start/Esc; `useTimeScalePause` opcional; trava input do Player.

---

## Dados & Salvamento
- **PlayerPrefs**
  - `highestUnlocked` (int) – maior índice desbloqueado.
- **Progressão**
  - Ao vencer o nível `i`, desbloqueia `i+1` (se existir).
  - Level Select habilita botões até `highestUnlocked`.
- **Debug**
  - Botões na cena de jogo: _Reset Highest_ / _Reset PlayerPrefs_.

---

## Validador de Levels
**LevelValidator** (na raiz de cada `Level_XX`)
- Verifica:
  - Existe **`Bounds`** com **`BoxCollider2D`**?
  - Há **Goals** e **Boxes**?
  - Boxes/Goals **dentro dos Bounds**?
  - **Alinhamento ao grid local** (posição múltipla de `cellSize`)? → *WARNING* se fora do grid.
- **Snap opcional** ao grid (localPosition):
  - **Desligado por padrão**.
  - Ignora **root** do level e o objeto **`Bounds`**.
- **Menu Editor (em lote, sem snap):**
  - `Tools → Levels → Validate All Levels (no snap)` instancia cada prefab e valida (evita falsos positivos).

---

## Ferramentas de Dev
**Overlay (F1)**
- Mostra:
  - `Level: index (name)`
  - `HighestUnlocked`
  - `Moves | Pushes | Undos | Restarts | Time`
  - Ajuda: `F1/F5/F6/F7/F8`
- Fail-safe: overlay não quebra se algum serviço estiver ausente.

**Hotkeys**
- **F1**: toggle overlay  
- **F5**: reload  
- **F6**: next  
- **F7**: prev  
- **F8**: force win

---

## Arte & Placeholders
- **Sprites temporários**:
  - Player (“pato pai”): idle/walk simples no Animator; **FlipX** ligado.
  - Box (“patinho”): troca sprite quando em **Goal** (via `BoxGoalState`).
  - Goal (“travesseiro”).
- **Import settings** serão refinados quando a arte oficial chegar.
- **Sorting Layers** já definidas (HUD, Entities, Tiles…).

---

## Como Rodar
- **Unity**: versão do projeto (2022/2023 LTS — ajustar conforme repo).
- Abrir `00_MainMenu` e dar Play.
- Construção: PC Standalone durante dev; alvo final Xbox (720p).

---

## Como Editar / Criar Fases (Mini-tutorial para GD)

Os layouts de cenário são feitos com **Tilemap** a partir de um prefab de grid base.  
Nada de grid vem “pronto” dentro dos levels – o GD é quem adiciona o grid em cada fase.

### Onde estão as coisas

- **Levels (fases):**
  ```
  Assets/Prefabs/Levels/
  ```

  Exemplos: `Level_01`, `Level_02`, etc.  
  Cada level é um prefab com algo como:

  ```
  LevelRoot
     └── Bounds (BoxCollider2D)
  ```

- **Grid base + Tilemap (pra pintar):**
  ```
  Assets/Prefabs/Tiles/Prefab_Base_Grid
  ```

  Esse prefab contém:

  ```
  Prefab_Base_Grid
     ├── Grid
     │    └── Tilemap_Palette_Target   ← onde você vai pintar os tiles
     └── (outros componentes auxiliares, se houver)
  ```

- **Tile Palette / Tiles:**
  - Palette configurada para o projeto (pixel art 32×32)
  - Tiles salvos em pastas de tiles (ex.: `Art/Tiles` ou `Tiles/...`)

---

### Passo a passo (GD)

#### 1) Abrir um level

1. No Project, vá em:
   ```
   Assets/Prefabs/Levels/
   ```
2. Dê **duplo clique** no prefab do level que você quer editar  
   **ou** arraste o prefab para a cena.

> O layout (desenho dos tiles) será salvo **na cena**, não no prefab base.

---

#### 2) Adicionar o Grid base (se ainda não tiver)

1. No Project, vá em:
   ```
   Assets/Prefabs/Tiles/Prefab_Base_Grid
   ```
2. Arraste o `Prefab_Base_Grid` para dentro do `LevelRoot` na Hierarchy.
3. Garanta que o objeto `Prefab_Base_Grid` (ou `Grid`) esteja em `(0, 0, 0)`,
   a menos que a fase peça um offset específico.

Agora o level deve ficar algo assim:

```text
LevelRoot
   ├── Bounds (BoxCollider2D)
   └── Prefab_Base_Grid (instância)
        └── Grid
             └── Tilemap_Palette_Target
```

> **Importante:** o GD **não precisa** dar *Apply* neste prefab. O grid base é só uma “ferramenta” para pintar o mapa dentro de cada fase.

---

#### 3) Abrir a Tile Palette

1. Selecione o objeto `Tilemap_Palette_Target` na Hierarchy (dentro do `Grid`).
2. No Inspector, clique em **Open Tile Palette**.

Se o botão não aparecer, você também pode abrir pela barra de menu:

```text
Window → 2D → Tile Palette
```

Na janela da Tile Palette, selecione a palette do projeto (se ainda não estiver selecionada).

---

#### 4) Pintar o cenário

1. Na janela **Tile Palette**, clique no tile que você quer usar.
2. Com o `Tilemap_Palette_Target` selecionado, use o brush na **Scene View**:

   - **Botão esquerdo do mouse** → pinta o tile.
   - **Botão direito** → apaga o tile.

O Unity já faz o **snap automático pro grid** (32×32 / 1×1 unidade).

> Dica: se pintar errado, use `Ctrl+Z` ou apague com o brush mesmo.

---

#### 5) Salvar

- Só precisa **salvar a cena** (`Ctrl+S`).  
- **Não clique em Apply** no prefab do level a menos que você tenha alterado só estrutura (ex.: adicionou/removeu algum objeto fixo que deve ser igual em todos os níveis).  
- O layout de tiles é específico daquela cena/level.

---

#### 6) Criar um level novo

1. Duplique um prefab existente em:
   ```
   Assets/Prefabs/Levels/
   ```
2. Renomeie (ex.: `Level_03`, `Level_04`).
3. Abra o novo level (duplo clique ou arrastar para cena).
4. Adicione o `Prefab_Base_Grid` (se ainda não estiver presente).
5. Abra a Tile Palette e **pinte seu layout** no `Tilemap_Palette_Target`.
6. Se quiser, rode a validação:

   ```text
   Tools → Levels → Validate Level
   ```

---

## Roadmap
- **Integração de arte oficial**
  - Player: idle/walk/push/pause…
  - Box: variações, squash sutil ao empurrar, feedbacks.
  - Goals: VFX sutil ao completar todos.
- **Achievements (Xbox)**
  - Mapear métricas do `LevelRunTracker` (moves/pushes/undos/restarts/time).
  - `AchievementsSink` (classe final) com eventos/milestones.
- **LevelData / Editor QoL**
  - Scriptable central de tileset/atlas e parâmetros de grid por tema.
  - Prefab Variants por tema + ferramentas de auto-preenchimento.
- **Ajustes de “feel”**
  - Curva/tween do lerp (ease in/out).
  - Velocidade configurável (toggle em Dev Tools).
  - SFX básicos (passo/push/goal/vitória).

---

## To-Do (curto prazo)
- [ ] Revisar grid dos 5 níveis atuais com **Validate Level** (sem snap).
- [ ] Consolidar **Input System** (bindings/repeat).
- [ ] Confirmar **solidMask** uniforme para Player/WinChecker/Boxes.
- [ ] Exportar cenas/prefabs com debug buttons apenas para _Dev_ (símbolo de compilação).
- [ ] Criar `/Docs/Scripts` e colar cada script com descrição (próxima etapa).
- [ ] Integrar assets de tileset já entregues por artista.

---

## Issues Conhecidas
- Undo durante movimento pode exigir cancel/snap (mitigado com `motion.IsMoving` e `CancelAndSnap`).
- `Bounds` precisa estar corretamente nomeado e conter **BoxCollider2D**.
- Se um prefab não tiver `LevelValidator`, o **Validate All** loga aviso (não falha o pipeline).

---

## Padrões & Notas
- **CellSize**: 1.0 (grid 1×1).  
- **Alinhamento ao grid**: `Validate Level` apenas **avisa**. Snap é **manual**.  
- **PlayerPrefs**: `highestUnlocked` salva o maior índice alcançado.  
- **Undo**: teleporte por design (QoL).  
- **Eventos**: `GameEvents` centralizam a comunicação (HUD/Win/Tracker etc.).

> **Contribuição**: abriremos `/Docs/Scripts` e linkaremos daqui para cada script com “O que faz / Pontos importantes / API rápida”.