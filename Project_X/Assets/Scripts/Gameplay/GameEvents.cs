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