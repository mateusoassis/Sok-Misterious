# GoalIdentifier

## Visão Geral
`GoalIdentifier` é um **componente marcador** (marker component), exatamente como `BoxIdentifier`, mas usado para identificar **células de Goal** no nível.

Ele não contém lógica — seu único propósito é permitir que outros sistemas (como highlights, contagem de goals, HUD, validação e o LevelManager) encontrem rapidamente todos os Goals da cena sem depender de tags, nomes ou hierarquias específicas.

---

## Para que serve?

- Identificar quais objetos são **Goals** no nível.
- Permitir que sistemas como:
  - `GoalHighlighter`
  - contadores de objetivos
  - validadores de vitória
  - LevelManager (quando for necessário)

  possam buscar:
  ```csharp
  currentLevel.GetComponentsInChildren<GoalIdentifier>(true);
  ```

- Evitar uso de strings como tags (que podem ser renomeadas ou removidas por acidente).
- Tornar o prefab de Goal autoexplicativo.

---

## Uso típico

### Nos prefabs de Goal:
- Basta adicionar `GoalIdentifier` no objeto principal (que fica na célula).
- O sprite, colisor ou highlight ficam em outros componentes.

### Nos scripts consumidores:
```csharp
var goals = currentLevel.GetComponentsInChildren<GoalIdentifier>(true);
```

---

## Benefícios do padrão

- **Clean code**: separa “identificação” de “comportamento”.
- **Segurança**: sem strings mágicas nem tags frágeis.
- **Performance**: busca por tipo é extremamente rápida.
- **Flexibilidade**: fácil adicionar novos comportamentos a Goals no futuro.

---

## Código original

```csharp
using UnityEngine;

public class GoalIdentifier : MonoBehaviour
{
    // mesma ideia do BoxIdentifier
    // os "GOALS" terão esse script
}
```