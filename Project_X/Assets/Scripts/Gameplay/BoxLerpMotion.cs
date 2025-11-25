using UnityEngine;
using System.Collections;

public class BoxLerpMotion : MonoBehaviour
{
    // Duração total do movimento interpolado da caixa.
    // Deve idealmente combinar com a duração do movimento do player.
    [SerializeField] float moveDuration = 0.10f;

    // Referência pra coroutine ativa, permitindo cancelar movimentos.
    Coroutine coro;

    // Indica se a caixa está em movimento no momento.
    public bool IsMoving { get; private set; }

    // Cancela qualquer animação em andamento e "teleporta" a caixa para o destino.
    public void CancelAndSnap(Vector3 worldTarget)
    {
        if (coro != null) StopCoroutine(coro);
        IsMoving = false;
        transform.position = worldTarget;
    }

    // Movimenta suavemente a caixa até a posição-alvo.
    // Pode receber um callback para acionar algo ao final.
    public IEnumerator MoveTo(Vector3 worldTarget, System.Action onComplete = null)
    {
        // Impede empurrões repetidos durante a animação.
        if (IsMoving) yield break;
        IsMoving = true;

        Vector3 start = transform.position;
        float t = 0f, dur = Mathf.Max(0.0001f, moveDuration);

        // Lerp suave baseado no deltaTime.
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(start, worldTarget, Mathf.Clamp01(t));
            yield return null;
        }

        // Garante snap exato ao final (evita drift flutuante).
        transform.position = worldTarget;
        IsMoving = false;

        // Dispara callback (se existir).
        onComplete?.Invoke();
    }
}