using UnityEngine;
using System.Collections;

public class GameBootstrapper : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Espera 1 frame pra todos os Awake/OnEnable da cena rodarem
        yield return null;

        const string Key = "highestUnlocked";
        if (!PlayerPrefs.HasKey(Key)) { PlayerPrefs.SetInt(Key, 0); PlayerPrefs.Save(); }

        int index = 0;
        if (LaunchArgs.PendingLevel.HasValue)
            index = Mathf.Max(0, LaunchArgs.PendingLevel.Value);

        Debug.Log($"[Bootstrap] Start → PendingLevel={LaunchArgs.PendingLevel}, chosenIndex={index}");

        if (LevelManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] LevelManager.Instance == null na 02_Game");
            yield break;
        }

        var lm = LevelManager.Instance;
        Debug.Log($"[Bootstrap] LM ok. LevelCount={lm.LevelCount}, levelListNull? {lm.levelList == null}");

        lm.LoadLevel(index);
        LaunchArgs.PendingLevel = null; // consumiu
    }
}
