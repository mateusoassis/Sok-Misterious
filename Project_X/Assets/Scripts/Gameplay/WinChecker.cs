using UnityEngine;
using UnityEngine.InputSystem;

public class WinChecker : MonoBehaviour
{
    [Header("Mesma LayerMask escolhida no Player")]
    public LayerMask solidMask;

    // inputs para NEXT/RESTART (de teclado e gamepad)
    // x para restart, c para next map
    private InputAction restartAction; // x teclado, A/sul gamepad
    private InputAction nextAction; // c teclado, R1 gamepad

    private bool won;

    private VictoryUI victoryUI;

    private void Awake()
    {
        // x para reiniciar fase em teclado
        // b (sul) para próxima fase em gamepad
        restartAction = new InputAction("Restart", InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/x");
        restartAction.AddBinding("<Gamepad>/buttonSouth");

        // c para próxima fase em teclado
        // L1 para próxima fase em gamepad
        nextAction = new InputAction("Next", InputActionType.Button);
        nextAction.AddBinding("<Keyboard>/c");
        nextAction.AddBinding("<Gamepad>/rightShoulder");

        restartAction.Enable();
        nextAction.Enable();

        // HUD: pergunta "quantos goals faltam?"
        GameEvents.SetGoalsLeftProvider(GetGoalsLeft);

        GameEvents.OnLevelLoaded += () => won = false;

        victoryUI = VictoryUI.Instance;
    }

    // ---------- HUD ----------
    private int GetGoalsLeft()
    {
        CountGoalsAndCovered(out int total, out int covered);
        return Mathf.Max(0, total - covered);
    }
    private void CountGoalsAndCovered(out int total, out int covered)
    {
        total = 0;
        covered = 0;

        // **NOVO**: conte apenas sob o level atual
        var root = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : null;
        GoalIdentifier[] goals = null;

        if (root != null)
        {
            goals = root.GetComponentsInChildren<GoalIdentifier>(true);
        }
        else
        {
            // fallback (não recomendado, só por segurança)
            goals = UnityEngine.Object.FindObjectsByType<GoalIdentifier>(UnityEngine.FindObjectsSortMode.None);
        }

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
    // -------------------------

    private void OnDestroy()
    {
        GameEvents.OnLevelLoaded -= () => won = false;
        restartAction.Disable();
        nextAction.Disable();

        // evita segurar referência se este WinChecker sair de cena
        if (GameEvents.GetGoalsLeft == GetGoalsLeft)
        {
            GameEvents.SetGoalsLeftProvider(null);
        }
    }
    
    private void OnEnable()
    {
        won = false; // ao entrar em cena, volta a checar vitória
    }  

    private void Update()
    {
        if (!won)
        {
            if (AllGoalsHaveBoxes())
            {
                won = true;
                GameEvents.RaiseGoalsMaybeChanged();
                Debug.Log("Level cleared! (X = restart, C = Next)");

                // Mostra painel de vitória (se existir)
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
        else
        {
            if (nextAction.WasPressedThisFrame())
            {
                GoNext();
            }
            if (restartAction.WasPressedThisFrame())
            {
                Restart();
            }
        }
    }

    private bool AllGoalsHaveBoxes()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.currentLevel == null)
        {
            return false;
        }

        // só goals do level atual
        var goals = UnityEngine.Object.FindObjectsByType<GoalIdentifier>(FindObjectsSortMode.None);
        if (goals.Length == 0)
        {
            return false;
        }

        foreach (var g in goals)
        {
            Vector2 p = g.transform.position;

            // tem coisa sólida no centro?
            var hit = Physics2D.OverlapPoint(p, solidMask);
            if (hit == null)
            {
                return false;
            }

            // tem caixa no centro?
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
        if (LevelManager.Instance == null)
        {
            return;
        }
        LevelManager.Instance.LoadNext();
        won = false;
    }

    public void Restart()
    {
        if (LevelManager.Instance == null)
        {
            return;
        }
        LevelManager.Instance.Reload();
        won = false;
    }
}
