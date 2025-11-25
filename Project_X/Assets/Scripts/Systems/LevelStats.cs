using System;

/// <summary>
/// Estrutura simples e serializável que representa
/// as estatísticas acumuladas durante a run do nível atual.
///
/// Quem preenche?
///   → LevelRunTracker
///
/// Onde é usada?
///   → Achievements (para detectar performance)
///   → DebugAchievementSink (logs)
///   → WinChecker (para achievements ao completar nível)
///
/// Ela NÃO contém lógica.
/// Apenas armazena valores e facilita debug.
/// </summary>
[Serializable]
public struct LevelStats
{
    public int moves;       // Total de GameEvents.OnMove disparados.
    public int pushes;      // Total de GameEvents.OnPush (empurrões de caixas).
    public int undos;       // Quantidade de ações de UNDO executadas.
    public int restarts;    // Quantas vezes o jogador reiniciou o nível (OnRestart).
    public float timeSec;   // Tempo total da run desde o LoadLevel.

    /// <summary>
    /// Representação amigável para Debug.Log e sinks.
    /// Ex.: "moves=10, pushes=3, undos=1, restarts=0, time=24.5s"
    /// </summary>
    public override string ToString()
    {
        return 
            $"moves={moves}, pushes={pushes}, undos={undos}, " +
            $"restarts={restarts}, time={timeSec:F1}s";
    }
}