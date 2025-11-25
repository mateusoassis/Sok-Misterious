/// Botões de debug/dev usados no menu de pausa ou menu secreto.
/// Permitem:
/// - Resetar apenas o progresso "highestUnlocked"
/// - Resetar TODA a PlayerPrefs
/// - Opcionalmente voltar para a cena de Level Select
/// - Ou apenas recarregar o nível atual
///
/// Uso típico:
/// Colocar esse script em um GameObject que tenha botões UI chamando:
///   - OnClickResetHighest()
///   - OnClickResetPlayerPrefs()
using UnityEngine;
using UnityEngine.SceneManagement;
public class DevProgressButtons : MonoBehaviour
{
    [Header("Comportamento")]
    [SerializeField] bool goToLevelSelectAfter = true; 
    // Se TRUE → após reset voltar para cena de Level Select
    // Se FALSE → permanecer na cena do jogo e recarregar o nível atual

    [SerializeField] string levelSelectScene = "01_LevelSelect";
    [SerializeField] string gameScene        = "02_Game";

    [Header("Debug")]
    [SerializeField] bool verboseLogs = true;

    // ----------------------------------------------------------------------
    // BOTÕES
    // ----------------------------------------------------------------------

    /// <summary>
    /// Reseta SOMENTE o progresso de níveis (highestUnlockedIndex = 0)
    /// </summary>
    public void OnClickResetHighest()
    {
        ClosePauseIfOpen();

        if (verboseLogs)
            Debug.Log("[DEV] ResetHighest: SaveManager.ResetProgress() → highest=0");

        SaveManager.ResetProgress(); // Zera apenas o progresso
        SaveManager.Load();          // Recarrega valor padrão

        JumpAfterReset();
    }

    /// <summary>
    /// Reseta TODA a PlayerPrefs (APAGA TUDO!)
    /// </summary>
    public void OnClickResetPlayerPrefs()
    {
        ClosePauseIfOpen();

        if (verboseLogs)
            Debug.Log("[DEV] ResetPlayerPrefs: PlayerPrefs.DeleteAll()");

        PlayerPrefs.DeleteAll();   // APAGA TODOS OS DADOS DO JOGO
        PlayerPrefs.Save();

        SaveManager.Load();        // Recarrega defaults

        JumpAfterReset();
    }

    // ----------------------------------------------------------------------
    // SUPORTE
    // ----------------------------------------------------------------------

    /// <summary>
    /// Depois de um reset, decide pra onde mandar o jogador:
    /// - Cena de Level Select (mais comum para debugging)
    /// - Ou recarregar o nível atual caso você deseje continuar testando
    /// </summary>
    void JumpAfterReset()
    {
        Time.timeScale = 1f;

        if (goToLevelSelectAfter)
        {
            // Volta para a cena de seleção de níveis
            if (!CanLoad(levelSelectScene))
            {
                Debug.LogError($"[DEV] Cena '{levelSelectScene}' não está no Build Settings.");
                return;
            }

            if (verboseLogs)
                Debug.Log($"[DEV] Carregando '{levelSelectScene}'…");

            SceneManager.LoadScene(levelSelectScene, LoadSceneMode.Single);
        }
        else
        {
            // Fica na cena do jogo e recarrega apenas o level atual
            if (LevelManager.Instance != null)
            {
                if (verboseLogs)
                    Debug.Log("[DEV] Reload do nível atual…");

                LevelManager.Instance.Reload();
            }
            else
            {
                // Se o LevelManager ainda não existe, recarrega a cena inteira
                if (!CanLoad(gameScene))
                {
                    Debug.LogError($"[DEV] Cena '{gameScene}' não está no Build Settings.");
                    return;
                }

                if (verboseLogs)
                    Debug.Log($"[DEV] Carregando '{gameScene}'…");

                SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
            }
        }
    }

    /// <summary>
    /// Se o PauseMenu estiver aberto, fecha antes de resetar progresso.
    /// Evita inconsistências no HUD após o reload.
    /// </summary>
    void ClosePauseIfOpen()
    {
        if (PauseMenu.Instance != null)
        {
            if (verboseLogs)
                Debug.Log("[DEV] Fechando Pause…");

            PauseMenu.Instance.Hide();
        }
    }

    /// <summary>
    /// Checa se uma cena está listada no Build Settings.
    /// Evita tentar carregar cenas inexistentes.
    /// </summary>
    bool CanLoad(string sceneName)
    {
        return !string.IsNullOrEmpty(sceneName)
            && Application.CanStreamedLevelBeLoaded(sceneName);
    }
}