# GoalHighlighter

## Visão Geral
`GoalHighlighter` é o componente responsável por dar **feedback visual** aos Goals do Sokoban, indicando quando uma caixa está corretamente posicionada em cima deles.

Ele detecta se o Goal está sendo “coberto” por uma Box usando **colisão pontual** (`Physics2D.OverlapPoint`) no centro do Goal e atualiza a aparência do sprite (cor ou troca de sprite).

Serve para dar clareza ao jogador e reforçar visualmente o progresso do puzzle.

---

## Como funciona a detecção?

A cada frame (`Update`):
1. O script chama `IsCovered()`
2. Ele executa:
   ```csharp
   Physics2D.OverlapPoint(transform.position, solidMask)
   ```
3. Se o collider encontrado tiver `BoxIdentifier`, considera o Goal “coberto”.

**Por que no centro?**  
O jogo trabalha com grid e snapping — então o centro do tile é sempre um ponto consistente e preciso para testar.

**Por que LayerMask?**  
O mesmo `solidMask` usado pelo Player evita interferências com objetos irrelevantes.

---

## Modos de Feedback Visual

### 1. Modo Tint (`useTint = true`)
O SpriteRenderer recebe:
- `unlitColor` quando vazio  
- `litColor` quando coberto  

Mais leve e sem necessidade de sprites extras.

---

### 2. Modo de Troca de Sprite (`useTint = false`)
O script troca entre:
- `unlitSprite`
- `litSprite`

Ideal para artes mais elaboradas (ex.: goal apagado/aceso).

---

## Performance

- Apenas **um OverlapPoint por frame**, extremamente barato.
- Só chama `ApplyVisual()` quando o estado muda (reduz chamadas ao renderer).
- Funciona em grid → alta precisão e baixa ambiguidade.

---

## Eventos Internos

- `Reset()` tenta encontrar automaticamente um SpriteRenderer ao adicionar o script.
- `Awake()` aplica o estado visual inicial.
- `Update()` verifica mudanças por frame.
- `ApplyVisual()` muda cor ou sprite eficientemente.

---

## Integração com o jogo
- Não depende do LevelManager.
- Não depende da lógica de goals do LevelManager.
- Apenas reage à presença física da caixa no tile.

Isso permite mover o Goal para qualquer posição, prefab ou cena — ele se autogerencia.

---

## Código original

```csharp
using UnityEngine;

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
```