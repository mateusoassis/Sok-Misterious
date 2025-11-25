using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    // Singleton global que gerencia o ciclo de níveis
    public static LevelManager Instance;

    [Header("Level Data")]
    public LevelList levelList;          // Scriptable com a lista de níveis (nome + prefab)
    public GameObject currentLevel;      // Instância atual do nível carregado na cena
    public int currentIndex = -1;        // Índice do nível atual dentro da levelList
    public BoxCollider2D CurrentBounds { get; private set; } // Colisor que define os limites do nível

    // Sugestão no StackOverflow para nunca perder quantos níveis tem utilizando property
    // Protege contra levelList ou o array levels serem nulos
    public int LevelCount => (levelList != null && levelList.levels != null)
        ? levelList.levels.Length
        : 0;

    private void Awake()
    {
        // Implementa o padrão Singleton persistente entre cenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Assina o callback de cena carregada para garantir que 02_Game sempre tenha um nível
            SceneManager.sceneLoaded += OnSceneLoaded; // <<< assina aqui
        }
        else
        {
            // Evita múltiplas instâncias de LevelManager coexistindo
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Loga o conteúdo do LevelList no início (útil para debug no Editor)
        for (int i = 0; i < levelList.levels.Length; i++)
        {
            Debug.Log($"[LevelList] {i}: {levelList.levels[i].displayName}, prefab = {levelList.levels[i].levelPrefab}");
        }
    }

    // Sincroniza o estado visual das caixas em relação aos Goals ao carregar um nível
    private void InitializeBoxHighlights()
    {
        // Descobre o cellSize atual (pega do PlayerMover se existir)
        float cellSize = 1f;
        var pm = FindObjectOfType<PlayerMover>();
        if (pm != null) cellSize = pm.cellSize;

        // Helper local para converter posição de mundo em coordenadas de grid (x, y)
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

        // Para cada Box do level, liga/desliga o estado "onGoal" no BoxGoalState
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

    // Carrega o nível de índice "index" da levelList
    public void LoadLevel(int index)
    {
        // Garante que Victory/Pause não fiquem abertos ao trocar de nível
        VictoryUI.Instance?.Hide();
        PauseMenu.Instance?.Hide();

        // Validações básicas de configuração
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

        // Se já existe um nível carregado, destrói antes de instanciar o novo
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

        // Instancia o prefab do nível em (0,0,0) com rotação identidade
        GameObject prefab = entry.levelPrefab;
        currentLevel = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        CurrentBounds = null;

        // Renomeia a instância para bater com o nome do prefab (melhora a hierarquia)
        currentLevel.name = prefab.name;
        currentIndex = index;

        // Foco de câmera e bounds
        var boundsObj = currentLevel.transform.Find("Bounds");
        CurrentBounds = null;

        if (boundsObj != null)
        {
            CurrentBounds = boundsObj.GetComponent<BoxCollider2D>();
            if (CurrentBounds != null)
            {
                // Ajusta a câmera para enquadrar os Bounds do nível
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

        // Atualiza visualmente quais caixas estão em Goals
        InitializeBoxHighlights();

        Debug.Log("[LevelManager] Level instanciado OK → RaiseLevelLoaded()");
        GameEvents.RaiseLevelLoaded();
    }

    // Reinicia o nível atual (se houver)
    public void Reload()
    {
        if (currentIndex < 0)
        {
            Debug.LogWarning("[LevelManager] Nenhum nível carregado pra recarregar.");
            return;
        }

        // Evento global para quem quiser reagir ao restart
        GameEvents.RaiseRestart();

        // Reinstancia o mesmo índice
        LoadLevel(currentIndex);
    }

    // Carrega o próximo nível (caso exista)
    public void LoadNext()
    {
        if (LevelCount == 0)
        {
            Debug.LogWarning("[LevelManager] LevelList vazio.");
            return;
        }

        // Se ainda não carregou nada, começa pelo 0
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

    // Verifica se um ponto de mundo está dentro dos Bounds do nível atual
    public bool InsideBounds(Vector2 worldPoint)
    {
        bool ok = (CurrentBounds != null && CurrentBounds.OverlapPoint(worldPoint));
        return ok;
    }
    
    private void OnDestroy()
    {
        // Remove a assinatura do evento de cena carregada se esta for a instância ativa
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
            // Escolhe pelo LaunchArgs (se veio do Level Select) ou fallback 0
            int index = LaunchArgs.PendingLevel.HasValue ? Mathf.Max(0, LaunchArgs.PendingLevel.Value) : 0;
            Debug.Log($"[LevelManager] OnSceneLoaded 02_Game → carregando level {index}");
            LoadLevel(index);

            // Consome o argumento para não reaplicar em futuros loads
            LaunchArgs.PendingLevel = null;
        }
    }
}