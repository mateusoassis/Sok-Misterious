# BoxGoalState

## Visão Geral
`BoxGoalState` é um componente simples responsável por **trocar o sprite da caixa** quando ela está:
- **Em cima de um goal**  
- **Fora de um goal**

O LevelManager chama `SetOnGoal()` depois de mapear as posições da grade e decidir quais caixas estão sobre quais Goals.

Esse script é o responsável direto pela troca visual — por exemplo:
- Patinho normal  
- Patinho no travesseiro (quando encaixado sobre o Goal)

---

## Responsabilidades
- Armazena o estado booleano `IsOnGoal`.
- Controla qual `Sprite` deve aparecer (idle vs encaixado).
- Se necessário, tenta encontrar automaticamente o `SpriteRenderer` da caixa.
- Permite atualizar o visual manualmente via `Refresh()`.

---

## Campos
### `IsOnGoal`
Indica se a caixa está posicionada em um Goal.

### `boxRenderer`
`SpriteRenderer` usado para exibir o sprite atual da caixa.  
Se não for atribuído no Inspector, o script tenta encontrá‑lo automaticamente:
- Busca um filho chamado `"BoxSprite"`
- Caso não exista, usa o primeiro `SpriteRenderer` encontrado nos filhos

### `normalSprite`
Sprite padrão da caixa (ex.: patinho normal).  
Se não for definido no Inspector, ele usa o sprite inicial do `boxRenderer`.

### `onGoalSprite`
Sprite exibido quando `IsOnGoal == true`.

---

## Métodos
### `Awake()`
Faz o setup inicial:
- Detecta automaticamente o SpriteRenderer, se necessário.
- Preenche `normalSprite` automaticamente caso esteja vazio.

### `SetOnGoal(bool v)`
Troca o valor de `IsOnGoal` e atualiza automaticamente o sprite exibido.

### `Refresh()`
Reaplica o estado visual atual.  
Útil caso o sprite seja modificado em runtime ou por outros sistemas.

---

## Integração com o sistema de níveis
O `LevelManager.InitializeBoxHighlights()` chama `SetOnGoal()` para cada caixa detectada, após:
1. Converter posições para grid
2. Montar um conjunto de células‑goal
3. Checar se cada caixa está dentro do conjunto

Assim, a lógica visual da caixa fica totalmente isolada neste script.

---

## Exemplo de uso in‑editor
1. Adicione `BoxGoalState` no GameObject da caixa.
2. Arraste:
   - `normalSprite`
   - `onGoalSprite`
3. Opcionalmente arraste o `boxRenderer`
4. Se não fizer nada disso, o script tentará adivinhar tudo automaticamente.