# LevelGridConfig (Grid & Bounds Controller)

Responsável por definir a grade lógica de cada nível de Sokoban, garantir que **tudo esteja alinhado ao grid**, e manter o **BoxCollider2D do Bounds** sempre coerente com Width/Height/CellSize.  
Ele também fornece utilidades essenciais para normalizar níveis editados no Unity.

---

## 📌 Objetivo Geral
O `LevelGridConfig` garante que:

- O tabuleiro tenha origem fixa em **(0,0)** no canto inferior esquerdo.
- O collider `Bounds` sempre cubra corretamente o retângulo do nível.
- Todos os objetos dentro de `BoardRoot` continuem alinhados ao grid.
- A edição manual no Unity nunca quebre o sistema de câmera, o LevelManager ou o gameplay.

---

## 🧩 Componentes esperados no prefab do Level
Para funcionar 100%, o nível deve ter:

```
LevelRoot
 ├── BoardRoot       (contém paredes, caixas, goals, piso, decor)
 └── Bounds          (BoxCollider2D, isTrigger = ON)
```

E no `LevelRoot`, o script:

```
LevelGridConfig
    Width
    Height
    CellSize
    BoardRoot (assign)
    BoundsCollider (assign)
```

---

## 📐 Como calcula o Bounds
O collider cobre:
```
X: 0 .. Width-1
Y: 0 .. Height-1
```

E o centro local fica em:
```
((Width-1)/2, (Height-1)/2) * CellSize
```

Assim o nível inteiro cresce **para cima e para a direita**.  
Isso garante que a câmera do `CameraController` funcione perfeitamente.

---

## 🔧 Métodos principais

### `ApplyBounds()`
Ajusta automaticamente:
- `BoundsCollider.size`
- `BoundsCollider.offset`
- Snapa a posição e escala do objeto `Bounds`

Chamado automaticamente por:
- `OnValidate()` (toda vez que mudar algo no inspector)
- `Reset()`
- Você também pode chamar manualmente.

---

### `NormalizeBottomLeftOrigin()`
Move todo o conteúdo do `BoardRoot` para que o menor X e Y se tornem **(0,0)**.

Útil quando:
- O designer criou o nível “torto”
- Objetos negativos em X/Y quebram câmera e lógica do LevelManager
- Você importou tiles ou copiou coisas que não estavam centralizadas

Fluxo recomendado:

1. Selecione o LevelRoot.
2. Menu do custom inspector → “Normalize Bottom-Left Origin”
3. Depois execute: “Snap Children To Grid”

---

### `SnapChildrenToGrid()`
Snapa todos os objetos dentro de `BoardRoot` para coordenadas inteiras.  
Resolve problemas de:
- Caixas levemente deslocadas (ex.: 2.0031)
- Goals que não alinham com OverlapPoint
- Cameras que ficam vibrando devido a floats

---

## 🧪 Uso típico no pipeline de criação de níveis

1. Posicione *todo o conteúdo* dentro do `BoardRoot`.
2. Rode **Normalize Bottom-Left Origin** para garantir que o nível comece em (0,0).
3. Ajuste **Width / Height** no LevelGridConfig.
4. Certifique-se de que o collider do `Bounds` está correto usando **ApplyBounds**.
5. Snape o conteúdo com **SnapChildrenToGrid**.
6. Teste o nível no 02_Game para ver foco automático da câmera.

---

## ⚠️ Erros comuns que esse script resolve
| Problema | Causa | Solução |
|---------|--------|---------|
| Câmera cortando parte do level | Bounds errado | ApplyBounds |
| Caixas não entram no goal | Posições não inteiras | SnapChildrenToGrid |
| Caixas/Goals aparecem fora da área válida | Conteúdo com X/Y negativo | NormalizeBottomLeftOrigin |
| Player trava nas bordas | Bounds desalinhado | ApplyBounds |
| LevelManager não detecta bounds | Objeto “Bounds” faltando ou errado | Ajuste as referências |

---

## 📝 Resumo rápido
| Função | Para que serve |
|--------|----------------|
| `ApplyBounds()` | Ajusta tamanho e posição do collider do nível |
| `NormalizeBottomLeftOrigin()` | Reposiciona tudo para começar em (0,0) |
| `SnapChildrenToGrid()` | Snapa todos os objetos para inteiros |

---

## ✔️ Entidade perfeita para pipeline de criação de puzzles
O `LevelGridConfig` garante que cada nível esteja 100% consistente com a lógica do jogo, facilita port futuro para outras plataformas e remove erros silenciosos que só aparecem mais tarde.
