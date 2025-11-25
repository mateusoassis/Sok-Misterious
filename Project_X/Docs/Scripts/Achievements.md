# Achievements (Sistema de Conquistas / Telemetria)

Sistema genérico para registrar conquistas, estatísticas e eventos de gameplay.  
O próprio jogo **não implementa achievements**, apenas **emite eventos** para um *backend plugável*.

---

## 🎯 Objetivo do Sistema

Padronizar eventos importantes do gameplay:

- Finalização de nível  
- Uso de UNDO  
- Uso de Restart  
- Envio de estatísticas (`LevelStats`) quando o nível termina  

E permitir que cada plataforma implemente sua própria lógica:

- Steam Achievements  
- Xbox / Playstation Achievements  
- Google Play Games Services  
- Sistema próprio de telemetria  
- Logging de debug  

---

## 🧱 Arquitetura

O sistema é composto de duas partes:

### **1. IAchievementSink (Interface)**
Define o contrato:

- `OnLevelCompleted(int, LevelStats)`
- `OnUndoUsed(int)`
- `OnRestartUsed(int)`

### **2. Achievements (Facade Estática)**
Gameplay chama:

- `Achievements.NotifyLevelCompleted(stats)`
- `Achievements.NotifyUndoUsed()`
- `Achievements.NotifyRestartUsed()`

E a implementação é plugada por:

```csharp
Achievements.SetSink(new MyAchievementBackend());
```

---

## 🚀 Fluxo de Funcionamento

1. Antes do jogo começar, um backend é registrado:  

```csharp
Achievements.SetSink(new DebugAchievementSink());
```

2. Quando o jogador **finaliza um nível**, o `WinChecker` chama:

```csharp
Achievements.NotifyLevelCompleted(stats);
```

3. Quando o jogador **refaz um passo (UNDO)**:

```csharp
Achievements.NotifyUndoUsed();
```

4. Quando o jogador **reinicia o nível**:

```csharp
Achievements.NotifyRestartUsed();
```

5. O sistema **não armazena nada por conta própria** — apenas repassa ao sink.

---

## 📦 Exemplo Simples de Backend

```csharp
public class DebugAchievementSink : IAchievementSink
{
    public void OnLevelCompleted(int levelIndex, LevelStats stats)
    {
        Debug.Log($"[ACH] Level {levelIndex} completado. Movimentos: {stats.moves}");
    }

    public void OnUndoUsed(int levelIndex)
    {
        Debug.Log($"[ACH] UNDO usado no level {levelIndex}");
    }

    public void OnRestartUsed(int levelIndex)
    {
        Debug.Log($"[ACH] RESTART usado no level {levelIndex}");
    }
}
```

---

## 🧪 Integração com o Jogo

O `WinChecker` chama:

```csharp
Achievements.NotifyLevelCompleted(stats);
```

O `PlayerMover` chama:

```csharp
Achievements.NotifyUndoUsed();
```

O `WinChecker` chama em Restart:

```csharp
Achievements.NotifyRestartUsed();
```

---

## ✔ Recomendado Para

- Jogos multiplataforma usando diferentes APIs de achievements  
- Sistemas de telemetria/analytics  
- Desenvolvimento com cheats ou ferramentas internas  
- Debug de comportamento do jogador  

---

## 📁 Arquivos Relacionados

- `WinChecker.cs`  
- `LevelManager.cs`  
- `LevelStats.cs` (estrutura com estatísticas do nível)