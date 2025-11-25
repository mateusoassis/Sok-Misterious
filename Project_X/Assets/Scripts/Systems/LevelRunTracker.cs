using UnityEngine;

/// <summary>
/// Rastreador de estatísticas da “run” do nível atual.
///
/// O que ele registra:
///  • Quantidade de movimentos (moves)
///  • Quantidade de pushes (quando o player empurra caixa)
///  • Quantidade de UNDOs
///  • Quantidade de RESTARTs
///  • Tempo total desde o carregamento do nível
///
/// Ele NÃO salva nada em disco.
/// Funciona somente em runtime e serve como fonte
/// para Achievements, DebugAchievementSink e logs.
/// </summary>
public class LevelRunTracker : MonoBehaviour
{
    // Singleton persistente que acompanha a run atual
    public static LevelRunTracker Instance { get; private set; }

    // Estrutura com counters da run atual
    LevelStats current;

    // Guarda o tempo de início do nível atual
    float levelStartTime;

    void Awake()
    {
        // Singleton padrão para evitar instâncias duplicadas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // Escuta os eventos principais do jogo
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
        GameEvents.OnMove        += HandleMove;
        GameEvents.OnPush        += HandlePush;
        GameEvents.OnUndo        += HandleUndo;
        GameEvents.OnRestart     += HandleRestart;
    }

    void OnDisable()
    {
        // Desinscrição limpa para evitar memory leaks
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnMove        -= HandleMove;
        GameEvents.OnPush        -= HandlePush;
        GameEvents.OnUndo        -= HandleUndo;
        GameEvents.OnRestart     -= HandleRestart;
    }

    /// <summary>
    /// Chamado quando um novo nível é carregado.
    /// Reset total: zera counters e registra o novo Time.time.
    /// </summary>
    void HandleLevelLoaded()
    {
        current = new LevelStats();
        levelStartTime = Time.time;

        Debug.Log("[LevelRunTracker] Novo nível carregado → stats zerados.");
    }

    /// <summary>Incrementa contador de movimentos.</summary>
    void HandleMove()   { current.moves++; }

    /// <summary>Incrementa contador de pushes.</summary>
    void HandlePush()   { current.pushes++; }

    /// <summary>
    /// Incrementa UNDOs e notifica Achievements.
    /// Isso permite achievements do tipo “terminar nível sem undo”.
    /// </summary>
    void HandleUndo()
    {
        current.undos++;
        Achievements.NotifyUndoUsed();
    }

    /// <summary>
    /// Incrementa restarts e avisa Achievements.
    /// Usado para achievements de “zerar sem restart”.
    /// </summary>
    void HandleRestart()
    {
        current.restarts++;
        Achievements.NotifyRestartUsed();
    }

    /// <summary>
    /// Retorna um snapshot atualizado de LevelStats.
    /// Atualiza timeSec dinamicamente com base no Time.time atual.
    /// </summary>
    public LevelStats GetSnapshot()
    {
        if (levelStartTime > 0f)
            current.timeSec = Time.time - levelStartTime;
        else
            current.timeSec = 0f;

        return current;
    }
}