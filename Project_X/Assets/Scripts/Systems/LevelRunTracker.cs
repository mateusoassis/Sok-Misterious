using UnityEngine;

/// <summary>
/// Acompanha estatísticas da "run" do nível atual:
/// - moves/pushes/undos/restarts
/// - tempo desde o LoadLevel
/// 
/// Ele NÃO salva nada em disco, só mantém o snapshot em memória
/// pra Achievements/Debug.
/// </summary>
public class LevelRunTracker : MonoBehaviour
{
    public static LevelRunTracker Instance { get; private set; }

    LevelStats current;
    float levelStartTime;

    void Awake()
    {
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
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
        GameEvents.OnMove        += HandleMove;
        GameEvents.OnPush        += HandlePush;
        GameEvents.OnUndo        += HandleUndo;
        GameEvents.OnRestart     += HandleRestart;
    }

    void OnDisable()
    {
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnMove        -= HandleMove;
        GameEvents.OnPush        -= HandlePush;
        GameEvents.OnUndo        -= HandleUndo;
        GameEvents.OnRestart     -= HandleRestart;
    }

    void HandleLevelLoaded()
    {
        current = new LevelStats();
        levelStartTime = Time.time;
        Debug.Log("[LevelRunTracker] Novo nível carregado → stats zerados.");
    }

    void HandleMove()   { current.moves++;   }
    void HandlePush()   { current.pushes++;  }
    void HandleUndo()   { current.undos++;   Achievements.NotifyUndoUsed(); }
    void HandleRestart(){ current.restarts++;Achievements.NotifyRestartUsed(); }

    public LevelStats GetSnapshot()
    {
        if (levelStartTime > 0f)
            current.timeSec = Time.time - levelStartTime;
        else
            current.timeSec = 0f;
        return current;
    }
}