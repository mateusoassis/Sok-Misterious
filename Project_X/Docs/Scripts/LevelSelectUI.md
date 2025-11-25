# LevelSelectUI — Documentação

Controla a tela de **seleção de fases** do jogo.  
Responsável por:

- Ler a lista de níveis (`LevelList`)
- Ler o progresso salvo via `SaveManager.HighestUnlockedIndex`
- Gerar dinamicamente botões para cada nível
- Bloquear/desbloquear botões com base no progresso
- Definir qual nível será carregado ao clicar (via `LaunchArgs.PendingLevel`)
- Carregar a cena de jogo (`02_Game`) ao selecionar um nível

---

## 🧩 Fluxo Geral

1. No `Start()`:
   - Valida se `levelList` está configurado e possui níveis.
   - Lê o total de níveis (`levelCount`).
   - Lê o progresso salvo: `SaveManager.HighestUnlockedIndex`.
   - Limpa o conteúdo anterior do `gridParent` (se houver).

2. Para cada nível em `levelList.levels`:
   - Instancia um botão a partir de `buttonPrefab` como filho de `gridParent`.
   - Define o texto do botão (`displayName` ou nome do prefab).
   - Verifica se o nível está **desbloqueado** (`i <= highestUnlocked`).
   - Ajusta:
     - `btn.interactable`
     - Cores do botão (locked/unlocked)
   - Registra o callback de clique:
     - Seta `LaunchArgs.PendingLevel = index`
     - Carrega a cena `"02_Game"`

3. Ao final:
   - Se houver um `EventSystem` ativo, define o foco inicial em um botão desbloqueado (útil para gamepad).

---

## 🎮 Integração com Progresso

Usa o `SaveManager`:

```csharp
int highestUnlocked = Mathf.Clamp(SaveManager.HighestUnlockedIndex, 0, levelCount - 1);
```

Isso garante que:

- O índice **nunca sai do range** `[0 .. levelCount-1]`.
- Todos os níveis no intervalo `0 .. highestUnlocked` são considerados desbloqueados.

Níveis com índice maior que `highestUnlocked` ficam bloqueados (botão não interagível + cor de travado).

---

## 🚪 Como o nível é carregado

Ao clicar em um botão de nível:

```csharp
LaunchArgs.PendingLevel = capturedIndex;
SceneManager.LoadScene("02_Game", LoadSceneMode.Single);
```

- `LaunchArgs.PendingLevel` guarda o índice do nível que deve ser carregado.
- A cena `"02_Game"` é carregada.
- Dentro dessa cena, o `LevelManager` lê esse valor e instancia o nível correto.

---

## 🎨 Visual de Locked / Unlocked

O script permite configurar as cores de bloqueado/desbloqueado no Inspector:

- `lockedColor` → cor usada para botões travados (e também `disabledColor`)
- `unlockedColor` → cor normal dos botões liberados

Ele modifica o `ColorBlock` do botão:

```csharp
var colors = btn.colors;
colors.normalColor   = unlocked ? unlockedColor : lockedColor;
colors.disabledColor = lockedColor;
btn.colors = colors;
```

---

## 🧱 Campos no Inspector

### Data
- `LevelList levelList`  
  Asset ScriptableObject que contém os níveis (`LevelEntry[] levels`).

### UI
- `Transform gridParent`  
  Transform que será o **pai** de todos os botões gerados (ex.: um GridLayoutGroup).
- `Button buttonPrefab`  
  Prefab do botão de nível (deve conter um `TextMeshProUGUI` filho para o rótulo).

### Lock Visual
- `Color lockedColor`  
- `Color unlockedColor`  

---

## 🎯 Requisitos

- `LevelList` devidamente configurado com todos os níveis.
- `SaveManager` funcional (especialmente `HighestUnlockedIndex`).
- `LaunchArgs` + `LevelManager` corretamente integrados na cena `"02_Game"`.
- Um `EventSystem` na cena de Level Select (para navegação via gamepad/teclado).

---

## ✅ Uso típico

1. Criar uma cena de Level Select.
2. Adicionar um `GameObject` com o script `LevelSelectUI`.
3. Preencher no Inspector:
   - `levelList`
   - `gridParent` (um objeto com `GridLayoutGroup` ou `VerticalLayoutGroup`)
   - `buttonPrefab`
4. Ajustar cores de lock/unlock se necessário.
5. Garantir que `"02_Game"` está no Build Settings.

Pronto: a tela de seleção de fases passa a ser gerada dinamicamente com base no progresso salvo.