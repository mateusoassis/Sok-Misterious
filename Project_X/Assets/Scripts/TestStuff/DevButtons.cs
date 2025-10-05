using UnityEngine;
using UnityEngine.SceneManagement;

public class DevProgressButtons : MonoBehaviour
{
    [Header("Comportamento")]
    [SerializeField] bool goToLevelSelectAfter = true;
    [SerializeField] string levelSelectScene = "01_LevelSelect";
    [SerializeField] string gameScene        = "02_Game";

    [Header("Debug")]
    [SerializeField] bool verboseLogs = true;

    // --------- BOTÕES ---------

    public void OnClickResetHighest()
    {
        ClosePauseIfOpen();

        if (verboseLogs) Debug.Log("[DEV] ResetHighest: SaveManager.ResetProgress() → highest=0");
        SaveManager.ResetProgress();               // zera progresso (highest=0)
        SaveManager.Load();                        // recarrega default em memória

        JumpAfterReset();
    }

    public void OnClickResetPlayerPrefs()
    {
        ClosePauseIfOpen();

        if (verboseLogs) Debug.Log("[DEV] ResetPlayerPrefs: PlayerPrefs.DeleteAll()");
        PlayerPrefs.DeleteAll();                   // APAGA TUDO
        PlayerPrefs.Save();

        SaveManager.Load();                        // garante default em memória

        JumpAfterReset();
    }

    // --------- SUPORTE ---------

    void JumpAfterReset()
    {
        Time.timeScale = 1f;

        if (goToLevelSelectAfter)
        {
            if (!CanLoad(levelSelectScene))
            {
                Debug.LogError($"[DEV] Cena '{levelSelectScene}' não está no Build Settings.");
                return;
            }
            if (verboseLogs) Debug.Log($"[DEV] Carregando '{levelSelectScene}'…");
            SceneManager.LoadScene(levelSelectScene, LoadSceneMode.Single);
        }
        else
        {
            // Fica na 02_Game e só recarrega o nível atual pra refletir HUD
            if (LevelManager.Instance != null)
            {
                if (verboseLogs) Debug.Log("[DEV] Reload do nível atual…");
                LevelManager.Instance.Reload();
            }
            else
            {
                if (!CanLoad(gameScene))
                {
                    Debug.LogError($"[DEV] Cena '{gameScene}' não está no Build Settings.");
                    return;
                }
                if (verboseLogs) Debug.Log($"[DEV] Carregando '{gameScene}'…");
                SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
            }
        }
    }

    void ClosePauseIfOpen()
    {
        if (PauseMenu.Instance != null)
        {
            if (verboseLogs) Debug.Log("[DEV] Fechando Pause…");
            PauseMenu.Instance.Hide();
        }
    }

    bool CanLoad(string sceneName)
    {
        // funciona pra cenas adicionadas no Build Settings
        return !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
    }
}