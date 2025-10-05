using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    [Header("Refs")]
    public CanvasGroup panel;
    public Button btnNext;
    public Button btnRestart;
    public Button btnLevelSelect;

    private PlayerMover cachedPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // tentar achar panel se não foi arrastado
        if (!panel)
        {
            panel = GetComponentInChildren<CanvasGroup>(true);
        }

        // HideImmediate();

        if (btnNext)
        {
            btnNext.onClick.AddListener(OnClickNext);
        }
        if (btnRestart)
        {
            btnRestart.onClick.AddListener(OnClickRestart);
        }
        if (btnLevelSelect)
        {
            btnLevelSelect.onClick.AddListener(OnClickSelect);
        }

        // sempre esconder quando um level novo carregar
        // GameEvents.OnLevelLoaded += Hide;
        // e quando a cena mudar (ex.: ir pro Level Select)
        SceneManager.activeSceneChanged += (_, __) => Hide();
    }

    private void OnEnable()
    {
        HideImmediate(); // garante que nunca aparece sozinho
        GameEvents.OnLevelLoaded += Hide; // sempre esconde quando trocar de level
    }
    private void OnDisable()
    {
        // Debug.Log("[VictoryUI] OnDisable -> remove OnLevelLoaded");
        GameEvents.OnLevelLoaded -= Hide;
    }

    private void OnDestroy()
    {
        GameEvents.OnLevelLoaded -= Hide;
        SceneManager.activeSceneChanged -= (_, __) => Hide();

        if (Instance == this) Instance = null;
    }

    public void Show()
    {
        // Debug.Log("[VictoryUI] Show chamado");
        if (!panel)
        {
            return;
        }

        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;

        cachedPlayer = FindObjectOfType<PlayerMover>();
        if (cachedPlayer)
        {
            cachedPlayer.enabled = false;
        }
    }

    public void Hide()
    {
        // Debug.Log("[VictoryUI] Hide chamado");
        if (!panel)
        {
            return;
        }

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        if (cachedPlayer) cachedPlayer.enabled = true;
        cachedPlayer = null;
    }

    private void HideImmediate()
    {
        // Debug.Log("[VictoryUI] OnEnable -> HideImmediate()");
        if (!panel) return;
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        cachedPlayer = null;
    }

    private void OnClickNext()
    {
        var wc = FindObjectOfType<WinChecker>();
        if (wc != null)
        {
            wc.GoNext();
        }
        else
        {
            Hide();
            LevelManager.Instance?.LoadNext();
        }
    }

    private void OnClickRestart()
    {
        var wc = FindObjectOfType<WinChecker>();
        if (wc != null)
        {
            wc.Restart();
        }
        else
        {
            Hide();
            LevelManager.Instance?.Reload();
        }
    }

    private void OnClickSelect()
    {
        Hide();
        SceneManager.LoadScene("01_LevelSelect", LoadSceneMode.Single);
    }
}