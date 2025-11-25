# PlayerLerpMotion

Controla o movimento suave (lerp) do player entre tiles no grid.

## Responsabilidades
- Movimentar o player suavemente de um tile ao próximo usando Lerp.
- Garantir que apenas um movimento ocorra por vez.
- Permitir cancelamento instantâneo e reposicionamento (snap).
- Notificar conclusão do movimento via callback (`onComplete`).

## Fluxo de Uso
1. `PlayerMover` chama `MoveBy(dir)` ao processar input.
2. O movimento interpola a posição atual até a posição final (start + dir).
3. Quando termina, chama `onComplete` se fornecido.
4. `CancelAndSnap()` é usado quando há necessidade de reposicionar o player imediatamente.

## API
- **IsMoving (bool)** — Indica se o player está em movimento.
- **MoveBy(Vector2Int dir, Action onComplete)** — Inicia movimento interpolado.
- **CancelAndSnap(Vector3 target)** — Cancela movimento e teleporta para um ponto específico.

## Notas
- `moveDuration` deve combinar com o tempo da animação de movimento para sincronização perfeita.
- Movimento ocorre em grid puro (incrementos inteiros).
- Lerp usa `Mathf.Clamp01` para garantir suavidade sem overshoot.