using UnityEngine;

/// <summary>
/// Exibe feedback visual no Goal quando uma Box está exatamente sobre ele.
/// Como detecta?
///   - Faz Physics2D.OverlapPoint no CENTRO do goal, usando a mesma LayerMask "solid"
///     que o Player usa (paredes/caixas).
///   - Se o collider encontrado (no ponto) tiver BoxIdentifier -> "goal coberto".
/// Visual:
///   - Ou aplica tint de cor no SpriteRenderer (useTint = true)
///   - Ou troca o sprite por um "aceso" (useTint = false)
/// Performance:
///   - Checa a cada frame, mas só atualiza visual quando o estado muda.
///   - Tudo em grid/snap (centros inteiros) -> OverlapPoint é barato e preciso.
/// </summary>
[DisallowMultipleComponent]
public class GoalHighlighter : MonoBehaviour
{
    [Header("Detecção")]
    [Tooltip("Mesma LayerMask de sólidos usada pelo Player (deve incluir as Boxes).")]
    public LayerMask solidMask;

    [Header("Renderer (obrigatório)")]
    public SpriteRenderer sr;

    [Header("Modo de Feedback")]
    [Tooltip("Se verdadeiro, aplica tint de cor. Se falso, faz troca de sprite.")]
    public bool useTint = true;

    [Header("Feedback por Tint (se useTint = true)")]
    public Color unlitColor = Color.white;
    public Color litColor   = new Color(1f, 0.95f, 0.4f, 1f); // leve amarelo "aceso"

    [Header("Feedback por Sprite (se useTint = false)")]
    public Sprite unlitSprite;
    public Sprite litSprite;

    // cache de estado pra evitar set repetido no renderer
    private bool lastCovered;

    private void Reset()
    {
        // Tenta pegar o SpriteRenderer do próprio GO ao adicionar o script
        if (!sr) sr = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        // Inicializa estado visual de acordo com a detecção atual
        ApplyVisual(IsCovered());
    }

    private void Update()
    {
        bool covered = IsCovered();
        if (covered != lastCovered)
        {
            ApplyVisual(covered);
        }
    }

    /// <summary>
    /// Retorna true se há uma Box (BoxIdentifier) exatamente no centro deste Goal.
    /// </summary>
    private bool IsCovered()
    {
        // Centro do goal (IMPORTANTE: tudo no grid e z=0)
        Vector2 p = transform.position;

        // Há algo "sólido" no centro?
        var hit = Physics2D.OverlapPoint(p, solidMask);
        if (!hit) return false;

        // É uma Box?
        var box = hit.GetComponent<BoxIdentifier>() ?? hit.GetComponentInParent<BoxIdentifier>();
        return box != null;
    }

    /// <summary>
    /// Aplica o feedback visual conforme estado "coberto".
    /// </summary>
    private void ApplyVisual(bool covered)
    {
        lastCovered = covered;

        if (!sr) return;

        if (useTint)
        {
            // Modo Tint: só muda cor
            sr.color = covered ? litColor : unlitColor;
        }
        else
        {
            // Modo Sprite Swap: troca sprite, preserva cor atual
            if (covered && litSprite != null)
                sr.sprite = litSprite;
            else if (!covered && unlitSprite != null)
                sr.sprite = unlitSprite;
        }
    }
}