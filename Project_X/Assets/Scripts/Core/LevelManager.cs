using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Data")]
    public LevelList levelList;
    public GameObject currentLevel;
    public int currentIndex = -1;
    public BoxCollider2D CurrentBounds { get; private set; }

    // sugestão no stackoverflow para nunca perder quantos níveis tem utilizando property
    public int LevelCount => (levelList != null && levelList.levels != null)
    ? levelList.levels.Length
    : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // <<< assina aqui
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < levelList.levels.Length; i++)
        {
            Debug.Log($"[LevelList] {i}: {levelList.levels[i].displayName}, prefab = {levelList.levels[i].levelPrefab}");
        }
    }

    private void InitializeBoxHighlights()
    {
        // Descobre o cellSize atual (pega do PlayerMover se existir)
        float cellSize = 1f;
        var pm = FindObjectOfType<PlayerMover>();
        if (pm != null) cellSize = pm.cellSize;

        // Helper local p/ grid conversion
        Vector2Int WorldToGridLocal(Vector3 wp)
        {
            int gx = Mathf.RoundToInt(wp.x / cellSize);
            int gy = Mathf.RoundToInt(wp.y / cellSize);
            return new Vector2Int(gx, gy);
        }

        // Constrói um set com todas as células de Goal do level atual
        var goals = currentLevel.GetComponentsInChildren<GoalIdentifier>(true);
        var goalCells = new HashSet<Vector2Int>();
        foreach (var gi in goals)
            goalCells.Add(WorldToGridLocal(gi.transform.position));

        // Para cada Box do level, liga/desliga o UnderSprite conforme esteja em goal
        var boxes = currentLevel.GetComponentsInChildren<BoxIdentifier>(true);
        foreach (var box in boxes)
        {
            var gs = box.GetComponent<BoxGoalState>();
            if (gs == null) continue; // se não tiver o componente, ignora (seguro)

            Vector2Int boxCell = WorldToGridLocal(box.transform.position);
            bool onGoal = goalCells.Contains(boxCell);
            gs.SetOnGoal(onGoal);
        }
    }

    public void LoadLevel(int index)
    {
        VictoryUI.Instance?.Hide();
        PauseMenu.Instance?.Hide();

        if (levelList == null)
        {
            Debug.LogError("[LevelManager] levelList == null (não tem asset atribuído no Instance ativo).");
            return;
        }

        if (LevelCount <= 0)
        {
            Debug.LogError("[LevelManager] LevelList vazio (LevelCount==0).");
            return;
        }

        if (index < 0 || index >= LevelCount)
        {
            Debug.LogError($"[LevelManager] Índice inválido ({index}). Range [0..{LevelCount - 1}].");
            return;
        }

        Debug.Log($"[LevelManager] Carregando level index={index} ({levelList.levels[index].displayName})");

        if (currentLevel != null)
        {
            Destroy(currentLevel);
            currentLevel = null;
        }

        var entry = levelList.levels[index];
        if (entry.levelPrefab == null)
        {
            Debug.LogError($"[LevelManager] Prefab nulo no Level {index} ({entry.displayName}).");
            return;
        }

        GameObject prefab = entry.levelPrefab;
        currentLevel = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        CurrentBounds = null;

        currentLevel.name = prefab.name;
        currentIndex = index;

        // foco de câmera e bounds (como você já tinha)...
        var boundsObj = currentLevel.transform.Find("Bounds");
        CurrentBounds = null;

        if (boundsObj != null)
        {
            CurrentBounds = boundsObj.GetComponent<BoxCollider2D>();
            if (CurrentBounds != null)
            {
                Bounds b = CurrentBounds.bounds;
                var camController = Camera.main?.GetComponent<CameraController>();
                if (camController != null) camController.FocusOnBounds(b);
            }
            else
            {
                Debug.LogWarning("[LevelManager] 'Bounds' encontrado, mas sem BoxCollider2D.");
            }
        }
        else
        {
            Debug.LogWarning("[LevelManager] Objeto 'Bounds' não encontrado no level.");
        }

        InitializeBoxHighlights();

        Debug.Log("[LevelManager] Level instanciado OK → RaiseLevelLoaded()");
        GameEvents.RaiseLevelLoaded();
    }
    // reiniciar nível
    public void Reload()
    {
        if (currentIndex < 0)
        {
            Debug.LogWarning("[LevelManager] Nenhum nível carregado pra recarregar.");
            return;
        }
        GameEvents.RaiseRestart();
        LoadLevel(currentIndex);
    }

    // próximo nível (caso exista)
    public void LoadNext()
    {
        if (LevelCount == 0)
        {
            Debug.LogWarning("[LevelManager] LevelList vazio.");
            return;
        }

        // começa no 0 se não carregou nada
        if (currentIndex < 0)
        {
            LoadLevel(0);
            return;
        }

        int next = currentIndex + 1;
        if (next >= LevelCount)
        {
            Debug.Log("[LevelManager] Já está no último nível.");
            return;
        }
        LoadLevel(next);
    }

    public bool InsideBounds(Vector2 worldPoint)
    {
        bool ok = (CurrentBounds != null && CurrentBounds.OverlapPoint(worldPoint));
        return ok;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se entrou na cena 02_Game e ainda não há level instanciado, carrega um
        if (scene.name == "02_Game" && currentLevel == null)
        {
            // escolhe pelo LaunchArgs (se veio do Level Select) ou fallback 0
            int index = LaunchArgs.PendingLevel.HasValue ? Mathf.Max(0, LaunchArgs.PendingLevel.Value) : 0;
            Debug.Log($"[LevelManager] OnSceneLoaded 02_Game → carregando level {index}");
            LoadLevel(index);
            LaunchArgs.PendingLevel = null;
        }
    }
}
