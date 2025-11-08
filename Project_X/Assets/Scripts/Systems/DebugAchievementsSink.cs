using UnityEngine;

/// <summary>
/// Implementação de debug: só dá Debug.Log.
/// Coloca num GameObject (por exemplo no Managers) pra já ver os eventos.
/// </summary>
public class DebugAchievementSink : MonoBehaviour, IAchievementSink
{
    void OnEnable()
    {
        Achievements.SetSink(this);
    }

    void OnDisable()
    {
        if (Achievements.CurrentSink == this)
            Achievements.SetSink(null);
    }

    public void OnLevelCompleted(int levelIndex, LevelStats stats)
    {
        Debug.Log($"[Achv] Level {levelIndex} COMPLETED → {stats}");
    }

    public void OnUndoUsed(int levelIndex)
    {
        Debug.Log($"[Achv] UNDO usado no level {levelIndex}");
    }

    public void OnRestartUsed(int levelIndex)
    {
        Debug.Log($"[Achv] RESTART usado no level {levelIndex}");
    }
}