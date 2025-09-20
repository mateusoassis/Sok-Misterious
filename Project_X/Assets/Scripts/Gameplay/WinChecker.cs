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
    }

    // ---------- HUD ----------
    private int GetGoalsLeft()
    {
        CountGoalsAndCovered(out int total, out int covered);
        return Mathf.Max(0, total - covered);
    }
    private void CountGoalsAndCovered(out int total, out int covered)
    {
        // Unity 2023+: use FindObjectsByType totalmente qualificado p/ evitar ambiguidade
        var goals = UnityEngine.Object.FindObjectsByType<GoalIdentifier>(UnityEngine.FindObjectsSortMode.None);
        total = goals.Length;
        covered = 0;

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
        restartAction.Disable();
        nextAction.Disable();

        // evita segurar referência se este WinChecker sair de cena
        if (GameEvents.GetGoalsLeft == GetGoalsLeft)
        {
            GameEvents.SetGoalsLeftProvider(null);
        }
            
    }
    private void Update()
    {
        if (!won)
        {
            if (AllGoalsHaveBoxes())
            {
                won = true;
                Debug.Log("Level cleared! (X = restart, C = Next)");
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

    private void GoNext()
    {
        if (LevelManager.Instance == null)
        {
            return;
        }
        LevelManager.Instance.LoadNext();
        won = false;
    }

    private void Restart()
    {
        if (LevelManager.Instance == null)
        {
            return;
        }
        LevelManager.Instance.Reload();
        won = false;
    }
}
