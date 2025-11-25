# GameEvents

## Visão Geral
`GameEvents` é o **hub global de eventos do jogo**.  
Ele funciona como um mini “EventBus” estático, permitindo que gameplay, HUD e sistemas de nível se comuniquem sem dependências diretas.

Ele expõe eventos para:
- Movimento do player  
- Empurrões  
- Undo  
- Restart de nível  
- Level Loaded  
- Mudanças nos Goals  
- Consulta global de “quantos goals faltam?”  

O padrão funciona muito bem para jogos de grade como Sokoban, pois garante **isolamento de responsabilidades** sem amarrar scripts entre si.

---

## Por que usar este padrão?
- Evita referência direta entre sistemas (HUD ↔ LevelManager ↔ PlayerMover).  
- A comunicação acontece via “pub/sub”: cada script assina apenas o evento que usa.  
- Componentes podem ser adicionados ou removidos sem quebrar outros sistemas.  
- Facilita achievements, telemetria, HUD, lógica de vitória, etc.

---

## Eventos disponíveis

### 🟦 **OnMove**
Chamado quando o player realiza um movimento simples.

### 🟧 **OnPush**
Chamado quando o player empurra uma caixa (movimento especial).

### 🟨 **OnUndo**
Chamado quando o jogador desfaz um movimento.

### 🟩 **OnRestart**
Chamado sempre que o nível é reiniciado (`LevelManager.Reload()`).

### 🟪 **OnLevelLoaded**
Chamado quando um nível é instanciado ou recarregado (inclusive via `OnSceneLoaded`).

### 🟫 **OnGoalsMaybeChanged**
Chamado sempre que:
- caixa entra/sai de um goal  
- undo afeta estado das caixas  
- restart  
- vitória  
- qualquer situação que possa alterar o número de goals restantes  

É um evento genérico, mas extremamente útil para HUD e lógica de vitória.

---

## Função especial

### **Func<int> GetGoalsLeft**
HUD e outros sistemas podem pedir uma função para ser usada quando for necessário saber **quantos goals ainda faltam**.

O LevelManager (ou outro controlador responsável) configura isso via:

```csharp
GameEvents.SetGoalsLeftProvider(() => goalsRestantes);
```

Depois qualquer parte do jogo pode consultar:

```csharp
int faltando = GameEvents.GetGoalsLeft?.Invoke() ?? 0;
```

---

## Métodos Helper

### RaiseMove()
Dispara `OnMove`.

### RaisePush()
Dispara `OnPush`.

### RaiseUndo()
Dispara `OnUndo`.

### RaiseRestart()
Dispara `OnRestart`.

### RaiseLevelLoaded()
Dispara `OnLevelLoaded`.

### RaiseGoalsMaybeChanged()
Dispara `OnGoalsMaybeChanged`.

### SetGoalsLeftProvider(Func<int> provider)
Configura a função que será usada para retornar o número de goals faltando.

---

## Fluxo típico de uso

### 🧩 PlayerMover
- Move o player  
- Se empurrar uma caixa → `RaisePush()`  
- Caso contrário → `RaiseMove()`  
- Se o jogador apertar Undo → `RaiseUndo()`

### 🧩 LevelManager
- Ao carregar ou recarregar nível → `RaiseLevelLoaded()`  
- Ao aplicar highlight / verificar goals → `RaiseGoalsMaybeChanged()`  

### 🧩 HUD / UI
- Assina `OnGoalsMaybeChanged` para atualizar indicadores  
- Usa `GetGoalsLeft` para saber quantos restam  
- Assina `OnMove` / `OnPush` para animar contadores  

---

## Código original

```csharp
using System;

public static class GameEvents
{
    // Disparados quando o player anda / empurra
    public static event Action OnMove;
    public static event Action OnPush;

    // Undo (não conta como Move, mas pode interessar pra achievements/telemetria)
    public static event Action OnUndo;

    // Restart de nível (LevelManager.Reload chama isso)
    public static event Action OnRestart;

    // Carregou/recarregou um nível (LevelManager)
    public static event Action OnLevelLoaded;

    // HUD pergunta “quantos goals faltam?”
    public static Func<int> GetGoalsLeft;

    // Goals podem ter mudado (box entrou/saiu de goal, undo, restart, vitória…)
    public static event Action OnGoalsMaybeChanged;

    // --------- Helpers ---------

    public static void RaiseMove()           => OnMove?.Invoke();
    public static void RaisePush()           => OnPush?.Invoke();
    public static void RaiseUndo()           => OnUndo?.Invoke();
    public static void RaiseRestart()        => OnRestart?.Invoke();
    public static void RaiseLevelLoaded()    => OnLevelLoaded?.Invoke();
    public static void RaiseGoalsMaybeChanged() => OnGoalsMaybeChanged?.Invoke();

    public static void SetGoalsLeftProvider(Func<int> provider)
        => GetGoalsLeft = provider;
}
```