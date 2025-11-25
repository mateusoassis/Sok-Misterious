# DevTools – Documentação

Ferramentas internas de desenvolvimento para ajudar no teste e navegação rápida entre níveis durante o desenvolvimento do jogo.

## 📌 Resumo

O script `DevTools.cs` adiciona funcionalidades ativadas somente em:
- **Unity Editor**, ou
- **Development Build**

Ele permite:
- Mostrar um overlay com informações de debug.
- Forçar recarregamento de nível.
- Ir para próximo/nível anterior.
- Forçar vitória.
- Visualizar estatísticas da run atual (moves, undos, pushes, tempo, etc).

---

## 🎮 Atalhos disponíveis

| Tecla | Ação |
|------|------|
| **F1** | Alterna overlay de debug |
| **F5** | Reload do nível atual |
| **F6** | Carregar próximo nível |
| **F7** | Carregar nível anterior |
| **F8** | Forçar vitória (cheat) |

---

## 🧩 Overlay de Debug

O overlay (opcional) exibe:
- Índice e nome do nível atual  
- HighestUnlocked (progresso salvo)
- Moves / Pushes / Undos / Restarts
- Tempo de gameplay no nível  
- Guia dos atalhos disponíveis  

Ele utiliza:
- **CanvasGroup** (ref para esconder/mostrar)
- **TextMeshProUGUI** para o texto

---

## ⚙ Configurações

### **devModeEnabled**
Se desativado, o script inteiro fica OFF — mesmo em Editor ou DevBuild.

### **overlayCanvas / overlayLabel**
Campos opcionais.  
Se não forem atribuídos, o overlay não será mostrado (mas as hotkeys continuam funcionando).

---

## 🚧 Segurança
O script se autodesativa em builds finais:
```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// ativo
#else
enabled = false;
return;
#endif
```

Isso garante que cheats **não vazem** para builds públicas.

---

## 📂 Local recomendado

Colocar o objeto `DevTools` dentro de:
```
/Managers
```
ou qualquer objeto persistente da cena principal.

---

## 🔧 Dependências
- **Input System**
- **TextMeshPro**
- `LevelManager`
- `LevelRunTracker`
- `WinChecker`

---

## 🧪 Uso típico

1. Abrir o jogo no Editor  
2. Pressionar **F5/F6/F7** para navegar entre níveis  
3. Usar **F1** para checar:
   - tempo de execução do nível  
   - estatísticas  
   - progresso atual  
4. Testar vitória rápida com **F8**

---

## 🏁 Extensões sugeridas
- Botão para teleportar player via overlay  
- Mostrar quantidade de Goals/Boxes ativos  
- Mostrar estado interno de BOUNDS  
- Integrar toggles para activar/desativar colisão/verificações