#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LevelValidationTools
{
    // Ajusta se sua pasta de levels for outra
    private const string LevelsFolder = "Assets/Prefabs/Levels";

    [MenuItem("Tools/Levels/Validate All Levels (no snap)")]
    public static void ValidateAllLevelsNoSnap()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder });

        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning($"[LevelValidationTools] Nenhum prefab encontrado em '{LevelsFolder}'.");
            return;
        }

        Debug.Log($"[LevelValidationTools] Validando {guids.Length} prefabs em '{LevelsFolder}' (SEM SNAP, usando instância temporária)...");

        int total = 0;
        int ok = 0;
        int missingValidator = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            total++;

            // Instancia o prefab temporariamente na cena
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogWarning($"[LevelValidationTools] Não foi possível instanciar prefab '{path}'.");
                continue;
            }

            try
            {
                var validator = instance.GetComponent<LevelValidator>();
                if (validator == null)
                {
                    Debug.LogWarning($"[LevelValidationTools] Prefab '{path}' não tem LevelValidator na raiz.");
                    missingValidator++;
                    continue;
                }

                // Força validação SEM SNAP
                bool originalSnap = validator.snapToGridOnValidate;
                validator.snapToGridOnValidate = false;

                Debug.Log($"[LevelValidationTools] --- Validando '{path}' ---");
                validator.ValidateLevel();
                ok++;

                // restaura flag
                validator.snapToGridOnValidate = originalSnap;
            }
            finally
            {
                // destrói a instância temporária
                Object.DestroyImmediate(instance);
            }
        }

        Debug.Log($"[LevelValidationTools] FIM: {total} prefabs, {ok} validados (instância), {missingValidator} sem LevelValidator.");
    }
}
#endif