using UnityEngine;

public class BoxGoalState : MonoBehaviour
{
    public bool IsOnGoal { get; private set; }

    [Header("Highlight (UnderSprite)")]
    [SerializeField] SpriteRenderer highlight; // arraste o SR do UnderSprite
    [SerializeField] bool startHidden = true;  // começa apagado fora do goal

    void Awake()
    {
        if (!highlight) highlight = transform.Find("UnderSprite")?.GetComponent<SpriteRenderer>();
        if (highlight && startHidden) highlight.enabled = false; // começa invisível
    }

    public void SetOnGoal(bool v)
    {
        IsOnGoal = v;
        if (highlight) highlight.enabled = v; // mostra/esconde o tapete cinza
    }
}