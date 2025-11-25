# BoxIdentifier

## Visão Geral
`BoxIdentifier` é um componente **marcador** (marker component).  
Ele não possui lógica própria e existe apenas para permitir que outros sistemas identifiquem rapidamente quais objetos são **caixas** dentro do nível.

Esse padrão é comum em jogos com lógica baseada em tipos de entidade, como Sokoban.

---

## Para que serve?
- Permite que scripts como `LevelManager` localizem todas as caixas usando:
  ```csharp
  currentLevel.GetComponentsInChildren<BoxIdentifier>(true);
  ```
- Evita depender de tags, nomes de objetos ou estruturas complicadas.
- Torna o prefab das caixas **autoexplicativo**: se tem `BoxIdentifier`, é uma caixa.

---

## Integração
O `LevelManager.InitializeBoxHighlights()` usa exatamente esse componente para:

1. Encontrar todas as caixas.
2. Converter suas posições para grid.
3. Checar se cada uma está sobre uma célula de Goal.
4. Acionar o script `BoxGoalState` correspondente.

Sem esse marcador, o LevelManager teria que procurar componentes específicos ou depender de hierarquia/nome.

---

## Quando usar?
Sempre que um GameObject representar uma caixa empurrável do Sokoban:
- Prefab da caixa → **deve conter um BoxIdentifier**  
- Não precisa adicionar nenhum outro componente obrigatório nesse script

---

## Estrutura do Script

```csharp
using UnityEngine;

// eu só existo para marcar caixas com o script
public class BoxIdentifier : MonoBehaviour
{
}
```

Simples, limpo e funcional.

---

## Benefícios do padrão “marker component”
- **Clareza:** fácil de ver e entender no Inspector.
- **Segurança:** não depende de strings (tags) que podem ser deletadas ou renomeadas acidentalmente.
- **Performance:** busca extremamente rápida por tipo de componente.
- **Extensibilidade:** novos comportamentos podem ser adicionados futuramente sem quebrar nada.