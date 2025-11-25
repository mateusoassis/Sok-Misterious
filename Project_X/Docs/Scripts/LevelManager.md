
# LevelManager (Core / Gestão de Níveis)

## Visão Geral
`LevelManager` é o sistema central responsável pelo ciclo de níveis no jogo: carregar, destruir, trocar, validar e notificar o restante do sistema sobre mudanças de nível. Ele funciona como um **singleton persistente** entre cenas, garantindo que o gerenciamento continue consistente durante toda a execução.

---

## Responsabilidades Principais
- Manter um **singleton** com `DontDestroyOnLoad`.
- Carregar níveis via índice (`LoadLevel`).
- Instanciar/destruir o prefab atual do nível.
- Detectar e armazenar os **Bounds** do nível.
- Ajustar a câmera via `CameraController.FocusOnBounds`.
- Enviar eventos de ciclo de jogo (`GameEvents.RaiseLevelLoaded`, `RaiseRestart`).
- Recarregar o nível atual (`Reload`).
- Avançar automaticamente para o próximo (`LoadNext`).
- Restaurar corretamente o nível escolhido ao carregar a cena `02_Game`, usando `LaunchArgs`.

---

## Estrutura Interna

### Singleton
O `LevelManager` se registra em `Awake`:
- Se for o primeiro, vira `Instance` e assina `SceneManager.sceneLoaded`.
- Se já houver outro, é destruído para evitar duplicatas.

---

### Campos principais
- **levelList** – Scriptable contendo todos os níveis (nome + prefab).
- **currentLevel** – Instância atual do nível carregado.
- **currentIndex** – Índice do nível atual.
- **CurrentBounds** – BoxCollider2D que representa o tamanho do nível.
- **LevelCount** – Quantidade de níveis disponíveis.

---

## Fluxo de Carregar Nível (`LoadLevel`)

1. Oculta VictoryUI e PauseMenu.
2. Valida:
   - Se existe LevelList.
   - Se LevelList tem itens.
   - Se o índice solicitado está no range.
   - Se o prefab do nível não é nulo.
3. Remove o nível anterior (caso já exista).
4. Instancia o prefab correspondente.
5. Procura o objeto “Bounds”:
   - Se existir com BoxCollider2D, registra em `CurrentBounds`.
   - Ajusta a câmera dinamicamente com base nesse Bounds.
6. Recalcula estado visual de caixas sobre goals (destaca caixas que estão em um Goal).
7. Dispara:
   ```
   GameEvents.RaiseLevelLoaded();
   ```

---

## Recarregar (`Reload`)
- Se houver nível carregado, chama:
  - `GameEvents.RaiseRestart();`
  - `LoadLevel(currentIndex);`

---

## Próximo Nível (`LoadNext`)
- Se nenhum nível tiver sido carregado ainda → carrega o índice 0.
- Se estiver no último nível → loga aviso.
- Caso contrário, carrega `currentIndex + 1`.

---

## Uso com LaunchArgs
Ao trocar de cenas, especialmente quando saindo do **Level Select**, o LevelManager verifica:

No `OnSceneLoaded`:
- Se a cena é `02_Game`
- E não há nível instanciado
→ então tenta carregar o nível indicado por `LaunchArgs.PendingLevel` ou nível 0.

Isso garante continuidade entre cenas sem objetos adicionais persistentes.

---

## Funções públicas

### `LoadLevel(int index)`
Carrega o nível indicado, caso válido.

### `Reload()`
Recarrega o nível atual.

### `LoadNext()`
Avança para o próximo nível, se disponível.

### `InsideBounds(Vector2 worldPoint)`
Retorna se o ponto está dentro dos limites do nível atual.

---

## Comentários sugeridos para colocar no script

```csharp
// Singleton persistente entre cenas
// Mantém estado do nível atual e controla o ciclo de níveis

// Carrega o nível solicitado: valida índice, instancia, ajusta câmera e emite eventos
// Remove instância antiga do nível antes de carregar o novo

// Após carregar, recalcula quais caixas estão sobre goals e aplica destaque visual

// Recarrega o nível atual (emitindo evento de restart)

// Avança para o próximo nível caso exista

// Verifica se um ponto está dentro do BoxCollider2D "Bounds"

// Quando cena 02_Game carrega e não há nível instanciado, escolhe nível via LaunchArgs
```

---

## Notas de Implementação

- O objeto do nível deve conter um filho chamado **“Bounds”** com `BoxCollider2D`.
- A câmera deve ter o componente `CameraController`.
- `LevelList` deve ser preenchido no Inspector.
- Esse sistema funciona 100% com níveis como **prefabs**.
- Chamadas a `GameEvents` permitem desacoplamento entre UI, HUD, câmera, Player e LevelManager.

---

## Exemplo de funcionamento esperado

```
MainMenu → LevelSelect → escolhe nível → define LaunchArgs → carrega 02_Game →
LevelManager lê LaunchArgs → instancia o nível correto → ajusta câmera → HUD atualiza
```

---