using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Painel de vitória: aparece quando o WinChecker detecta win.
/// Responsável por:
/// - Mostrar/ocultar o painel (CanvasGroup)
/// - Bloquear/desbloquear o input do Player (desabilita PlayerMover)
/// - Lidar com botões Next/Restart/Select (chama LevelManager)
///
/// Observação:
/// - Não mexe em Time.timeScale.
/// - Mantém o WinChecker livre pra aceitar inputs (C/X) como redundância.
/// </summary>
public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    [Header("References")]
    [Tooltip("CanvasGroup do painel de vitória")]
    public CanvasGroup panel;

    [Tooltip("Botão Next (opcional)")]
    public Button btnNext;

    [Tooltip("Botão Restart (opcional)")]
    public Button btnRestart;

    [Tooltip("Botão Level Select (opcional, pode ficar desligado)")]
    public Button btnLevelSelect;

    // cache pra reativar o Player depois
    private PlayerMover cachedPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Estado inicial: oculto
        HideImmediate();

        // Wire dos botões (se existirem)
        if (btnNext) btnNext.onClick.AddListener(OnClickNext);
        if (btnRestart) btnRestart.onClick.AddListener(OnClickRestart);
        if (btnLevelSelect) btnLevelSelect.onClick.AddListener(OnClickSelect);
    }

    /// <summary>Exibe o painel e bloqueia input do Player.</summary>
    public void Show()
    {
        if (!panel) return;

        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;

        // Desabilita o PlayerMover atual (bloqueia input)
        cachedPlayer = FindObjectOfType<PlayerMover>();
        if (cachedPlayer) cachedPlayer.enabled = false;
    }

    /// <summary>Oculta o painel e libera input do Player.</summary>
    public void Hide()
    {
        if (!panel) return;

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        // Reabilita o PlayerMover do nível atual (se ainda existir)
        if (cachedPlayer) cachedPlayer.enabled = true;
        cachedPlayer = null;
    }

    /// <summary>Oculta sem tentar reabilitar player (estado inicial).</summary>
    private void HideImmediate()
    {
        if (!panel) return;
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
    }

    // ------- Botões -------

    private void OnClickNext()
    {
        // Oculta primeiro pra não “ficar” em cima do novo level
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
        // Por enquanto só loga (Level Select ainda não existe)
        Hide();
        Debug.Log("[VictoryUI] Level Select ainda não implementado.");
        // Quando tiver cena de select, é só fazer: SceneManager.LoadScene("01_LevelSelect");
    }
}
