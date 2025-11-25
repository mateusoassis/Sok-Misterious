using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Controla a tela de seleção de fases:
/// - Lê a LevelList (lista de níveis)
/// - Lê progresso salvo via SaveManager.HighestUnlockedIndex
/// - Instancia botões dinamicamente em um grid
/// - Bloqueia/desbloqueia níveis de acordo com o progresso
/// - Ao clicar em um nível, seta LaunchArgs.PendingLevel e carrega a cena 02_Game
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("Data")]
    public LevelList levelList;            // ScriptableObject com todos os níveis disponíveis

    [Header("UI")]
    public Transform gridParent;           // Pai onde os botões serão instanciados (ex.: GridLayoutGroup)
    public Button buttonPrefab;            // Prefab do botão de nível

    [Header("Lock Visual (opcional)")]
    public Color lockedColor   = new Color(0.6f, 0.6f, 0.6f, 1f); // cor usada para níveis bloqueados
    public Color unlockedColor = Color.white;                     // cor padrão dos níveis liberados

    private void Start()
    {
        // Validação básica: precisa de uma LevelList válida
        if (levelList == null || levelList.levels == null || levelList.levels.Length == 0)
        {
            Debug.LogError("[LevelSelectUI] LevelList não configurado.");
            return;
        }

        int levelCount = levelList.levels.Length;

        // Usa o SaveManager para ler o progresso do jogador
        int highestUnlocked = Mathf.Clamp(SaveManager.HighestUnlockedIndex, 0, levelCount - 1);
        Debug.Log($"[LevelSelectUI] LevelCount={levelCount} | HighestUnlockedIndex={highestUnlocked}");

        // Limpa qualquer botão que já esteja dentro do grid (caso a cena tenha algo antigo)
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        Button firstToSelect = null; // botão que ficará com o foco inicial (para gamepad)

        // Loop em todos os níveis configurados
        for (int i = 0; i < levelCount; i++)
        {
            var entry = levelList.levels[i];

            // Instancia um botão filho do grid
            var btn = Instantiate(buttonPrefab, gridParent);

            // Tenta encontrar o TextMeshProUGUI no botão para exibir o nome do nível
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                // Usa displayName se estiver definido, se não, o nome do prefab do nível
                tmp.text = string.IsNullOrEmpty(entry.displayName)
                    ? entry.levelPrefab.name
                    : entry.displayName;
            }

            // Regra de desbloqueio: níveis com índice <= highestUnlocked estão liberados
            bool unlocked = (i <= highestUnlocked);
            btn.interactable = unlocked;

            // Ajuste visual de locked/unlocked
            var colors = btn.colors;
            colors.normalColor   = unlocked ? unlockedColor : lockedColor;
            colors.disabledColor = lockedColor;
            btn.colors = colors;

            // Captura o índice do nível para usar dentro do listener do botão
            int capturedIndex = i;
            btn.onClick.AddListener(() =>
            {
                // Define qual nível deve ser carregado na próxima cena de jogo
                LaunchArgs.PendingLevel = capturedIndex;
                Debug.Log($"[LevelSelect] Click → PendingLevel={capturedIndex}");

                // Carrega a cena de gameplay (02_Game)
                SceneManager.LoadScene("02_Game", LoadSceneMode.Single);
            });

            // Escolhe um botão válido para receber foco inicial (para navegação via controle)
            if (unlocked)
                firstToSelect = btn;
        }

        // Define foco inicial no EventSystem para navegação com controle / teclado
        if (EventSystem.current && firstToSelect)
            EventSystem.current.SetSelectedGameObject(firstToSelect.gameObject);
    }
}