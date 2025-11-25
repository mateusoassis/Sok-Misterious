using UnityEngine;
using System.Collections;

public class GameBootstrapper : MonoBehaviour
{
    // Coroutine de inicialização da cena de jogo (02_Game)
    private IEnumerator Start()
    {
        // Espera 1 frame pra garantir que todos os Awake/OnEnable já rodaram
        yield return null;

        // Garante que a chave de progresso exista no PlayerPrefs
        const string Key = "highestUnlocked";
        if (!PlayerPrefs.HasKey(Key))
        {
            PlayerPrefs.SetInt(Key, 0);
            PlayerPrefs.Save();
        }

        // Define qual índice de level será carregado
        int index = 0;
        if (LaunchArgs.PendingLevel.HasValue)
            // Protege contra índices negativos vindos de fora
            index = Mathf.Max(0, LaunchArgs.PendingLevel.Value);

        Debug.Log($"[Bootstrap] Start → PendingLevel={LaunchArgs.PendingLevel}, chosenIndex={index}");

        // Confere se o LevelManager singleton está disponível na cena
        if (LevelManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] LevelManager.Instance == null na 02_Game");
            yield break; // Não tem como continuar sem LevelManager
        }

        // Referência local pro LevelManager (qualidade de vida + log)
        var lm = LevelManager.Instance;
        Debug.Log($"[Bootstrap] LM ok. LevelCount={lm.LevelCount}, levelListNull? {lm.levelList == null}");

        // Carrega o nível inicial decidido acima
        lm.LoadLevel(index);

        // Consome o argumento de lançamento pra não reaplicar na próxima vez
        LaunchArgs.PendingLevel = null;
    }
}