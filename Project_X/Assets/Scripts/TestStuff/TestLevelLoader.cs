using UnityEngine;

public class TestLevelLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelManager.Instance.LoadLevel(0);
    }
}
