# SaveManager — Documentação

O `SaveManager` é um utilitário estático responsável por gerenciar o progresso do jogador — especificamente qual nível mais alto já foi desbloqueado.  
Ele usa **PlayerPrefs** como backend simples de armazenamento, garantindo compatibilidade com PC, consoles e builds mobile.

---

## ✨ Funcionalidades Principais

### 🔹 Carregar progresso
- Lê duas chaves:
  - `progress.highestUnlockedIndex` (nova e oficial)
  - `highestUnlocked` (legada, usada em builds antigas)
- Migra automaticamente a chave antiga para a nova, caso necessário.

### 🔹 Salvar progresso
- Salva `_highest` de volta na chave oficial.
- Garante que valores negativos não sejam gravados.

### 🔹 Desbloquear níveis
- `UnlockUpTo(index)` desbloqueia até um índice específico.
- Idempotente: não regrava se já há progresso maior ou igual.
- Chamado normalmente pelo `WinChecker` ao completar um nível.

### 🔹 Resetar progresso
- Apaga somente a chave oficial e a legada.
- Força recarregamento na próxima leitura.

---

## 🧠 Fluxo de Funcionamento

1. **Primeira leitura de `HighestUnlockedIndex` → chama `Load()` automaticamente.**
2. `Load()` decide qual valor usar (novo ou legado).
3. O jogo consulta esse valor para saber:
   - Quais botões/níveis ficam ativos.
   - Qual o último nível permitido no menu de seleção.
4. Quando o jogador avança:
   - `UnlockUpTo(nextLevelIndex)` é chamado.
   - `Save()` grava permanentemente.
5. Em caso de wipe de progresso:
   - `ResetProgress()` limpa tudo.

---

## ⚠️ Observações importantes

- PlayerPrefs **não é seguro** contra manipulação manual (Save scumming).
- Para ports futuros (Xbox/PS4/Switch), pode-se trocar o backend por:
  - Sistema de save encriptado.
  - API de achievements/progress do console.
- `_loaded = true` garante que o sistema não execute `Load()` mais de uma vez por sessão.

---

## 📌 Exemplo de Uso

```csharp
// Ao completar um nível:
int next = currentLevelIndex + 1;
SaveManager.UnlockUpTo(next);

// Ao abrir o menu:
int maxLevel = SaveManager.HighestUnlockedIndex;
buttonLevel5.interactable = (maxLevel >= 5);
```

---

## 📂 Chaves utilizadas

| Chave PlayerPrefs | Uso | Observações |
|------------------|------|-------------|
| `progress.highestUnlockedIndex` | Chave atual | Sempre preferida |
| `highestUnlocked` | Chave antiga | Migrada automaticamente |

---

## ✔️ Conclusão

O `SaveManager` é simples, eficiente e robusto para uso em um Sokoban — e já preparado para futuras expansões e substituição de backend caso necessário.