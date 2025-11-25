using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla toda a HUD do jogo:
/// - Exibe nome do nível
/// - Contador de Moves, Pushes e Goals restantes
/// - Botões: Undo, Restart, Pause
///
/// Ele escuta eventos do GameEvents para manter a HUD atualizada.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Labels (TMP)")]
    [SerializeField] TMP_Text levelNameText;   // Nome do nível (opcional)
    [SerializeField] TMP_Text movesText;       // Texto "Moves: X"
    [SerializeField] TMP_Text pushesText;      // Texto "Pushes: X"
    [SerializeField] TMP_Text goalsText;       // Texto "Goals: X"

    [Header("Buttons")]
    [SerializeField] Button btnUndo;           // Botão desfazer
    [SerializeField] Button btnRestart;        // Botão reiniciar
    [SerializeField] Button btnPause;          // Botão pause

    // contadores locais apenas para exibição visual
    int moves;
    int pushes;

    // referência ao script que controla o jogador
    PlayerMover mover;

    void Awake()
    {
        // Pega referência do player mover automaticamente
        mover = FindObjectOfType<PlayerMover>();

        // Registra listeners dos botões (se existirem)
        if (btnUndo)    btnUndo.onClick.AddListener(OnClickUndo);
        if (btnRestart) btnRestart.onClick.AddListener(OnClickRestart);
        if (btnPause)   btnPause.onClick.AddListener(OnClickPause);
    }

    void OnEnable()
    {
        // Escuta eventos globais do jogo
        GameEvents.OnMove += HandleMove;                 // quando o player move
        GameEvents.OnPush += HandlePush;                 // quando empurra caixa
        GameEvents.OnLevelLoaded += HandleLevelLoaded;   // quando nível é carregado
        GameEvents.OnGoalsMaybeChanged += UpdateGoals;   // quando objetivos mudam

        // Atualiza HUD inteira ao habilitar
        RefreshAll();
    }

    void OnDisable()
    {
        // Remove listeners (boa prática)
        GameEvents.OnMove -= HandleMove;
        GameEvents.OnPush -= HandlePush;
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnGoalsMaybeChanged -= UpdateGoals;
    }

    void Update()
    {
        // Atualiza botão Undo dinamicamente
        // (Desabilitar quando não há nada pra desfazer = melhor UX)
        if (!mover)
            mover = FindObjectOfType<PlayerMover>();

        if (btnUndo)
            btnUndo.interactable = mover && mover.CanUndoNow();
    }

    // ---------------------------------------------------------------------
    // EVENTOS
    // ---------------------------------------------------------------------

    void HandleMove()
    {
        moves++;
        UpdateMoves();
        UpdateGoals();   // goals podem mudar após movimento
    }

    void HandlePush()
    {
        pushes++;
        UpdatePushes();
        UpdateGoals();   // goals podem mudar após push
    }

    void HandleLevelLoaded()
    {
        // limpa contadores
        moves = 0;
        pushes = 0;

        // player mover muda a cada load
        mover = FindObjectOfType<PlayerMover>();

        // atualiza HUD inteira
        RefreshAll();
    }

    // ---------------------------------------------------------------------
    // AÇÕES DE BOTÕES
    // ---------------------------------------------------------------------

    public void OnClickUndo()
    {
        if (!mover)
            mover = FindObjectOfType<PlayerMover>();

        if (!mover)
            return;

        if (!mover.CanUndoNow())
            return;

        mover.TryUndoFromUI();   // chama undo do player
    }

    public void OnClickRestart()
    {
        // fecha pause se estiver aberto (pra não travar a UI)
        if (PauseMenu.Instance != null)
            PauseMenu.Instance.Hide();

        // reinicia nível atual
        LevelManager.Instance?.Reload();
    }

    public void OnClickPause()
    {
        // Toggle do menu de pause
        PauseMenu.Instance?.Toggle();
    }

    // ---------------------------------------------------------------------
    // HELPERS DE UI
    // ---------------------------------------------------------------------

    void RefreshAll()
    {
        RefreshLevelName();
        UpdateMoves();
        UpdatePushes();
        UpdateGoals();
    }

    public void RefreshLevelName()
    {
        if (!levelNameText)
            return;

        var lm = LevelManager.Instance;

        // valida que existe level list e o index está dentro do range
        if (lm != null && lm.levelList != null &&
            lm.currentIndex >= 0 && lm.currentIndex < lm.LevelCount)
        {
            var entry = lm.levelList.levels[lm.currentIndex];
            levelNameText.text = entry.displayName;
        }
        else
        {
            levelNameText.text = "—"; // fallback
        }
    }

    void UpdateMoves()
    {
        if (movesText)
            movesText.text = $"Moves: {moves}";
    }

    void UpdatePushes()
    {
        if (pushesText)
            pushesText.text = $"Pushes: {pushes}";
    }

    void UpdateGoals()
    {
        // GoalsLeft é provido pelo GameEvents
        int left = GameEvents.GetGoalsLeft != null ? GameEvents.GetGoalsLeft() : 0;

        if (goalsText)
            goalsText.text = $"Goals: {left}";
    }
}