using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public LevelList levelList;
    private GameObject currentLevel;

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
        if (levelList == null || index < 0 || index >= levelList.levels.Length)
        {
            Debug.LogError("LevelManager: índice inválido ou LevelList não atribuído.");
            return;
        }

        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        GameObject prefab = levelList.levels[index].levelPrefab;
        currentLevel = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        var boundsObj = currentLevel.transform.Find("Bounds");
        if (boundsObj != null)
        {
            var collider = boundsObj.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                Bounds b = collider.bounds;

                CameraController camController = Camera.main.GetComponent<CameraController>();
                if (camController != null)
                {
                    camController.FocusOnBounds(b);
                }
            }
        }
    }
}
