# WinChecker — Lógica de Vitória do Sokoban

Este componente é responsável por **detectar quando o jogador completou o nível** (todas as caixas estão em cima dos goals), exibir a tela de vitória, liberar o próximo nível e integrar com achievements/telemetria.

Ele funciona em conjunto com:
- **LevelManager**
- **GameEvents**
- **PlayerMover**
- **GoalIdentifier / BoxIdentifier**
- **VictoryUI**
- **SaveManager**
- **LevelRunTracker / Achievements**

---

## 📌 Objetivo

Garantir que o jogo só considere a vitória quando:

1. O nível estiver completamente carregado.  
2. O jogador tiver realmente feito uma jogada válida (andar ou empurrar).  
3. **TODOS os goals estiverem cobertos por caixas**.

Quando isso acontece:
- A UI de vitória é exibida.
- O próximo nível é desbloqueado (SaveManager).
- Estatísticas são enviadas ao sistema de achievements.
- O jogador pode reiniciar, avançar ou usar cheat de vitória (apenas dev).

---

## ⚙️ Funcionamento Geral

### 1. Assinaturas de eventos  
Ao iniciar, o WinChecker registra handlers para:

- **OnLevelLoaded** → prepara o sistema para checar vitória.  
- **OnMove** e **OnPush** → só deixa o jogo considerar vitória após uma ação real do jogador.

Isso impede “vitória fantasma” ao carregar a cena.

### 2. Armar a checagem  
Depois que o nível é carregado, ele espera:

- 1 frame (para sincronizar colliders)
- +0.05s (para garantir que tudo está posicionado)
- Player se mover OU Push acontecer

Só então a checagem começa.

### 3. Verificação de vitória  
A cada frame (Update):

- Se **canCheck** e **armedForVictory**, verifica se **AllGoalsHaveBoxes()**.
- Se sim → vitória.

### 4. Vitória  
Quando `won = true`:

- Envia métricas para achievements.  
- Chama `TryUnlockNextLevel()`.  
- Notifica HUD via `GameEvents.RaiseGoalsMaybeChanged()`.  
- Exibe tela de vitória (VictoryUI.Show).  

---

## 🎯 Como ele conta os goals

O método usa o `LevelManager.currentLevel` para garantir que **só os goals do nível atual** sejam verificados.

Para cada Goal:
- Faz `Physics2D.OverlapPoint` no centro do goal, usando `solidMask`.
- Verifica se há uma BoxIdentifier naquele ponto.

Se **qualquer** goal não tiver caixa → ainda não venceu.

---

## 🧩 Inputs de Jogador

O WinChecker também escuta input:

### Reiniciar
- **X** no teclado  
- **A** (button South) no gamepad

Chama `lm.Reload()`.

### Próximo nível
- **C** no teclado  
- **Right Shoulder (RB)** no gamepad

Chama `lm.LoadNext()`.

---

## 🔐 Sistema de Progresso

Ao vencer:
- `SaveManager.UnlockUpTo(nextIndex)` é chamado.
- Isso desbloqueia o próximo nível persistentemente.

---

## 🛠️ Integração com Achievements

Quando ganha:

```csharp
var stats = LevelRunTracker.Instance.GetSnapshot();
Achievements.NotifyLevelCompleted(stats);
```

Ou seja:
- Monitora tempo do nível
- Contagem de passos
- Pushes
- Undo
- etc.

É opcional mas recomendável.

---

## 🧪 Cheat para Dev

Disponível apenas em Unity Editor ou Development Build:

```csharp
ForceWinCheat()
```

A vitória é ativada mesmo sem cobrir todos os goals.

---

## 🗂️ Estados Internos Importantes

| Variável | Função |
|---------|--------|
| `canCheck` | Só permite checagem após LevelLoaded + delay |
| `armedForVictory` | Só permite vitória depois de um movimento real |
| `won` | Evita executar vitória mais de uma vez |
| `solidMask` | LayerMask usada para detectar caixas sobre goals |

---

## 🔗 Dependências Obrigatórias

- LevelManager com level instanciado.
- Colliders das caixas devem estar em `solidMask`.
- Cada caixa deve ter `BoxIdentifier`.
- Cada goal deve ter `GoalIdentifier`.
- VictoryUI opcional (mas recomendado).

---

## 📝 Resumo em 20 segundos (para onboarding)

- Espera o nível carregar → sincroniza colliders.  
- Só passa a checar vitória quando o jogador fizer algo.  
- Verifica se cada Goal tem uma Box acima.  
- Mostra tela de vitória, salva progresso e ativa achievements.  
- Inputs X/C reiniciam ou vão para o próximo nível.  

Simples, robusto e seguro contra triggers falsos.