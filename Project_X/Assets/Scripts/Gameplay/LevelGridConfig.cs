using UnityEngine;

/// <summary>
/// Configura a grade lógica de um nível (largura/altura em tiles) ancorada em (0,0)
/// e ajusta automaticamente o BoxCollider2D do objeto "Bounds".
///
/// O collider sempre cresce para +X e +Y conforme Width / Height.
/// Assim, toda a área válida do tabuleiro fica no retângulo [0..Width-1] x [0..Height-1].
///
/// Como usar:
/// - O GameObject do level deve ter:
///     • Um filho "Bounds" com BoxCollider2D (IsTrigger = ON)
///     • Um filho "BoardRoot" contendo TODO o conteúdo (parede, caixas, goals etc)
///
/// Fluxo recomendado de criação de nível:
/// 1) Monte o layout dentro de BoardRoot.
/// 2) Utilize “Normalize Bottom-Left Origin” para garantir que o menor X/Y dos tiles vire (0,0).
/// 3) Ajuste Width/Height/CellSize no inspector — o collider será atualizado automaticamente.
/// </summary>
[ExecuteAlways]
public class LevelGridConfig : MonoBehaviour
{
    [Header("Grid Dimensions (in tiles)")]
    [Min(1)] public int Width = 8;
    [Min(1)] public int Height = 8;

    [Header("Cell Size")]
    [Tooltip("1.0 = 1 unidade do Unity = 1 tile. Ajuste se usar escala diferenciada.")]
    public float CellSize = 1f;

    [Header("References")]
    [Tooltip("Raiz que contém todos os objetos do tabuleiro.")]
    public Transform BoardRoot;

    [Tooltip("BoxCollider2D do objeto 'Bounds' (deve estar com IsTrigger ON).")]
    public BoxCollider2D BoundsCollider;

    private void Reset()
    {
        // tenta achar BoardRoot automaticamente
        if (BoardRoot == null)
        {
            var t = transform.Find("BoardRoot");
            if (t != null) BoardRoot = t;
        }

        // tenta achar o collider do Bounds automaticamente
        if (BoundsCollider == null)
        {
            var b = transform.Find("Bounds");
            if (b != null) BoundsCollider = b.GetComponent<BoxCollider2D>();
        }

        ApplyBounds(); // aplica ao reset
    }

    private void OnValidate()
    {
        // toda vez que alterar os valores no inspector
        ApplyBounds();
    }

    /// <summary>
    /// Ajusta o BoxCollider2D do Bounds para cobrir de (0,0) até (Width-1, Height-1).
    /// O offset do collider é configurado como o centro dessa área.
    /// </summary>
    public void ApplyBounds()
    {
        if (BoundsCollider == null)
            return;

        // tamanho do collider
        var size = new Vector2(Width * CellSize, Height * CellSize);

        // centro local do collider
        var center = new Vector2(
            ((Width - 1) * 0.5f) * CellSize,
            ((Height - 1) * 0.5f) * CellSize
        );

        BoundsCollider.isTrigger = true;
        BoundsCollider.size = size;
        BoundsCollider.offset = center;

        // garante que o objeto Bounds em si não está com escalas estranhas
        var t = BoundsCollider.transform;
        t.localScale = Vector3.one;

        // snap opcional no Transform só pra manter organização
        t.localPosition = new Vector3(
            Mathf.Round(t.localPosition.x),
            Mathf.Round(t.localPosition.y),
            t.localPosition.z
        );
    }

    /// <summary>
    /// Normaliza o tabuleiro para que o menor X e Y entre todos os tiles vire (0,0).
    /// Útil se o level foi montado fora do grid, com coordenadas negativas ou tortas.
    /// </summary>
    [ContextMenu("Normalize Bottom-Left Origin")]
    public void NormalizeBottomLeftOrigin()
    {
        if (BoardRoot == null)
            return;

        bool hasAny = false;
        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        foreach (Transform child in BoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == BoardRoot) continue;

            Vector3 p = child.localPosition;
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            hasAny = true;
        }

        if (!hasAny)
            return;

        Vector3 delta = new Vector3(-minX, -minY, 0f);

        foreach (Transform child in BoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == BoardRoot) continue;
            child.localPosition += delta;
        }

        SnapChildrenToGrid();
        ApplyBounds();
        Debug.Log("[LevelGridConfig] NormalizeBottomLeftOrigin: Reposicionado para iniciar em (0,0).");
    }

    /// <summary>
    /// Snapa todos os filhos do BoardRoot para coordenadas inteiras.
    /// Útil para manter grid 100% limpo.
    /// </summary>
    [ContextMenu("Snap Children To Grid")]
    public void SnapChildrenToGrid()
    {
        if (BoardRoot == null)
            return;

        foreach (Transform child in BoardRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == BoardRoot) continue;
            Vector3 p = child.localPosition;
            child.localPosition = new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), p.z);
        }

        Debug.Log("[LevelGridConfig] SnapChildrenToGrid: finalizado.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Desenha o retângulo da área do level no Editor
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(0f, 1f, 0.6f, 0.25f);
        var size = new Vector3(Width * CellSize, Height * CellSize, 0f);
        var center = new Vector3(
            ((Width - 1) * 0.5f) * CellSize,
            ((Height - 1) * 0.5f) * CellSize,
            0f
        );
        Gizmos.DrawCube(center, size);

        // eixos visuais
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, new Vector3(Width * CellSize, 0f, 0f));
        Gizmos.DrawLine(Vector3.zero, new Vector3(0f, Height * CellSize, 0f));
    }
#endif
}