using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu de Pause simples:
/// - Abre/fecha com Esc (PC) ou Start (gamepad).
/// - Trava/destrava input do Player (desabilita PlayerMover).
/// - Opcional: usa Time.timeScale = 0 enquanto pausado.
/// - Botões: Resume, Restart (LevelManager.Reload), Level Select (carrega cena 01_LevelSelect).
/// 
/// Safe-guards:
/// - Sempre inicia oculto (HideImmediate).
/// - Esconde ao carregar um novo level (escuta GameEvents.OnLevelLoaded).
/// - Garante restaurar o timeScale ao sair/destruir.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("Refs")]
    public CanvasGroup panel;
    public Button btnResume;
    public Button btnRestart;
    public Button btnLevelSelect;

    [Header("Options")]
    [Tooltip("Se true, usa Time.timeScale=0 quando pausado.")]
    public bool useTimeScalePause = true;

    // input
    private InputAction pauseAction;

    // cache do player pra travar input
    private PlayerMover cachedPlayer;

    // estado interno
    private bool isVisible;

    // guard para (des)inscrever eventos corretamente
    private System.Action onLevelLoadedHandler;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // cria ação de Pause (Esc / Start)
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");

        pauseAction.Enable();

        // tenta achar panel se não foi arrastado
        if (!panel) panel = GetComponentInChildren<CanvasGroup>(true);

        // wire dos botões
        if (btnResume)      btnResume.onClick.AddListener(OnClickResume);
        if (btnRestart)     btnRestart.onClick.AddListener(OnClickRestart);
        if (btnLevelSelect) btnLevelSelect.onClick.AddListener(OnClickSelect);

        // escutar carregamento de level pra sempre esconder o pause
        onLevelLoadedHandler = Hide;
        GameEvents.OnLevelLoaded += onLevelLoadedHandler;

        HideImmediate(); // estado inicial invisível
    }

    private void OnDestroy()
    {
        pauseAction.Disable();

        if (onLevelLoadedHandler != null)
            GameEvents.OnLevelLoaded -= onLevelLoadedHandler;

        // failsafe: se for destruído pausado, volta o timeScale
        if (useTimeScalePause) Time.timeScale = 1f;

        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        // Toggle por input
        if (pauseAction.WasPressedThisFrame())
        {
            // se o VictoryPanel estiver aberto, ignorar pause (opcional)
            // (mantemos simples: deixa pausar mesmo em victory, mas geralmente não precisa)
            Toggle();
        }
    }

    // ------- API -------

    public void Toggle()
    {
        if (isVisible) Hide();
        else Show();
    }

    public void Show()
    {
        if (!panel) return;
        if (isVisible) return;

        isVisible = true;

        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;

        // trava o player
        cachedPlayer = FindObjectOfType<PlayerMover>();
        if (cachedPlayer) cachedPlayer.enabled = false;

        // pausa lógica global (opcional)
        if (useTimeScalePause) Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (!panel) return;
        if (!isVisible) return;

        isVisible = false;

        // retoma timeScale antes de liberar player
        if (useTimeScalePause) Time.timeScale = 1f;

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        // libera o player do nível atual
        if (cachedPlayer) cachedPlayer.enabled = true;
        cachedPlayer = null;
    }

    private void HideImmediate()
    {
        if (!panel) return;
        isVisible = false;

        // garante timeScale normal
        if (useTimeScalePause) Time.timeScale = 1f;

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        cachedPlayer = null;
    }

    // ------- Botões -------

    private void OnClickResume()
    {
        Hide();
    }

    private void OnClickRestart()
    {
        Hide(); // fecha o pause antes de recarregar
        LevelManager.Instance?.Reload();
    }

    private void OnClickSelect()
    {
        Hide(); // fecha o pause antes de trocar de cena
        SceneManager.LoadScene("01_LevelSelect", LoadSceneMode.Single);
    }
}
