### GameBootstrapper (Core / Boot)

**Cena alvo:** `02_Game`  
**Categoria:** Core / Bootstrapping

#### O que esse script faz

`GameBootstrapper` é o responsável por **inicializar a cena de jogo** (`02_Game`), garantindo que:

- A chave de progresso `highestUnlocked` exista no `PlayerPrefs` (começa em `0` se ainda não existir). :contentReference[oaicite:0]{index=0}  
- O **primeiro level a ser carregado** seja decidido com base em `LaunchArgs.PendingLevel` (se vier do Level Select) ou **0** por padrão.
- O `LevelManager.Instance` esteja presente e, se estiver tudo certo, chame `LoadLevel(index)` para instanciar o nível.
- O valor de `LaunchArgs.PendingLevel` seja **consumido** (resetado para `null`) depois de usado.

Ele roda como uma **coroutine no `Start()`**, esperando um frame antes de fazer tudo isso, para garantir que todos os `Awake`/`OnEnable` dos outros sistemas já foram chamados.

---

#### Fluxo passo a passo

1. **Espera 1 frame**
   - `yield return null;`  
   - Dá tempo de todos os `Awake`/`OnEnable` da cena rodarem antes de tentar usar `LevelManager` ou `LaunchArgs`.

2. **Garante o PlayerPrefs de progresso**
   - Usa a constante `Key = "highestUnlocked"`.
   - Se a chave **não existe**, cria com valor `0` e já chama `PlayerPrefs.Save()`.
   - Isso evita `HasKey`/`GetInt` quebrando em outros lugares que assumem a existência dessa chave.

3. **Determina o índice do level inicial**
   - Começa em `index = 0`.
   - Se `LaunchArgs.PendingLevel.HasValue`:
     - Usa esse valor, mas protege com `Mathf.Max(0, value)` para evitar índice negativo.
   - Faz um `Debug.Log` com:
     - `PendingLevel` original.
     - `chosenIndex` final usado.

4. **Valida o `LevelManager`**
   - Se `LevelManager.Instance == null`:
     - Loga um `Debug.LogError` avisando que o singleton não está presente na `02_Game`.
     - Dá `yield break;` e para aqui (não tenta carregar level nenhum).

5. **Carrega o nível**
   - Guarda `var lm = LevelManager.Instance;`
   - Loga:
     - `LevelCount` (quantidade de níveis registrados).
     - Se `lm.levelList` está `null` ou não (ajuda debug de setup).
   - Chama `lm.LoadLevel(index);` para instanciar o prefab do nível correspondente.

6. **Consome o `LaunchArgs`**
   - Após carregar o nível, faz:
     - `LaunchArgs.PendingLevel = null;`
   - Isso garante que, se a cena `02_Game` for recarregada sem passar pelo Level Select, ela **volta a carregar o level 0** por padrão.

---

#### Dependências

- **`LevelManager`**
  - Precisa existir como singleton (`LevelManager.Instance`) na cena (normalmente via `Managers.prefab`). :contentReference[oaicite:1]{index=1}  
  - Expõe:
    - `int LevelCount`
    - `LevelList levelList`
    - `void LoadLevel(int index)`

- **`LaunchArgs`**
  - Classe/struct estática que guarda `PendingLevel` (`int?`).
  - Usada para comunicar o level escolhido na `01_LevelSelect` para a `02_Game`.

- **`PlayerPrefs`**
  - Usa a mesma chave de progresso descrita em **Dados & Salvamento**: `highestUnlocked`. :contentReference[oaicite:2]{index=2}  

---

#### Pontos importantes / gotchas

- Se o `LevelManager` não estiver na cena, o jogo **não quebra**, mas fica preso sem carregar nenhum nível (erro só em log).
- Esse script **não desbloqueia** levels — ele só garante que a chave de progresso existe e manda carregar o nível certo. Quem controla o desbloqueio continua sendo o `SaveManager`/`WinChecker`. :contentReference[oaicite:3]{index=3}  
- A espera de 1 frame (`yield return null`) é importante para evitar race conditions com outros sistemas que fazem setup no `Awake`/`OnEnable`.

---

#### API rápida

- `Start()` (coroutine, `IEnumerator`)
  - Não deve ser chamada manualmente.
  - Executa automaticamente na inicialização, faz o fluxo descrito acima.