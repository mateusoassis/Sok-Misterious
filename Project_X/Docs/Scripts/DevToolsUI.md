# DevToolsUI — README

Pequeno utilitário para testes e depuração no Unity.  
Este script fornece botões simples que você pode conectar na UI para resetar PlayerPrefs durante o desenvolvimento.

## 🎯 Objetivo

Facilitar o processo de:

- Resetar **toda** a PlayerPrefs rapidamente.
- Resetar **apenas** o progresso salvo (`highestUnlocked`).

Isso é útil durante criação de níveis, balanceamento, testes de fluxo, validações de HUD, etc.

---

## 📄 Funções disponíveis

### `ResetAllPlayerPrefs()`
Apaga absolutamente **todas** as chaves armazenadas no PlayerPrefs.

Use para:
- Testar o jogo como se estivesse sendo aberto pela primeira vez.
- Validar fluxos de tutorial/configuração inicial.

⚠️ **Atenção:** isto apaga *tudo* — inclusive configurações, áudio, progresso, opções etc.

---

### `ResetHighestUnlocked()`
Apaga apenas a chave usada pelo progresso de níveis.

Ideal para:
- Testar desbloqueio de níveis.
- Verificar comportamento do `SaveManager` em valores iniciais.

Chave resetada:
```
highestUnlocked
```

---

## 🧩 Como usar

1. Arraste este script (`DevToolsUI.cs`) para um GameObject na cena.
2. Crie botões UI (Unity UI).
3. No OnClick() de cada botão:
   - Arraste o GameObject com o script.
   - Escolha a função desejada (ResetAllPlayerPrefs ou ResetHighestUnlocked).

---

## ⚙️ Ambiente recomendado

- **Editor** ou **Development Build**
- Apenas para depuração.  
  Não inclua acesso fácil no jogo final.

---

## 📝 Notas

- Esse script não lida com cenas ou recarregamento — apenas PlayerPrefs.
- Para resets mais completos (incluindo SaveManager), use o `DevProgressButtons`.