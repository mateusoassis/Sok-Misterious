# DebugAchievementSink (Implementação de Debug para Achievements)

Este componente fornece uma implementação simples do `IAchievementSink`, usada apenas para **debug**.  
Ele não salva nada, não envia nada para backend — apenas registra eventos no Console via `Debug.Log`.

## 📌 Objetivo
- Exibir no Console todos os eventos de achievements disparados pelo jogo.
- Facilitar o teste de fluxo: vitória, uso de undo, uso de restart.
- Permitir verificar se o sistema de achievements está funcionando corretamente antes de conectar algo real (ex.: Xbox, Steam ou backend próprio).

## 🧩 Como funciona
Quando o GameObject com este script é ativado:

- `OnEnable()` registra este objeto como **sink atual** dos Achievements.
- Eventos chamam:
  - `OnLevelCompleted(levelIndex, stats)`
  - `OnUndoUsed(levelIndex)`
  - `OnRestartUsed(levelIndex)`

Quando o GameObject é desativado:

- Se ainda for o sink atual, ele remove a si mesmo (`SetSink(null)`).

## 📘 API Implementada
### `OnLevelCompleted(int levelIndex, LevelStats stats)`
Chamado quando o jogador completa um nível.  
Mostra no Console:
```
[Achv] Level X COMPLETED → LevelStats(...)
```

### `OnUndoUsed(int levelIndex)`
Chamado quando o jogador usa UNDO.  
Log:
```
[Achv] UNDO usado no level X
```

### `OnRestartUsed(int levelIndex)`
Chamado quando usa RESTART.  
Log:
```
[Achv] RESTART usado no level X
```

## 🛠️ Como usar no Unity
1. Crie um GameObject vazio chamado **DebugAchievements** ou use um Manager existente.
2. Adicione o componente **DebugAchievementSink**.
3. Dê Play.  
   - Toda vitória/undo/restart aparecerá no Console.
4. Remova do build final se não quiser logs.

## 🔎 Observações
- Ideal para testes de WinChecker, LevelRunTracker e fluxo geral.
- Apenas 1 sink pode estar ativo por vez — este componente substitui qualquer outro sink registrado.
- Se quiser implementar achievements reais (Steam/Xbox/custom backend), crie outra classe que implemente `IAchievementSink` e registre-a via `Achievements.SetSink()`.