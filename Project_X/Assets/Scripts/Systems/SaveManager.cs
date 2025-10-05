using UnityEngine;

public static class SaveManager
{
    // chave “oficial”
    const string KEY_HIGHEST = "progress.highestUnlockedIndex";
    // se você usou um key antigo antes (no WinChecker):
    const string KEY_LEGACY = "highestUnlocked";

    static int _highest = -1;
    static bool _loaded = false;

    public static int HighestUnlockedIndex
    {
        get { if (!_loaded) Load(); return _highest; }
    }

    public static void Load()
    {
        int fromNew = PlayerPrefs.GetInt(KEY_HIGHEST, 0);
        int fromLegacy = PlayerPrefs.GetInt(KEY_LEGACY, 0);
        _highest = Mathf.Max(fromNew, fromLegacy);

        // migra (se legacy > new)
        if (fromLegacy > fromNew)
        {
            PlayerPrefs.SetInt(KEY_HIGHEST, _highest);
            PlayerPrefs.DeleteKey(KEY_LEGACY);
            PlayerPrefs.Save();
            Debug.Log($"[SaveManager] Migrado '{KEY_LEGACY}' → '{KEY_HIGHEST}' = {_highest}");
        }

        _loaded = true;
        Debug.Log($"[SaveManager] LOAD highestUnlockedIndex={_highest}");
    }

    public static void Save()
    {
        if (!_loaded) _loaded = true;
        PlayerPrefs.SetInt(KEY_HIGHEST, Mathf.Max(0, _highest));
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] SAVE highestUnlockedIndex={_highest}");
    }

    /// Desbloqueia até 'index' (idempotente).
    public static void UnlockUpTo(int index)
    {
        if (!_loaded) Load();
        if (index > _highest)
        {
            _highest = index;
            Save();
        }
        else
        {
            Debug.Log($"[SaveManager] Unlock ignorado: {index} <= atual={_highest}");
        }
    }

    /// Reseta só o progresso de “highest”.
    public static void ResetProgress()
    {
        PlayerPrefs.SetInt(KEY_HIGHEST, 0);
        if (PlayerPrefs.HasKey(KEY_LEGACY)) PlayerPrefs.DeleteKey(KEY_LEGACY);
        PlayerPrefs.Save();
        _loaded = false; // força reler default
        Debug.Log("[SaveManager] RESET progress → highest=0");
    }
}