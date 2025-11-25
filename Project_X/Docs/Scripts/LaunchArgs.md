### LaunchArgs (Core / Navegação Entre Cenas)

**Categoria:** Core · Comunicação entre cenas  
**Local:** Script estático, não depende de GameObject

#### O que esse script faz

`LaunchArgs` funciona como um **envelope de parâmetros temporários** usado para transferir dados entre cenas — especificamente, da tela **Level Select (01_LevelSelect)** para a cena **02_Game**.

Ele guarda apenas um campo:

- **`PendingLevel: int?`** — nível que deve ser carregado quando a cena `02_Game` iniciar.

Esse valor é:
1. **Setado** pelo Level Select quando o jogador escolhe um nível.  
2. **Consumido** pelo `GameBootstrapper` assim que a cena `02_Game` começa, carregando o level e zerando (`null`) o campo.

---

#### Como funciona no fluxo do jogo

1. O jogador abre o **Level Select**.
2. Clique em um nível chama algo como:
   ```csharp
   LaunchArgs.PendingLevel = indexEscolhido;
   SceneManager.LoadScene("02_Game");
