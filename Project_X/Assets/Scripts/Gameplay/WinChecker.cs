using UnityEngine;
using UnityEngine.InputSystem;

public class WinChecker : MonoBehaviour
{
    [Header("Mesma LayerMask escolhida no Player")]
    public LayerMask solidMask;

    // inputs para NEXT/RESTART (teclado e gamepad)
    private InputAction restartAction; // X teclado / Sul(A) gamepad
    private InputAction nextAction;    // C teclado / Right Shoulder

    private bool canCheck;   // só checa vitória quando o level estiver pronto
    private bool won;
    private bool armedForVictory;


    private VictoryUI victoryUI;

    // guardamos o delegate pra desinscrever corretamente
    private System.Action onLevelLoadedHandler;
    private System.Action onMoveHandler;
    private System.Action onPushHandler;

    private void Awake()
    {
        // Bindings
        restartAction = new InputAction("Restart", InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/x");
        restartAction.AddBinding("<Gamepad>/buttonSouth");

        nextAction = new InputAction("Next", InputActionType.Button);
        nextAction.AddBinding("<Keyboard>/c");
        nextAction.AddBinding("<Gamepad>/rightShoulder");

        restartAction.Enable();
        nextAction.Enable();

        // Provider para HUD (Goals restantes)
        GameEvents.SetGoalsLeftProvider(GetGoalsLeft);

        // VictoryUI cache (pode ser null na primeira chamada)
        victoryUI = VictoryUI.Instance;

        // escuta "level carregado" do LevelManager (um handler único, sem lambda anônima)
        onLevelLoadedHandler = HandleLevelLoaded;
        GameEvents.OnLevelLoaded += onLevelLoadedHandler;

        // trigga por input real (move/push), pra evitar vitória fantasma
        onMoveHandler = () => armedForVictory = true;
        onPushHandler = () => armedForVictory = true;
        GameEvents.OnMove += onMoveHandler;
        GameEvents.OnPush += onPushHandler;
    }

    private void OnEnable()
    {
        won = false;
        canCheck = false;
        armedForVictory = false;

        // Se por algum motivo o level já estiver carregado quando este componente habilitar,
        // arma manualmente (sem depender do evento que pode já ter passado).
        var lm = LevelManager.Instance;
        if (lm != null && lm.currentLevel != null)
        {
            // faz o mesmo que HandleLevelLoaded: arma no próximo frame
            StartCoroutine(ArmCheckNextFrame());
        }  // espera o LevelLoaded sinalizar
    }

    private void OnDestroy()
    {
        restartAction.Disable();
        nextAction.Disable();

        if (GameEvents.GetGoalsLeft == GetGoalsLeft)
        {
            GameEvents.SetGoalsLeftProvider(null);
        }

        if (onLevelLoadedHandler != null)
        {
            GameEvents.OnLevelLoaded -= onLevelLoadedHandler;
        }
        if (onMoveHandler != null)
        {
            GameEvents.OnMove -= onMoveHandler;
        }
        if (onPushHandler != null)
        {
            GameEvents.OnPush -= onPushHandler;
        }
    }

    private void HandleLevelLoaded()
    {
        // chamado pelo LevelManager no fim do LoadLevel
        won = false;
        canCheck = false;
        armedForVictory = false;
        Debug.Log("[WinChecker] HandleLevelLoaded chamado");

        // espera 1 frame pra garantir colliders atualizados
        StartCoroutine(ArmCheckNextFrame());
    }

    private System.Collections.IEnumerator ArmCheckNextFrame()
    {
        yield return null;                 // 1 frame
        Physics2D.SyncTransforms();        // sincroniza transforms -> OverlapPoint já enxerga estado novo
        canCheck = true;
        Debug.Log("[WinChecker] ArmCheckNextFrame: canCheck=true");

        // delay mínimo extra para armar mesmo que o jogador não tenha mexido ainda
        yield return new WaitForSeconds(0.05f);
        armedForVictory = true;
        Debug.Log("[WinChecker] ArmCheckNextFrame: armedForVictory=true");
    }

    private void Update()
    {
        // Debug.Log($"[WinChecker] Update: canCheck={canCheck}, armed={armedForVictory}, won={won}");
        // importante: só checar após LevelLoaded
        if (!canCheck || !armedForVictory)
        {
            return;
        }

        if (!won)
        {
            if (AllGoalsHaveBoxes())
            {
                won = true;

                // >>> ACHIEVEMENTS
                if (LevelRunTracker.Instance != null)
                {
                    var stats = LevelRunTracker.Instance.GetSnapshot();
                    Achievements.NotifyLevelCompleted(stats);
                }
                else
                {
                    Debug.LogWarning("[WinChecker] LevelRunTracker.Instance == null; sem stats pra achievements.");
                }

                TryUnlockNextLevel();                // progresso
                GameEvents.RaiseGoalsMaybeChanged(); // HUD mostra 0 imediatamente

                if (victoryUI == null)
                {
                    victoryUI = VictoryUI.Instance;
                }
                if (victoryUI != null)
                {
                    victoryUI.Show();
                }
                else
                {
                    Debug.Log("Level cleared! (C = Next, X = Restart)");
                }
            }
        }

    }

    // ---------------- HUD provider ----------------
    private int GetGoalsLeft()
    {
        CountGoalsAndCovered(out int total, out int covered);
        return Mathf.Max(0, total - covered);
    }

    private void CountGoalsAndCovered(out int total, out int covered)
    {
        total = 0; covered = 0;

        // conte SOMENTE sob o level atual (usa o currentLevel público do LevelManager)
        var lm = LevelManager.Instance;
        var root = (lm != null) ? lm.currentLevel : null;

        GoalIdentifier[] goals = root != null
            ? root.GetComponentsInChildren<GoalIdentifier>(true)
            : UnityEngine.Object.FindObjectsByType<GoalIdentifier>(UnityEngine.FindObjectsSortMode.None);

        total = goals.Length;

        for (int i = 0; i < goals.Length; i++)
        {
            Vector2 p = goals[i].transform.position;
            var hit = Physics2D.OverlapPoint(p, solidMask);
            if (!hit) continue;

            var box = hit.GetComponent<BoxIdentifier>() ?? hit.GetComponentInParent<BoxIdentifier>();
            if (box != null) covered++;
        }
    }
    // ----------------------------------------------

    private bool AllGoalsHaveBoxes()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.currentLevel == null)
        {
            return false;
        }

        var root = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : null;
        GoalIdentifier[] goals = root != null
        ? root.GetComponentsInChildren<GoalIdentifier>(true)
        : System.Array.Empty<GoalIdentifier>();
        if (goals.Length == 0)
        {
            return false;
        }

        foreach (var g in goals)
        {
            Vector2 p = g.transform.position;
            var hit = Physics2D.OverlapPoint(p, solidMask);
            if (hit == null)
            {
                return false;
            }

            var box = hit.GetComponent<BoxIdentifier>() ?? hit.GetComponentInParent<BoxIdentifier>();
            if (box == null)
            {
                return false;
            }
        }
        return true;
    }

    public void GoNext()
    {
        var lm = LevelManager.Instance;
        if (lm == null) return;

        victoryUI?.Hide();
        lm.LoadNext();

        won = false;
        canCheck = false; // aguarda novo level carregar
    }

    public void Restart()
    {
        var lm = LevelManager.Instance;
        if (lm == null) return;

        victoryUI?.Hide();
        lm.Reload();

        won = false;
        canCheck = false; // aguarda novo level carregar
    }

    private void TryUnlockNextLevel()
    {
        var lm = LevelManager.Instance;
        if (lm == null || lm.levelList == null) return;

        int next = lm.currentIndex + 1;
        if (next >= lm.LevelCount) return; // último nível

        SaveManager.UnlockUpTo(next);
        Debug.Log($"[WinChecker] highestUnlockedIndex = {SaveManager.HighestUnlockedIndex}");
    }

    // ---------------------------------------------------
    // CHEAT DEV: força vitória mesmo sem todos os goals
    // ---------------------------------------------------
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void ForceWinCheat()
    {
        if (won) return;  // já venceu? ignora

        won = true;

        // Telemetria / achievements
        if (LevelRunTracker.Instance != null)
        {
            var stats = LevelRunTracker.Instance.GetSnapshot();
            Achievements.NotifyLevelCompleted(stats);
        }

        // Progresso normal
        TryUnlockNextLevel();
        GameEvents.RaiseGoalsMaybeChanged();

        // UI de vitória
        if (victoryUI == null)
            victoryUI = VictoryUI.Instance;

        if (victoryUI != null)
            victoryUI.Show();
        else
            Debug.Log("[WinChecker] ForceWinCheat: Level cleared! (C = Next, X = Restart)");
    }
#endif
}
