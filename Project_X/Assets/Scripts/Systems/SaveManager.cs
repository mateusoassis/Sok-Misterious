using UnityEngine;

/// <summary>
/// Gerencia o progresso salvo do jogador usando PlayerPrefs.
/// Aqui controlamos apenas *qual nível máximo já foi desbloqueado*.
/// 
/// Funciona assim:
/// - Usa a chave nova: "progress.highestUnlockedIndex"
/// - Se encontrar chave antiga (legacy), migra automaticamente
/// - Mantém o valor em cache (_highest) após a primeira leitura
/// - Oferece operações simples: Load, Save, UnlockUpTo e Reset
/// </summary>
public static class SaveManager
{
    // Chave oficial nova — esta deve ser usada daqui pra frente
    const string KEY_HIGHEST = "progress.highestUnlockedIndex";

    // Chave antiga legada, usada pelas versões anteriores do WinChecker
    const string KEY_LEGACY = "highestUnlocked";

    // Cache do valor carregado
    static int _highest = -1;

    // Flag indicando se já fizemos Load()
    static bool _loaded = false;

    /// <summary>
    /// Retorna o maior índice de nível desbloqueado.
    /// Garante que Load() terá sido chamado.
    /// </summary>
    public static int HighestUnlockedIndex
    {
        get
        {
            if (!_loaded) Load();
            return _highest;
        }
    }

    /// <summary>
    /// Lê PlayerPrefs e carrega o maior nível desbloqueado.
    /// Também executa migração se detectar chave antiga.
    /// </summary>
    public static void Load()
    {
        // Lê da chave nova (com default = 0)
        int fromNew = PlayerPrefs.GetInt(KEY_HIGHEST, 0);

        // Lê da chave antiga, caso ainda exista
        int fromLegacy = PlayerPrefs.GetInt(KEY_LEGACY, 0);

        // Fica com o maior entre os dois (protege o progresso do jogador)
        _highest = Mathf.Max(fromNew, fromLegacy);

        // Se a chave antiga tinha valor maior, então precisamos migrar
        if (fromLegacy > fromNew)
        {
            // Escreve o valor correto na chave nova
            PlayerPrefs.SetInt(KEY_HIGHEST, _highest);

            // Remove a antiga de vez
            PlayerPrefs.DeleteKey(KEY_LEGACY);

            PlayerPrefs.Save();
            Debug.Log($"[SaveManager] Migrado '{KEY_LEGACY}' → '{KEY_HIGHEST}' = {_highest}");
        }

        _loaded = true;
        Debug.Log($"[SaveManager] LOAD highestUnlockedIndex={_highest}");
    }

    /// <summary>
    /// Salva o valor atual (_highest) no PlayerPrefs.
    /// </summary>
    public static void Save()
    {
        if (!_loaded)
            _loaded = true; // Se Save for chamado antes de Load, assume carregado

        PlayerPrefs.SetInt(KEY_HIGHEST, Mathf.Max(0, _highest));
        PlayerPrefs.Save();

        Debug.Log($"[SaveManager] SAVE highestUnlockedIndex={_highest}");
    }

    /// <summary>
    /// Desbloqueia até o nível 'index'.
    /// Só modifica algo se 'index' for MAIOR que o valor atual.
    /// </summary>
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

    /// <summary>
    /// Reseta completamente o progresso gravado.
    /// Deixa ambos: chave nova = 0 e chave antiga removida.
    /// </summary>
    public static void ResetProgress()
    {
        // Zera progresso salvo
        PlayerPrefs.SetInt(KEY_HIGHEST, 0);

        // Remove chave legacy se existir
        if (PlayerPrefs.HasKey(KEY_LEGACY))
            PlayerPrefs.DeleteKey(KEY_LEGACY);

        PlayerPrefs.Save();

        // Força Load() a recarregar da próxima vez
        _loaded = false;

        Debug.Log("[SaveManager] RESET progress → highest=0");
    }
}