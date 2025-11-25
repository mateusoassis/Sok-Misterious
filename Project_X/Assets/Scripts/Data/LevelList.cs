using UnityEngine;

// Asset de configuração que lista todos os níveis disponíveis do jogo.
// Crie via menu: Create → Sokoban → Level List
[CreateAssetMenu(menuName = "Sokoban/Level List", fileName = "LevelList")]
public class LevelList : ScriptableObject
{
    // Array de entradas de nível. Cada entrada aponta para um prefab de level.
    public LevelEntry[] levels;
}

// Entrada individual de nível, exibida no Inspector dentro do LevelList.
[System.Serializable]
public class LevelEntry
{
    // Nome amigável para debug/Level Select/logs.
    public string displayName;

    // Prefab do nível (layout completo + Bounds + entidades).
    public GameObject levelPrefab;
}