# LevelStats (Estrutura de Estatísticas do Nível)

`LevelStats` é uma struct serializável responsável por armazenar os principais dados de uma run do nível atual.

Esses dados são coletados pelo `LevelRunTracker` e consumidos por sistemas como:
- **Achievements**
- **DebugAchievementSink**
- **WinChecker**
- **Telemetria / Analytics (futuro)**

---

## 📊 Campos rastreados

| Campo      | Tipo   | Descrição |
|------------|--------|-----------|
| `moves`    | int    | Quantidade total de movimentos do jogador. Incrementado por `GameEvents.OnMove`. |
| `pushes`   | int    | Quantidade de pushes (quando o jogador empurra caixas). |
| `undos`    | int    | Quantidade de ações de desfazer (`UNDO`). |
| `restarts` | int    | Quantidade de vezes que o jogador reiniciou o nível. |
| `timeSec`  | float  | Tempo total da run desde o carregamento do nível. |

---

## 🔍 Uso típico

`LevelStats` funciona como uma **fotografia** do estado atual do progresso do jogador no nível.  
Ele sempre é atualizado pelo `LevelRunTracker`, e pode ser consultado via:

```csharp
LevelStats stats = LevelRunTracker.Instance.GetSnapshot();
```

---

## 🧾 Exemplo de saída do `ToString()`

```text
moves=34, pushes=12, undos=3, restarts=1, time=42.5s
```

---

## 🔌 Onde é usado

- **Achievements.NotifyLevelCompleted(stats)**
- Logs de debug
- HUD de performance
- Métricas internas do jogo

---

## ✔ Responsabilidades
- Armazenar estatísticas puras (sem lógica).
- Ser serializável (`[Serializable]`).
- Facilitar debug com `ToString()`.

---

