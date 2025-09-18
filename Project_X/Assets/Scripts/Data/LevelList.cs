using UnityEngine;

[CreateAssetMenu(menuName = "Sokoban/Level List", fileName = "LevelList")]
public class LevelList : ScriptableObject
{
    public LevelEntry[] levels;
}

[System.Serializable]
public class LevelEntry
{
    public string displayName;
    public GameObject levelPrefab;
}