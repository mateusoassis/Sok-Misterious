### CameraController (Core / Visão)

**Categoria:** Core · Câmera  
**Cena:** 02_Game  
**Tipo:** Componente em um GameObject com `Camera`

#### O que esse script faz

`CameraController` centraliza e ajusta a câmera ortográfica para enquadrar **todo o Bounds do nível** carregado.  
Ele é normalmente chamado pelo `LevelManager` logo após instanciar o prefab do nível (via `FocusOnBounds`).

A lógica garante:

- A câmera fica **centralizada** no nível.
- O **orthographicSize** é calculado para caber:
  - A **altura** do Bounds **ou**
  - A **largura** do Bounds convertida para “altura visível” considerando o **aspect ratio** da câmera.

Ou seja: o nível inteiro sempre aparece na tela, independentemente do tamanho do mapa.

---

#### Como funciona o cálculo

1. **Centraliza a câmera** no `center` do Bounds (e mantém o `z` atual).
2. Calcula:
   - `camHeight = b.size.y / 2`  
   - `camWidth = b.size.x / 2 / cam.aspect`  
     (a largura precisa ser convertida para a escala vertical por causa do aspect ratio)  
3. O `orthographicSize` escolhido é o maior dos dois valores, garantindo que **nenhuma borda do nível corte**.
4. Faz um `Debug.Log` informando o tamanho do Bounds ajustado.

---

#### Dependências

- Exige que o GameObject possua um **Camera** (obtida via `GetComponent<Camera>()` no `Awake`).
- Recebe `Bounds` calculado pelo `LevelManager` quando instancia o nível.

---

#### Quando é chamado

O fluxo típico é:

1. `LevelManager` instancia o nível (`LoadLevel(i)`).
2. O LevelManager detecta o objeto **Bounds** (BoxCollider2D).
3. Ele passa esse Bounds para:
   ```csharp
   cameraController.FocusOnBounds(levelBounds);