using UnityEngine;

public class BoxGoalState : MonoBehaviour
{
    // Indica se a caixa está posicionada sobre um Goal
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
        // Se o SpriteRenderer não for arrastado no Inspector, tenta encontrá-lo automaticamente
        if (!boxRenderer)
        {
            // 1) Tenta encontrar um child específico chamado "BoxSprite"
            var child = transform.Find("BoxSprite");
            if (child) boxRenderer = child.GetComponent<SpriteRenderer>();

            // 2) Se ainda não encontrou, pega o primeiro SpriteRenderer em filhos
            if (!boxRenderer)
                boxRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        // Se não houver sprite "normal" definido, usa o sprite atual como padrão
        if (boxRenderer && normalSprite == null)
            normalSprite = boxRenderer.sprite;
    }

    // Atualiza IsOnGoal e troca o sprite conforme o estado
    public void SetOnGoal(bool v)
    {
        IsOnGoal = v;

        if (!boxRenderer)
            return;

        // Se está em goal, usa o sprite de "caixa encaixada"
        if (v && onGoalSprite != null)
        {
            boxRenderer.sprite = onGoalSprite;
        }
        // Se não está, volta para o sprite normal
        else if (!v && normalSprite != null)
        {
            boxRenderer.sprite = normalSprite;
        }
    }

    // Reaplica o estado visual caso algo tenha mudado externamente
    public void Refresh()
    {
        SetOnGoal(IsOnGoal);
    }
}