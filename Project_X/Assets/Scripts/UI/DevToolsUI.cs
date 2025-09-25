using UnityEngine;

public class DevToolsUI : MonoBehaviour
{
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();   // apaga tudo
        PlayerPrefs.Save();
        Debug.Log("[DevTools] PlayerPrefs resetados!");
    }
    public void ResetHighestUnlocked()
    {
        PlayerPrefs.DeleteKey("highestUnlocked");   // apaga tudo
        PlayerPrefs.Save();
        Debug.Log("[DevTools] highestUnocked resetado!");
    }
}

