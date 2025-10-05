using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Labels (TMP)")]
    [SerializeField] TMP_Text levelNameText;   // opcional (pode deixar vazio)
    [SerializeField] TMP_Text movesText;
    [SerializeField] TMP_Text pushesText;
    [SerializeField] TMP_Text goalsText;

    [Header("Buttons")]
    [SerializeField] Button btnUndo;
    [SerializeField] Button btnRestart;
    [SerializeField] Button btnPause;

    // contadores locais (somente visual)
    int moves;
    int pushes;

    // cache
    PlayerMover mover;

    void Awake()
    {
        mover = FindObjectOfType<PlayerMover>();

        if (btnUndo)    btnUndo.onClick.AddListener(OnClickUndo);
        if (btnRestart) btnRestart.onClick.AddListener(OnClickRestart);
        if (btnPause)   btnPause.onClick.AddListener(OnClickPause);
    }

    void OnEnable()
    {
        GameEvents.OnMove += HandleMove;
        GameEvents.OnPush += HandlePush;
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
        GameEvents.OnGoalsMaybeChanged += UpdateGoals;

        RefreshAll();
    }

    void OnDisable()
    {
        GameEvents.OnMove -= HandleMove;
        GameEvents.OnPush -= HandlePush;
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnGoalsMaybeChanged -= UpdateGoals;
    }

    void Update()
    {
        // feedback de UX: habilita/desabilita Undo conforme pode desfazer
        if (!mover) mover = FindObjectOfType<PlayerMover>();
        if (btnUndo) btnUndo.interactable = mover && mover.CanUndoNow();
    }

    // ----------------- Events -----------------

    void HandleMove()
    {
        moves++;
        UpdateMoves();
        UpdateGoals();
    }

    void HandlePush()
    {
        pushes++;
        UpdatePushes();
        UpdateGoals();
    }

    void HandleLevelLoaded()
    {
        // reset visual ao (re)carregar level
        moves = 0;
        pushes = 0;

        // referência do mover muda após reload
        mover = FindObjectOfType<PlayerMover>();

        RefreshAll();
    }

    // ----------------- UI Actions -----------------

    public void OnClickUndo()
    {
        if (!mover) mover = FindObjectOfType<PlayerMover>();
        if (!mover) return;

        if (!mover.CanUndoNow()) return;
        mover.TryUndoFromUI();
    }

    public void OnClickRestart()
    {
        // fecha pause se estiver aberto, pra evitar ficar travado
        if (PauseMenu.Instance != null) PauseMenu.Instance.Hide();
        LevelManager.Instance?.Reload();
    }

    public void OnClickPause()
    {
        PauseMenu.Instance?.Toggle();
    }

    // ----------------- Helpers -----------------

    void RefreshAll()
    {
        RefreshLevelName();
        UpdateMoves();
        UpdatePushes();
        UpdateGoals();
    }

    public void RefreshLevelName()
    {
        if (!levelNameText) return;

        var lm = LevelManager.Instance;
        if (lm != null && lm.levelList != null && lm.currentIndex >= 0 && lm.currentIndex < lm.LevelCount)
        {
            var entry = lm.levelList.levels[lm.currentIndex];
            levelNameText.text = entry.displayName;
        }
        else
        {
            levelNameText.text = "—";
        }
    }

    void UpdateMoves()
    {
        if (movesText) movesText.text = $"Moves: {moves}";
    }

    void UpdatePushes()
    {
        if (pushesText) pushesText.text = $"Pushes: {pushes}";
    }

    void UpdateGoals()
    {
        int left = GameEvents.GetGoalsLeft != null ? GameEvents.GetGoalsLeft() : 0;
        if (goalsText) goalsText.text = $"Goals: {left}";
    }
}
