using UnityEngine;

/// <summary>
/// Detecta se uma Box está exatamente em cima deste Goal
/// e aplica feedback visual (tint ou troca de sprite).
/// A detecção é feita com OverlapPoint no centro do tile.
/// </summary>
[DisallowMultipleComponent]
public class GoalHighlighter : MonoBehaviour
{
    [Header("Detecção")]
    [Tooltip("Mesmo LayerMask sólido usado pelo Player. Deve incluir caixas.")]
    public LayerMask solidMask;

    [Header("Renderer (obrigatório)")]
    public SpriteRenderer sr;

    [Header("Modo de Feedback")]
    [Tooltip("true = usa tint de cor; false = troca de sprite.")]
    public bool useTint = true;

    [Header("Feedback por Tint")]
    public Color unlitColor = Color.white;                     // Goal vazio
    public Color litColor   = new Color(1f, 0.95f, 0.4f, 1f);  // Goal aceso/amarelo

    [Header("Feedback por Sprite (se useTint = false)")]
    public Sprite unlitSprite;
    public Sprite litSprite;

    // Cache de estado para evitar updates desnecessários no renderer
    private bool lastCovered;

    private void Reset()
    {
        // Autopega o SpriteRenderer se o usuário arrastar o script no GO
        if (!sr) sr = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();

        // Aplica estado inicial imediatamente
        ApplyVisual(IsCovered());
    }

    private void Update()
    {
        bool covered = IsCovered();

        // Só atualiza visual se o estado mudou
        if (covered != lastCovered)
            ApplyVisual(covered);
    }

    /// <summary>
    /// Retorna true se existe uma Box (BoxIdentifier) no centro deste Goal.
    /// </summary>
    private bool IsCovered()
    {
        // Centro do goal (o jogo é todo grid → sempre posição inteira)
        Vector2 p = transform.position;

        // Checa se há algo sólido nesse ponto
        var hit = Physics2D.OverlapPoint(p, solidMask);
        if (!hit)
            return false;

        // Verifica se o objeto encontrado é uma Box
        var box = hit.GetComponent<BoxIdentifier>() ??
                  hit.GetComponentInParent<BoxIdentifier>();

        return box != null;
    }

    /// <summary>
    /// Aplica feedback visual conforme estado "coberto".
    /// </summary>
    private void ApplyVisual(bool covered)
    {
        lastCovered = covered;

        if (!sr) return;

        if (useTint)
        {
            // Apenas muda cor do sprite
            sr.color = covered ? litColor : unlitColor;
        }
        else
        {
            // Troca sprite preservando a cor
            if (covered && litSprite != null)
                sr.sprite = litSprite;
            else if (!covered && unlitSprite != null)
                sr.sprite = unlitSprite;
        }
    }
}