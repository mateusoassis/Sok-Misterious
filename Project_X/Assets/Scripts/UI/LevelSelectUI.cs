using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Data")]
    public LevelList levelList;

    [Header("UI")]
    public Transform gridParent;
    public Button buttonPrefab;

    [Header("Lock Visual (opcional)")]
    public Color lockedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color unlockedColor = Color.white;

    private void Start()
    {
        if (levelList == null || levelList.levels == null || levelList.levels.Length == 0)
        {
            Debug.LogError("[LevelSelectUI] LevelList não configurado.");
            return;
        }

        int levelCount = levelList.levels.Length;

        // Usa o SaveManager para ler o progresso
        int highestUnlocked = Mathf.Clamp(SaveManager.HighestUnlockedIndex, 0, levelCount - 1);
        Debug.Log($"[LevelSelectUI] LevelCount={levelCount} | HighestUnlockedIndex={highestUnlocked}");

        // Limpa grid
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        Button firstToSelect = null;

        for (int i = 0; i < levelCount; i++)
        {
            var entry = levelList.levels[i];
            var btn = Instantiate(buttonPrefab, gridParent);

            // Texto do botão (displayName ou nome do prefab)
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
                tmp.text = string.IsNullOrEmpty(entry.displayName) ? entry.levelPrefab.name : entry.displayName;

            bool unlocked = (i <= highestUnlocked);
            btn.interactable = unlocked;

            // Visual locked/unlocked
            var colors = btn.colors;
            colors.normalColor   = unlocked ? unlockedColor : lockedColor;
            colors.disabledColor = lockedColor;
            btn.colors = colors;

            int capturedIndex = i;
            btn.onClick.AddListener(() =>
            {
                LaunchArgs.PendingLevel = capturedIndex;
                Debug.Log($"[LevelSelect] Click → PendingLevel={capturedIndex}");
                SceneManager.LoadScene("02_Game", LoadSceneMode.Single);
            });

            if (unlocked) firstToSelect = btn;
        }

        // Foco inicial para gamepad: último desbloqueado
        if (EventSystem.current && firstToSelect)
            EventSystem.current.SetSelectedGameObject(firstToSelect.gameObject);
    }
}