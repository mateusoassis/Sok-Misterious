using UnityEngine;

public class Snapper : MonoBehaviour
{
    [ContextMenu("Snap children to grid (1 unit)")]
    void SnapChildren()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t == transform) continue;
            var p = t.position;
            t.position = new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), p.z);
        }
        Debug.Log("[Snapper] Done.");
    }
}
