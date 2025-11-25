# BoxLerpMotion

## Visão Geral
`BoxLerpMotion` é responsável por **animar o movimento da caixa** durante um empurrão, realizando uma interpolação suave (lerp) da posição atual até a nova posição na grade.

Ele funciona como um "motorzinho de movimento" independente, usado quando o player empurra caixas.  
Assim, o movimento da caixa não é instantâneo — mas suave, consistente e visualmente claro.

---

## Responsabilidades
- Realizar movimento interpolado (Lerp) até uma posição-alvo.
- Bloquear movimento duplo (`IsMoving`).
- Cancelar movimentos em andamento e “teleportar” a caixa para o destino caso necessário.
- Executar um callback (`onComplete`) após finalizar a animação.

---

## Campos

### `moveDuration : float`
Tempo total (em segundos) para completar o movimento da caixa.  
Idealmente deve estar sincronizado com o movimento do player para parecer natural.

### `coro : Coroutine`
Referência interna para a coroutine ativa.

### `IsMoving : bool`
Indica se a caixa está em animação no momento.

---

## Métodos

### `CancelAndSnap(Vector3 worldTarget)`
- Interrompe qualquer animação atual.
- Desmarca `IsMoving`.
- Teleporta diretamente para a posição final.
- Usado quando algo externo força reposicionamento imediato.

### `IEnumerator MoveTo(Vector3 worldTarget, Action onComplete = null)`
- Inicia o movimento suave até o ponto desejado.
- Ignora chamadas se já estiver movendo.
- Usa `Vector3.Lerp` com base no tempo acumulado.
- Ao finalizar:
  - Ajusta posição final exata.
  - Libera `IsMoving`.
  - Dispara `onComplete` (se fornecido).

---

## Fluxo da animação

1. Player tenta empurrar a caixa.
2. Lógica valida se o próximo tile é válido.
3. A caixa recebe:
   ```csharp
   StartCoroutine(boxLerpMotion.MoveTo(destino));
   ```
4. Durante `moveDuration`, a caixa desliza suavemente até a célula.
5. No final, ela se alinha perfeitamente à grade.

---

## Benefícios desse padrão
- Movimentação previsível e suave.
- Evita jumps instantâneos de posição.
- Permite adicionar efeitos (som, partículas, etc.) no callback.

---

## Código original

```csharp
using UnityEngine;
using System.Collections;

public class BoxLerpMotion : MonoBehaviour
{
    [SerializeField] float moveDuration = 0.10f; // combine com o Player
    Coroutine coro;
    public bool IsMoving { get; private set; }

    public void CancelAndSnap(Vector3 worldTarget)
    {
        if (coro != null) StopCoroutine(coro);
        IsMoving = false;
        transform.position = worldTarget;
    }

    public IEnumerator MoveTo(Vector3 worldTarget, System.Action onComplete = null)
    {
        if (IsMoving) yield break;
        IsMoving = true;

        Vector3 start = transform.position;
        float t = 0f, dur = Mathf.Max(0.0001f, moveDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(start, worldTarget, Mathf.Clamp01(t));
            yield return null;
        }

        transform.position = worldTarget;
        IsMoving = false;
        onComplete?.Invoke();
    }
}
```