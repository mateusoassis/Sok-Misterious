using UnityEngine;

/// <summary>
/// Implementação de debug para o sistema de Achievements.
/// Esta classe **não salva nada**, não integra com Xbox/Steam,
/// não faz upload de progresso ― ela **apenas dá Debug.Log**.
/// 
/// • Ideal para desenvolvimento local
/// • Mostra claramente quando eventos são disparados
/// • Ajuda a validar LevelStats, UNDO, Restart, etc.
/// 
/// Basta colocar esse componente em um GameObject
/// (ex.: "Managers") para ele interceptar todos os eventos.
/// </summary>
public class DebugAchievementSink : MonoBehaviour, IAchievementSink
{
    void OnEnable()
    {
        // Quando este objeto ativa, vira o sink oficial de achievements.
        Achievements.SetSink(this);
    }

    void OnDisable()
    {
        // Quando desativa, remove o sink (somente se ainda for o ativo).
        if (Achievements.CurrentSink == this)
            Achievements.SetSink(null);
    }

    // Chamado quando o jogador completa o nível.
    // "stats" contém informações como tempo, número de passos, pushes etc.
    public void OnLevelCompleted(int levelIndex, LevelStats stats)
    {
        Debug.Log($"[Achv] Level {levelIndex} COMPLETED → {stats}");
    }

    // Chamado sempre que o jogador usa UNDO em qualquer nível.
    public void OnUndoUsed(int levelIndex)
    {
        Debug.Log($"[Achv] UNDO usado no level {levelIndex}");
    }

    // Chamado sempre que o jogador usa RESTART.
    public void OnRestartUsed(int levelIndex)
    {
        Debug.Log($"[Achv] RESTART usado no level {levelIndex}");
    }
}