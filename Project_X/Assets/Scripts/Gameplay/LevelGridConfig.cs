using UnityEngine;

/// <summary>
/// Define a grade lógica do level (largura/altura em tiles) ancorada em (0,0),
/// e ajusta automaticamente o BoxCollider2D do "Bounds" para crescer
/// para a DIREITA (+X) e para CIMA (+Y) quando Width/Height aumentam.
/// 
/// Use com:
/// - Um filho "Bounds" com BoxCollider2D (IsTrigger ON).
/// - Um filho "BoardRoot" contendo todos os tiles/boxes/goals do tabuleiro.
/// 
/// Fluxo:
/// 1) Garanta que todos os objetos do board fiquem com posições >= 0 em X/Y.
///    (Se não, use o menu: ContextMenu -> "Normalize Bottom-Left Origin")
/// 2) Ajuste Width/Height aqui. O collider vai cobrir [0..Width-1] x [0..Height-1]
///    e, visualmente, o tabuleiro terá pivot efetivo no canto inferior esquerdo.
/// </summary>
[ExecuteAlways]
public class LevelGridConfig : MonoBehaviour
{
    [Header("Grid Dimensions (in tiles)")]
    [Min(1)] public int Width = 8;
    [Min(1)] public int Height = 8;

    [Header("Cell Size")]
    [Tooltip("1.0 se 1 unidade = 1 tile. Ajuste se usar escala diferente.")]
    public float CellSize = 1f;

    [Header("References")]
    [Tooltip("RAÍZ DO NÍVEL! Raiz que contém TODO o conteúdo do tabuleiro (paredes, boxes, goals...).")]
    public Transform BoardRoot;
    [Tooltip("Collider do objeto 'Bounds' (IsTrigger ON).")]
    public BoxCollider2D BoundsCollider;

    private void Reset()
    {
        // Tenta encontrar referências automaticamente
        if (BoardRoot == null)
        {
            var t = transform.Find("BoardRoot");
            if (t != null) BoardRoot = t;
        }

        if (BoundsCollider == null)
        {
            var b = transform.Find("Bounds");
            if (b != null) BoundsCollider = b.GetComponent<BoxCollider2D>();
        }

        ApplyBounds(); // inicial
    }

    private void OnValidate()
    {
        // Sempre que mudar Width/Height/CellSize no Inspector, atualiza o Bounds.
        ApplyBounds();
    }

    /// <summary>
    /// Ajusta o BoxCollider2D do Bounds para cobrir de (0,0) até (Width-1, Height-1),
    /// crescendo sempre em +X/+Y. O "offset" funciona como o "center" em coordenadas locais.
    /// </summary>
    public void ApplyBounds()
    {
        if (BoundsCollider == null) return;

        // Tamanho do collider em unidades do mundo
        var size = new Vector2(Width * CellSize, Height * CellSize);

        // Centro do collider (local) precisa ficar no MEIO do retângulo
        // Para cobrir [0..Width-1] e [0..Height-1], o centro local fica em:
        // ((Width-1)/2, (Height-1)/2) * CellSize
        var center = new Vector2(((Width - 1) * 0.5f) * CellSize,
                                 ((Height - 1) * 0.5f) * CellSize);

        BoundsCollider.isTrigger = true;
        BoundsCollider.size = size;
        BoundsCollider.offset = center; // offset local do BoxCollider2D

        // Garante snap visual do objeto Bounds em si (posição do Transform), opcional
        var t = BoundsCollider.transform;
        t.localPosition = new Vector3(Mathf.Round(t.localPosition.x), Mathf.Round(t.localPosition.y), t.localPosition.z);
        t.localScale = Vector3.one;
    }

    /// <summary>
    /// Normaliza o conteúdo: desloca o BoardRoot para que o menor X/Y dos filhos
    /// vire (0,0). Útil para padronizar um level já existente.
    /// </summary>
    [ContextMenu("Normalize Bottom-Left Origin")]
    public void NormalizeBottomLeftOrigin()
    {
        if (BoardRoot == null) return;

        bool hasAny = false;
        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        foreach (Transform child in BoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == BoardRoot) continue;
            var p = child.localPosition;
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            hasAny = true;
        }

        if (!hasAny) return;

        // Desloca todo mundo de forma que o ponto mais à esquerda/baixo vire (0,0).
        Vector3 delta = new Vector3(-minX, -minY, 0f);
        foreach (Transform child in BoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == BoardRoot) continue;
            child.localPosition += delta;
        }

        // Opcional: snap para inteiros (mantém grid limpo)
        SnapChildrenToGrid();

        // Após normalizar, o Bounds pode ser reaplicado (caso deseje)
        ApplyBounds();
        Debug.Log("[LevelGridConfig] NormalizeBottomLeftOrigin: conteúdo realocado para iniciar em (0,0).");
    }

    /// <summary>
    /// Snapa todos os filhos do BoardRoot para coordenadas inteiras (XY).
    /// </summary>
    [ContextMenu("Snap Children To Grid")]
    public void SnapChildrenToGrid()
    {
        if (BoardRoot == null) return;

        foreach (Transform child in BoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == BoardRoot) continue;
            var p = child.localPosition;
            child.localPosition = new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), p.z);
        }
        Debug.Log("[LevelGridConfig] SnapChildrenToGrid: concluído.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Gizmo do retângulo do board em espaço LOCAL do Level
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.25f);
        var size = new Vector3(Width * CellSize, Height * CellSize, 0f);
        var center = new Vector3(((Width - 1) * 0.5f) * CellSize, ((Height - 1) * 0.5f) * CellSize, 0f);
        Gizmos.DrawCube(center, size);

        // Eixos de referência no (0,0)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, new Vector3(Width * CellSize, 0f, 0f));
        Gizmos.DrawLine(Vector3.zero, new Vector3(0f, Height * CellSize, 0f));
    }
#endif
}
