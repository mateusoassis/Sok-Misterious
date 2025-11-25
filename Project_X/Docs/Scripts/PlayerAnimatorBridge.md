# PlayerAnimatorBridge

Bridge entre lógica de movimento e o Animator/SpriteRenderer do player.

## Responsabilidades
- Atualizar parâmetros do Animator (`isMoving`, `isPushing`, `dirX`, `dirY`).
- Controlar flip horizontal opcional baseado na direção.
- Executar animação temporária de push.
- Permitir habilitar/desabilitar o flip via runtime.
- Manter última direção horizontal para orientar sprites ao mover verticalmente.

## Fluxo de Uso
1. Arrastar `Animator` e `SpriteRenderer` do PlayerSprite para o Inspector.
2. Chamar `SetMoving()` e `SetDirection()` a partir do PlayerMover.
3. Usar `PulsePush()` quando o jogador empurrar uma caixa.
4. Usar `ResetAll()` ao reiniciar nível ou parar movimento.

## API
- **SetMoving(bool v)**  
  Liga/desliga parâmetro `isMoving`.

- **SetDirection(Vector2Int dir)**  
  Atualiza `dirX`, `dirY` e controla o flip horizontal.

- **PulsePush(float seconds)**  
  Coroutine que seta `isPushing` por alguns segundos.

- **ResetAll()**  
  Reseta todos os parâmetros do animator.

- **SetFlipXEnabled(bool)** / **GetFlipXEnabled()**

## Notas
- `keepFacingOnVertical=true` mantém última direção horizontal ao andar verticalmente.
- `flipXEnabled=false` impede qualquer flip visual.