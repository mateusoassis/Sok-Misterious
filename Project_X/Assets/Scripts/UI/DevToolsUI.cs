using UnityEngine;

/// <summary>
/// Pequeno utilitário de UI para botões de debug.
/// Usado apenas para resetar PlayerPrefs rapidamente por botões no Editor.
/// </summary>
public class DevToolsUI : MonoBehaviour
{
    /// <summary>
    /// Reseta TODA a PlayerPrefs — literalmente APAGA TUDO.
    /// Ideal para testes de estado inicial do jogo.
    /// </summary>
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();   // Apaga absolutamente todas as chaves
        PlayerPrefs.Save();        // Força gravação imediata

        Debug.Log("[DevTools] PlayerPrefs resetados!");
    }

    /// <summary>
    /// Reseta apenas a chave 'highestUnlocked', usada no progresso do jogador.
    /// Mantém todo o resto intacto (configs, volume etc.).
    /// </summary>
    public void ResetHighestUnlocked()
    {
        PlayerPrefs.DeleteKey("highestUnlocked");  // Remove somente essa chave específica
        PlayerPrefs.Save();                        // Salva

        Debug.Log("[DevTools] highestUnlocked resetado!");
    }
}