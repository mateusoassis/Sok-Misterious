# DevProgressButtons

Ferramentas de desenvolvimento para **resetar progresso** e **limpar PlayerPrefs** durante testes no editor ou builds de desenvolvimento.

---

## 📌 Visão Geral

O `DevProgressButtons` é um script usado em botões do menu Developer/Debug para:

- 🔄 **Resetar o progresso salvo** (HighestUnlockedIndex)
- 🧹 **Limpar TODOS os PlayerPrefs**
- ↩️ **Recarregar o Level Select** OU apenas recarregar o nível atual
- ⏸️ Fechar automaticamente o Pause Menu caso esteja aberto
- 📣 Logar ações no Console para debug

Ideal para testes de QA, debug de fluxo de progressão, ou para checar se o jogo se comporta corretamente após wipes de dados.

---

## 🧪 Funções Principais

### **▶️ OnClickResetHighest()**
Reseta apenas o progresso de estágios desbloqueados (`SaveManager.ResetProgress()`).

### **▶️ OnClickResetPlayerPrefs()**
Deleta absolutamente TODOS os PlayerPrefs (`PlayerPrefs.DeleteAll()`).

### **🔁 JumpAfterReset()**
Decide o que fazer após resetar:
- Ir para a cena de Level Select **(padrão)**  
- Ou ficar na cena atual e recarregar o nível

### **🧽 ClosePauseIfOpen()**
Fecha o Pause Menu caso esteja aberto.

---

## ⚙️ Configurações no Inspector

| Campo | Função |
|-------|--------|
| **goToLevelSelectAfter** | Se o botão deve enviar o jogador para o Level Select |
| **levelSelectScene** | Nome da cena de Level Select nas Build Settings |
| **gameScene** | Cena onde os níveis são executados |
| **verboseLogs** | Mostra logs detalhados ao clicar nos botões |

---

## 🧩 Dependências

- **SaveManager** — para resetar progresso
- **PauseMenu** — opcional, usado apenas para fechar menu pausado
- **SceneManager** — para trocar cenas
- **LevelManager** — usado quando o reset mantém o jogador dentro da cena principal

---

## 👨‍💻 Uso Típico

1. Crie um Canvas com UI Buttons.
2. Adicione o componente `DevProgressButtons` em algum GameObject do Canvas.
3. Linke os botões via **OnClick()**:
   - Reset Highest → `OnClickResetHighest()`
   - Reset PlayerPrefs → `OnClickResetPlayerPrefs()`
4. Certifique-se de que as cenas configuradas estão nas **Build Settings**.

---

## ✔️ Boas práticas

- Evite usar em builds públicas.
- Coloque atrás de um painel de debug/QA.
- Use logs como referência para verificar comportamento.
- Combine com ferramentas como `LevelValidator` e `SaveManager`.

---

## 📎 Observação

Este script **não deve ir para produção** sem proteções adicionais.  
Utilize somente em builds internas, edições de QA ou ferramentas de debug.