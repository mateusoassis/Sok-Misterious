using UnityEngine;

public interface IAchievementSink
{
    void OnLevelCompleted(int levelIndex, LevelStats stats);  // quando o jogador finaliza um nível
    void OnUndoUsed(int levelIndex);                         // quando o jogador usa UNDO
    void OnRestartUsed(int levelIndex);                      // quando o jogador reinicia o nível
}

/// <summary>
/// Sistema de Achievements genérico:
/// - Gameplay chama Achievements.* (NotifyX)
/// - Implementação real (debug, Xbox, Steam, Analytics etc.)
///   é plugada via SetSink() em runtime.
/// 
/// Ou seja: este arquivo é a fachada, NÃO grava nada sozinho.
/// Apenas repassa eventos para o "sink" atual.
/// </summary>
public static class Achievements
{
    static IAchievementSink _sink;   // backend ativo (pode mudar conforme plataforma)

    // quem quiser inspecionar o sink corrente
    public static IAchievementSink CurrentSink => _sink;

    /// <summary>
    /// Registra o backend concreto.
    /// Ex.: Achievements.SetSink(new XboxSink()).
    /// </summary>
    public static void SetSink(IAchievementSink sink)
    {
        _sink = sink;
        Debug.Log($"[Achievements] Sink set to: {(_sink != null ? _sink.GetType().Name : "null")}");
    }

    // pega nível atual (helper)
    static int GetCurrentLevelIndex()
    {
        var lm = LevelManager.Instance;
        return (lm != null) ? lm.currentIndex : -1;
    }

    /// <summary>
    /// Chamado pelo WinChecker quando o jogador finaliza um nível.
    /// Passa índices + estatísticas de desempenho (LevelStats).
    /// </summary>
    public static void NotifyLevelCompleted(LevelStats stats)
    {
        _sink?.OnLevelCompleted(GetCurrentLevelIndex(), stats);
    }

    /// <summary>
    /// Chamado quando o jogador usa UNDO (PlayerMover).
    /// </summary>
    public static void NotifyUndoUsed()
    {
        _sink?.OnUndoUsed(GetCurrentLevelIndex());
    }

    /// <summary>
    /// Chamado quando o jogador reinicia o nível (Restart).
    /// </summary>
    public static void NotifyRestartUsed()
    {
        _sink?.OnRestartUsed(GetCurrentLevelIndex());
    }
}