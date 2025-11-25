using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        // Cache da referência da câmera anexada a este GameObject
        cam = GetComponent<Camera>();
    }

    // Ajusta a câmera ortográfica para enquadrar todos os Bounds do nível
    public void FocusOnBounds(Bounds b)
    {
        // Centraliza a câmera no centro do nível (mantém o Z atual)
        transform.position = new Vector3(b.center.x, b.center.y, transform.position.z);

        // Calcula metade da altura do Bounds
        float camHeight = b.size.y / 2f;

        // Calcula a "altura equivalente" para a largura, compensando o aspect ratio
        float camWidth = b.size.x / 2f / cam.aspect;

        // Usa o maior valor — garante que a câmera caiba o nível inteiro
        cam.orthographicSize = Mathf.Max(camHeight, camWidth);

        Debug.Log($"[CameraController] ajustada para bounds {b.size}");
    }
}