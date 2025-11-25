# LevelRunTracker (Rastreamento da Run do Nível)

Responsável por acompanhar estatísticas da run atual do nível:
- Quantidade de *moves*
- Quantidade de *pushes*
- Quantidade de *undos*
- Quantidade de *restarts*
- Tempo total desde o carregamento do nível

Não salva nada em disco — funciona apenas em memória, servindo como fonte de dados para o sistema de Achievements.

## O que ele faz

### ✔ Escuta os eventos do GameEvents
- `OnLevelLoaded` → reinicia estatísticas
- `OnMove` → incrementa `moves`
- `OnPush` → incrementa `pushes`
- `OnUndo` → incrementa `undos` + notifica Achievements
- `OnRestart` → incrementa `restarts` + notifica Achievements

### ✔ Mantém um snapshot de LevelStats
```csharp
public struct LevelStats {
    public int moves;
    public int pushes;
    public int undos;
    public int restarts;
    public float timeSec;
}
```

### ✔ Fornece dados atualizados
`GetSnapshot()` retorna uma cópia de LevelStats com `timeSec` atualizado antes da consulta.