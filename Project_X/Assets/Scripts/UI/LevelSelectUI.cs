using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gera o grid de botões a partir do LevelList.
/// - Botões até highestUnlocked ficam interativos; o resto fica "locked".
/// - Ao clicar, seta o LaunchArgs.PendingLevel e carrega a 02_Game.
/// </summary>
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

    private const string PlayerPrefsKey = "highestUnlocked";

    private void Start()
    {
        if (levelList == null || levelList.levels == null || levelList.levels.Length == 0)
        {
            Debug.LogError("[LevelSelectUI] LevelList não configurado.");
            return;
        }

        int levelCount = levelList.levels.Length;
        int highestUnlocked = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        highestUnlocked = Mathf.Clamp(highestUnlocked, 0, levelCount - 1);

        // Limpa grid (caso esteja recarregando)
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        for (int i = 0; i < levelCount; i++)
        {
            var entry = levelList.levels[i];
            var btn = Instantiate(buttonPrefab, gridParent);

            // Texto do botão (usa displayName ou fallback pro nome do prefab)
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = string.IsNullOrEmpty(entry.displayName) ? entry.levelPrefab.name : entry.displayName;

            bool unlocked = (i <= highestUnlocked);
            btn.interactable = unlocked;

            // Visual locked/unlocked
            var colors = btn.colors;
            colors.normalColor = unlocked ? unlockedColor : lockedColor;
            colors.disabledColor = lockedColor;
            btn.colors = colors;

            int capturedIndex = i;
            btn.onClick.AddListener(() =>
            {
                // guarda o índice escolhido e vai pra 02_Game
                LaunchArgs.PendingLevel = capturedIndex;
                Debug.Log($"[LevelSelect] Click → PendingLevel={capturedIndex}");
                SceneManager.LoadScene("02_Game", LoadSceneMode.Single);
            });
        }
    }
}
