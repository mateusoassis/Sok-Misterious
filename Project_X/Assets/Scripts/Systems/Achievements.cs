using UnityEngine;

public interface IAchievementSink
{
    void OnLevelCompleted(int levelIndex, LevelStats stats);
    void OnUndoUsed(int levelIndex);
    void OnRestartUsed(int levelIndex);
}

/// <summary>
/// Facade estática. Gameplay chama Achievements.*
/// e a implementação concreta (Xbox, debug, etc.)
/// é plugada via SetSink.
/// </summary>
public static class Achievements
{
    static IAchievementSink _sink;

    public static IAchievementSink CurrentSink => _sink;

    public static void SetSink(IAchievementSink sink)
    {
        _sink = sink;
        Debug.Log($"[Achievements] Sink set to: {(_sink != null ? _sink.GetType().Name : "null")}");
    }

    static int GetCurrentLevelIndex()
    {
        var lm = LevelManager.Instance;
        return (lm != null) ? lm.currentIndex : -1;
    }

    public static void NotifyLevelCompleted(LevelStats stats)
    {
        _sink?.OnLevelCompleted(GetCurrentLevelIndex(), stats);
    }

    public static void NotifyUndoUsed()
    {
        _sink?.OnUndoUsed(GetCurrentLevelIndex());
    }

    public static void NotifyRestartUsed()
    {
        _sink?.OnRestartUsed(GetCurrentLevelIndex());
    }
}