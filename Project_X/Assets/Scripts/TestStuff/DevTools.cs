using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Dev tools simples:
/// - F1: toggle overlay de debug
/// - F5: reload nível atual
/// - F6: próximo nível
/// - F7: nível anterior
/// - F8: força vitória (cheat)
/// 
/// Só funciona em Editor ou Development Build.
/// </summary>
public class DevTools : MonoBehaviour
{
    [Header("Dev Mode")]
    [Tooltip("Se desligado, desativa tudo mesmo em Editor/DevBuild.")]
    [SerializeField] bool devModeEnabled = true;

    [Header("Overlay (opcional)")]
    [SerializeField] CanvasGroup overlayCanvas;
    [SerializeField] TextMeshProUGUI overlayLabel;

    InputAction toggleOverlayAction;
    InputAction reloadAction;
    InputAction nextLevelAction;
    InputAction prevLevelAction;
    InputAction forceWinAction;

    bool overlayVisible;

    void Awake()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!devModeEnabled)
        {
            enabled = false;
            return;
        }
#else
        // em build final, desliga tudo
        enabled = false;
        return;
#endif
        // cria input actions em runtime
        toggleOverlayAction = new InputAction("DevToggleOverlay", InputActionType.Button);
        toggleOverlayAction.AddBinding("<Keyboard>/f1");

        reloadAction = new InputAction("DevReload", InputActionType.Button);
        reloadAction.AddBinding("<Keyboard>/f5");

        nextLevelAction = new InputAction("DevNext", InputActionType.Button);
        nextLevelAction.AddBinding("<Keyboard>/f6");

        prevLevelAction = new InputAction("DevPrev", InputActionType.Button);
        prevLevelAction.AddBinding("<Keyboard>/f7");

        forceWinAction = new InputAction("DevForceWin", InputActionType.Button);
        forceWinAction.AddBinding("<Keyboard>/f8");

        toggleOverlayAction.Enable();
        reloadAction.Enable();
        nextLevelAction.Enable();
        prevLevelAction.Enable();
        forceWinAction.Enable();

        // começa overlay desligado
        SetOverlayVisible(false);
    }

    void OnDestroy()
    {
        toggleOverlayAction.Disable();
        reloadAction.Disable();
        nextLevelAction.Disable();
        prevLevelAction.Disable();
        forceWinAction.Disable();
    }

    void Update()
    {
        if (toggleOverlayAction.WasPressedThisFrame())
        {
            SetOverlayVisible(!overlayVisible);
        }

        if (reloadAction.WasPressedThisFrame())
        {
            DevReload();
        }

        if (nextLevelAction.WasPressedThisFrame())
        {
            DevNextLevel();
        }

        if (prevLevelAction.WasPressedThisFrame())
        {
            DevPrevLevel();
        }

        if (forceWinAction.WasPressedThisFrame())
        {
            DevForceWin();
        }

        if (overlayVisible)
        {
            UpdateOverlay();
        }
    }

    // --------- Comandos ---------

    void DevReload()
    {
        var lm = LevelManager.Instance;
        if (lm == null)
        {
            Debug.LogWarning("[DevTools] Reload: LevelManager.Instance == null");
            return;
        }
        Debug.Log("[DevTools] Reload current level (F5).");
        lm.Reload();
    }

    void DevNextLevel()
    {
        var lm = LevelManager.Instance;
        if (lm == null)
        {
            Debug.LogWarning("[DevTools] NextLevel: LevelManager.Instance == null");
            return;
        }
        Debug.Log("[DevTools] Next level (F6).");
        lm.LoadNext();
    }

    void DevPrevLevel()
    {
        var lm = LevelManager.Instance;
        if (lm == null || lm.levelList == null)
        {
            Debug.LogWarning("[DevTools] PrevLevel: LevelManager/LevelList null.");
            return;
        }

        int cur = lm.currentIndex;
        if (cur <= 0)
        {
            Debug.Log("[DevTools] PrevLevel: já está no primeiro nível.");
            return;
        }

        int prev = cur - 1;
        Debug.Log($"[DevTools] Prev level → {prev} (F7).");
        lm.LoadLevel(prev);
    }

    void DevForceWin()
    {
        var wc = FindObjectOfType<WinChecker>();
        if (wc == null)
        {
            Debug.LogWarning("[DevTools] ForceWin: WinChecker não encontrado na cena.");
            return;
        }

        Debug.Log("[DevTools] ForceWinCheat (F8).");
        wc.ForceWinCheat();
    }

    // --------- Overlay ---------

    void SetOverlayVisible(bool visible)
    {
        overlayVisible = visible;

        if (!overlayCanvas) return;

        overlayCanvas.alpha = visible ? 1f : 0f;
        overlayCanvas.interactable = false;
        overlayCanvas.blocksRaycasts = false;
    }

    void UpdateOverlay()
    {
        if (!overlayLabel) return;

        var lm = LevelManager.Instance;
        var rt = LevelRunTracker.Instance;

        int levelIndex = lm != null ? lm.currentIndex : -1;
        string levelName = "???";
        string highestStr = "?";

        if (lm != null && lm.levelList != null &&
            levelIndex >= 0 && levelIndex < lm.levelList.levels.Length)
        {
            var entry = lm.levelList.levels[levelIndex];
            levelName = string.IsNullOrEmpty(entry.displayName)
                ? entry.levelPrefab ? entry.levelPrefab.name : "???"
                : entry.displayName;
        }

        highestStr = SaveManager.HighestUnlockedIndex.ToString();

        LevelStats stats = default;
        if (rt != null)
            stats = rt.GetSnapshot();

        overlayLabel.text =
            $"DEV TOOLS\n" +
            $"Level: {levelIndex} ({levelName})\n" +
            $"HighestUnlocked: {highestStr}\n" +
            $"Moves: {stats.moves} | Pushes: {stats.pushes}\n" +
            $"Undos: {stats.undos} | Restarts: {stats.restarts}\n" +
            $"Time: {stats.timeSec:F1}s\n" +
            $"F1: Toggle overlay | F5: Reload | F6: Next | F7: Prev | F8: Force Win";
    }
}