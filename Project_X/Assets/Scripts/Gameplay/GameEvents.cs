using System;

public static class GameEvents
{
    // Disparados quando o player anda / empurra (Undo NÃO dispara nada)
    public static event Action OnMove;
    public static event Action OnPush;

    // Opcional: resetar HUD quando um nível carrega/recarrega
    public static event Action OnLevelLoaded;

    // HUD pergunta “quantos goals faltam?”
    public static Func<int> GetGoalsLeft;

    // Helpers pra disparar com null-check
    public static void RaiseMove() => OnMove?.Invoke();
    public static void RaisePush() => OnPush?.Invoke();
    public static void RaiseLevelLoaded() => OnLevelLoaded?.Invoke();

    // Registrar um provider pra GoalsLeft (WinChecker faz isso no Awake)
    public static void SetGoalsLeftProvider(Func<int> provider) => GetGoalsLeft = provider;
}