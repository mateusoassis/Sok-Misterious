using UnityEngine;
using TMPro;

/// <summary>
/// Atualiza HUD de Moves, Pushes e Goals restantes.
/// - Escuta GameEvents.OnMove/OnPush.
/// - Lê GameEvents.GetGoalsLeft() para Goals.
/// - Reseta contadores quando GameEvents.OnLevelLoaded dispara.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Refs (TMP)")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI pushesText;
    public TextMeshProUGUI goalsText;

    // contadores locais do HUD (não gravam progresso; só visual)
    private int moves;
    private int pushes;

    private void OnEnable()
    {
        GameEvents.OnMove += HandleMove;
        GameEvents.OnPush += HandlePush;
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
        GameEvents.OnGoalsMaybeChanged += UpdateGoals;

        // Inicializa visual na entrada da cena
        RefreshAll();
    }

    private void OnDisable()
    {
        GameEvents.OnMove -= HandleMove;
        GameEvents.OnPush -= HandlePush;
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnGoalsMaybeChanged -= UpdateGoals;
    }

    private void HandleMove()
    {
        moves++;
        UpdateMoves();
        UpdateGoals();
    }

    private void HandlePush()
    {
        pushes++;
        UpdatePushes();
        // Goals podem ter mudado (caixa entrou/saiu de goal)
        UpdateGoals();
    }

    private void HandleLevelLoaded()
    {
        // Sempre que um nível é carregado/recarregado, zera contadores
        moves = 0;
        pushes = 0;
        RefreshAll();
    }

    private void RefreshAll()
    {
        UpdateMoves();
        UpdatePushes();
        UpdateGoals();
    }

    private void UpdateMoves()
    {
        if (movesText) movesText.text = $"Moves: {moves}";
    }

    private void UpdatePushes()
    {
        if (pushesText) pushesText.text = $"Pushes: {pushes}";
    }

    private void UpdateGoals()
    {
        int left = GameEvents.GetGoalsLeft != null ? GameEvents.GetGoalsLeft() : 0;
        if (goalsText) goalsText.text = $"Goals: {left}";
    }
}
