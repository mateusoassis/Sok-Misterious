# LevelValidator (Ferramenta de Validação de Prefab de Level)

Valida automaticamente a integridade estrutural de um Level Sokoban:
- Verifica presença e configuração do **Bounds**.
- Confere **Goals e Boxes**.
- Checa se todos estão **dentro dos Bounds**.
- Valida **alinhamento ao grid lógico**.
- Oferece **Snap-To-Grid automático**.
- Útil para evitar erros silenciosos durante criação de fases.

---

## 📌 Funções Principais

### ✔ ValidateLevel()
Executa toda a validação:
- Bounds presente? Com **BoxCollider2D**?
- Existem Goals?
- Existem Boxes? Há mais Boxes que Goals?
- Algum Box/Goal está **fora dos Bounds**?
- Goal/Box está **alinhado ao grid**?
- Se ativado, aplica **snapToGridOnValidate**.

Executa via:
- Inspector → menu ··· → **Validate Level**
- Ou botão exposto (se você decidir expor).

---

### ✔ CheckGridAlignment()
Valida se Boxes e Goals estão no grid:
- Verifica `localPosition / cellSize`  
- Mostra warnings quando estiverem “entre tiles”.

---

### ✔ SnapChildrenToGrid()
Snapa TODOS os filhos do level **exceto**:
- o próprio root
- o objeto **Bounds**

---

## 🔧 Configurações do Inspector

### **cellSize**
Tamanho da célula do grid.  
Deve corresponder ao cellSize do Player e do LevelGridConfig.

### **snapToGridOnValidate**
Se ativo, o ValidateLevel tenta corrigir posições desalinhadas.

### **verbose**
Se true, mostra logs detalhados no Console.

---

## 🧱 Estrutura Esperada do Prefab

```
LevelRoot (tem LevelValidator)
 ├── Bounds (BoxCollider2D)
 └── BoardRoot
      ├── Paredes
      ├── Goals (com GoalIdentifier)
      ├── Boxes (com BoxIdentifier)
      └── Outros elementos
```

---

## 🚨 O que o Validator PEGA que costuma dar problema?

- Bounds sem collider  
- Collider não sendo trigger  
- Objetos posicionados em valores quebrados como 3.97  
- Goals fora do retângulo de jogo  
- Caixas lançadas fora da área válida  
- Levels acidentalmente criados com grid desalinhado

---

## 💡 Quando usar?

Antes de:
- Subir level no Git
- Colocar level na build
- Testar nova fase no jogo
- Gerar screenshots

Use SEMPRE.

---

## 🧾 Resumo da API

- `ValidateLevel()`  
- `ContextSnapChildrenToGrid()`  
- `SnapChildrenToGrid()`  
- `CheckGridAlignment(...)`