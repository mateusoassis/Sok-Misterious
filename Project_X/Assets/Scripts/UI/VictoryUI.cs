using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Painel de vitória do jogo.
/// - Mostra botões de Next, Restart e Level Select quando o player completa o nível.
/// - Desabilita o PlayerMover enquanto o painel está visível.
/// - Se esconde automaticamente quando:
///     - Um novo level é carregado (GameEvents.OnLevelLoaded)
///     - A cena ativa muda (ex.: ir pro Level Select)
/// </summary>
public class VictoryUI : MonoBehaviour
{
    // Singleton simples pra facilitar acesso (VictoryUI.Instance)
    public static VictoryUI Instance { get; private set; }

    [Header("Refs")]
    public CanvasGroup panel;          // CanvasGroup do painel inteiro de vitória
    public Button btnNext;             // Botão "Next"
    public Button btnRestart;          // Botão "Restart"
    public Button btnLevelSelect;      // Botão "Level Select"

    // Referência pro PlayerMover atual pra poder travar/destravar input
    private PlayerMover cachedPlayer;

    private void Awake()
    {
        // Garante singleton: se já existir outro, destrói este
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Se o panel não foi arrastado no Inspector, tenta achar nos filhos
        if (!panel)
        {
            panel = GetComponentInChildren<CanvasGroup>(true);
        }

        // Liga os callbacks dos botões, se existirem
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

        // Esconde sempre que a cena ativa mudar (ex.: ir pro Level Select)
        SceneManager.activeSceneChanged += (_, __) => Hide();
    }

    private void OnEnable()
    {
        // Garante que nunca comece visível sem querer
        HideImmediate();

        // Sempre esconde quando um novo level é carregado
        GameEvents.OnLevelLoaded += Hide;
    }

    private void OnDisable()
    {
        // Remove inscrição do evento de LevelLoaded
        GameEvents.OnLevelLoaded -= Hide;
    }

    private void OnDestroy()
    {
        // Remove inscrição do evento de LevelLoaded
        GameEvents.OnLevelLoaded -= Hide;

        // Tentativa de desinscrever do activeSceneChanged.
        // OBS: esse lambda aqui é uma nova instância, não o mesmo usado no Awake,
        // então na prática ele não "remove" o handler anterior.
        SceneManager.activeSceneChanged -= (_, __) => Hide();

        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Mostra o painel de vitória e desabilita o PlayerMover atual.
    /// </summary>
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

        // Desabilita o PlayerMover pra impedir input enquanto o painel está aberto
        cachedPlayer = FindObjectOfType<PlayerMover>();
        if (cachedPlayer)
        {
            cachedPlayer.enabled = false;
        }
    }

    /// <summary>
    /// Esconde o painel e reabilita o PlayerMover, se houver.
    /// </summary>
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

        // Reabilita o player se ele tiver sido cacheado
        if (cachedPlayer) cachedPlayer.enabled = true;
        cachedPlayer = null;
    }

    /// <summary>
    /// Esconde o painel sem efeitos extras (usado pra garantir estado inicial).
    /// </summary>
    private void HideImmediate()
    {
        // Debug.Log("[VictoryUI] OnEnable -> HideImmediate()");
        if (!panel) return;

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        cachedPlayer = null;
    }

    // ----------------- Botões -----------------

    /// <summary>
    /// Botão "Next": tenta usar WinChecker (GoNext),
    /// se não achar, cai no LoadNext direto do LevelManager.
    /// </summary>
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

    /// <summary>
    /// Botão "Restart": tenta usar WinChecker (Restart),
    /// se não achar, recarrega via LevelManager.Reload().
    /// </summary>
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

    /// <summary>
    /// Botão "Level Select": vai direto pra cena 01_LevelSelect.
    /// </summary>
    private void OnClickSelect()
    {
        Hide();
        SceneManager.LoadScene("01_LevelSelect", LoadSceneMode.Single);
    }
}