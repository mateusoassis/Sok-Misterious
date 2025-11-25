using System;

// Hub global de eventos usado pelo jogo inteiro.
// Segue um padrão de EventBus simples: scripts publicam eventos e outros scripts assinam.
public static class GameEvents
{
    // Disparado quando o player realiza um movimento normal (andar).
    public static event Action OnMove;

    // Disparado quando o player empurra uma caixa (movimento especial).
    public static event Action OnPush;

    // Disparado quando o jogador realiza Undo.
    // Não é movimento, mas ainda pode ser útil para achievements/telemetria.
    public static event Action OnUndo;

    // Disparado quando o LevelManager reinicia o nível.
    public static event Action OnRestart;

    // Disparado sempre que um nível é carregado ou recarregado.
    public static event Action OnLevelLoaded;

    // Delegate especial: HUD e outros sistemas perguntam quantos goals faltam.
    // É configurado por quem controla a lógica de goals (ex.: um GoalsManager).
    public static Func<int> GetGoalsLeft;

    // Disparado sempre que o estado dos goals PODE ter mudado:
    // - caixa entrando/saindo de goal
    // - undo
    // - restart
    // - vitória
    public static event Action OnGoalsMaybeChanged;

    // --------- Métodos Helper para disparar eventos ---------

    public static void RaiseMove()               => OnMove?.Invoke();
    public static void RaisePush()               => OnPush?.Invoke();
    public static void RaiseUndo()               => OnUndo?.Invoke();
    public static void RaiseRestart()            => OnRestart?.Invoke();
    public static void RaiseLevelLoaded()        => OnLevelLoaded?.Invoke();
    public static void RaiseGoalsMaybeChanged()  => OnGoalsMaybeChanged?.Invoke();

    // Define o provedor da função que retorna quantos goals faltam.
    public static void SetGoalsLeftProvider(Func<int> provider)
        => GetGoalsLeft = provider;
}