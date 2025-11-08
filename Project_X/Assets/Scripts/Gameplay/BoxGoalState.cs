using UnityEngine;

public class BoxGoalState : MonoBehaviour
{
    public bool IsOnGoal { get; private set; }

    [Header("Sprite da Caixa (Patinho)")]
    [Tooltip("SpriteRenderer do patinho (ex.: child 'BoxSprite').")]
    [SerializeField] SpriteRenderer boxRenderer;

    [Tooltip("Sprite normal (patinho solto). Se deixar vazio, usa o sprite atual como padrão.")]
    [SerializeField] Sprite normalSprite;

    [Tooltip("Sprite quando estiver em cima do goal (patinho no travesseiro).")]
    [SerializeField] Sprite onGoalSprite;

    void Awake()
    {
        // tenta achar o SR da box sozinho, se não foi arrastado
        if (!boxRenderer)
        {
            var child = transform.Find("BoxSprite");
            if (child) boxRenderer = child.GetComponent<SpriteRenderer>();

            if (!boxRenderer)
                boxRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        // se não setou normalSprite no inspector, usa o sprite atual como "normal"
        if (boxRenderer && normalSprite == null)
            normalSprite = boxRenderer.sprite;
    }

    public void SetOnGoal(bool v)
    {
        IsOnGoal = v;

        if (!boxRenderer)
            return;

        if (v && onGoalSprite != null)
        {
            boxRenderer.sprite = onGoalSprite;
        }
        else if (!v && normalSprite != null)
        {
            boxRenderer.sprite = normalSprite;
        }
    }

    // se em algum momento você mudar IsOnGoal manualmente e quiser re-aplicar visual:
    public void Refresh()
    {
        SetOnGoal(IsOnGoal);
    }
}