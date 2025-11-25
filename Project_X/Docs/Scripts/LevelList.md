# LevelList (ScriptableObject)

## Visão Geral
`LevelList` é um ScriptableObject usado como **catálogo de níveis** do jogo.  
Ele define um array serializável de `LevelEntry`, onde cada entrada representa um nível jogável do Sokoban.

Esse asset é referenciado pelo `LevelManager` para descobrir:
- Quantos níveis existem (`LevelCount`).
- Qual prefab instanciar para cada índice.
- Qual nome exibir/logar para cada nível.

---

## Estrutura

### Classe `LevelList`

- **Tipo:** `ScriptableObject`
- **Criação no menu:** `Sokoban/Level List`  
- **Campo principal:**
  - `LevelEntry[] levels` – array de entradas de nível.

Você cria esse asset pelo menu:
> `Create → Sokoban → Level List`

Depois, no Inspector do asset, você preenche o array `levels` com cada nível do jogo.

---

### Classe `LevelEntry`

Marcada como `[System.Serializable]` para aparecer no Inspector dentro do `LevelList`.

Campos:

- `string displayName`  
  Nome amigável do nível (usado para debug, UI de seleção, logs etc.).

- `GameObject levelPrefab`  
  Prefab do nível – normalmente um prefab contendo:
  - Layout do grid (tiles/parede/chão)
  - Player/caixas/goals
  - Objeto `Bounds` com `BoxCollider2D`

O `LevelManager` acessa `LevelList.levels[index]` para descobrir qual prefab instanciar e qual nome mostrar no log.

---

## Integração com LevelManager

O `LevelManager` usa este Scriptable da seguinte forma:

- O campo público `LevelList levelList` é preenchido no Inspector com o asset criado.
- A property `LevelCount` é baseada em `levelList.levels.Length`.
- Em `LoadLevel(int index)`, o manager faz:

1. Pega a entrada:
   ```csharp
   var entry = levelList.levels[index];
   ```
2. Valida se `entry.levelPrefab` não é nulo.
3. Instancia o prefab:
   ```csharp
   Instantiate(entry.levelPrefab, Vector3.zero, Quaternion.identity);
   ```
4. Usa `entry.displayName` para logs/debug.

Com isso, a ordem dos níveis, seus nomes de exibição e seus prefabs ficam totalmente **data-driven** no asset, sem precisar mexer em código.

---

## Boas práticas de uso

- Mantenha o `LevelList` em uma pasta clara, por exemplo:  
  `Assets/Data/LevelList.asset`
- Nomeie os níveis de forma consistente:  
  - `Level 01 - Introdução`
  - `Level 02 - Empurrões Diagonais (não permitidos)`
- Garanta que todos os `levelPrefab` têm:
  - Objeto `Bounds` com `BoxCollider2D`
  - Componentes necessários (`LevelValidator`, etc., se aplicável)

---

## Exemplo de conteúdo do LevelList

No Inspector, o array `levels` poderia estar assim:

1. **Element 0**
   - `displayName`: `"Level 01 - Básico"`
   - `levelPrefab`: `Level_01`

2. **Element 1**
   - `displayName`: `"Level 02 - Corredor"`
   - `levelPrefab`: `Level_02`

3. **Element 2**
   - `displayName`: `"Level 03 - Backtracking"`
   - `levelPrefab`: `Level_03`

…e assim por diante.

---

## Resumo

- `LevelList` = coleção de níveis, configurável via Inspector.
- `LevelEntry` = nome + prefab para cada nível.
- `LevelManager` lê o `LevelList` para controlar toda a progressão de níveis sem hardcode.