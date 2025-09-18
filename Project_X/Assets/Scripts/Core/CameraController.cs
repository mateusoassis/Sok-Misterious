using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void FocusOnBounds(Bounds b)
    {
        transform.position = new Vector3(b.center.x, b.center.y, transform.position.z);

        float camHeight = b.size.y / 2f;
        float camWidth = b.size.x / 2f / cam.aspect;
        cam.orthographicSize = Mathf.Max(camHeight, camWidth);

        Debug.Log($"[CameraController] ajustada para bounds {b.size}");
    }
}
