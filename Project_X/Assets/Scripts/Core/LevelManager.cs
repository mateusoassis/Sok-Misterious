using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Data")]
    public LevelList levelList;
    private GameObject currentLevel;
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

    public void LoadLevel(int index)
    {
        if (levelList == null || index < 0 || index >= LevelCount)
        {
            Debug.LogError("LevelManager: índice inválido ou LevelList não atribuído.");
            return;
        }

        if (currentLevel != null)
        {
            Destroy(currentLevel);
            currentLevel = null;
        }

        var entry = levelList.levels[index];
        if (entry.levelPrefab == null)
        {
            Debug.LogError($"[LevelManager] Prefab nulo no Level {index} ({entry.displayName})");
        }

        GameObject prefab = levelList.levels[index].levelPrefab;
        currentLevel = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        CurrentBounds = null;

        // renomear nome de nível na hierarquia com atualização de índice atual
        currentLevel.name = prefab.name;
        currentIndex = index;

        // foco de câmera
        var boundsObj = currentLevel.transform.Find("Bounds");
        CurrentBounds = null; // zera referência anterior

        if (boundsObj != null) {
            CurrentBounds = boundsObj.GetComponent<BoxCollider2D>();
            if (CurrentBounds != null) {
                Bounds b = CurrentBounds.bounds;

                CameraController camController = Camera.main.GetComponent<CameraController>();
                if (camController != null) {
                    camController.FocusOnBounds(b);
                }
            } else {
                Debug.LogWarning("[LevelManager] 'Bounds' encontrado, mas sem BoxCollider2D.");
            }
        } else {
            Debug.LogWarning("[LevelManager] Objeto 'Bounds' não encontrado no level.");
        }
    }
    // reiniciar nível
    public void Reload()
    {
        if (currentIndex < 0)
        {
            Debug.LogWarning("[LevelManager] Nenhum nível carregado pra recarregar.");
            return;
        }
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
}
