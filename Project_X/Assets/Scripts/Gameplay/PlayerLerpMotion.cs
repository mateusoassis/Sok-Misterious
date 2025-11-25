using UnityEngine;
using System.Collections;

public class PlayerLerpMotion : MonoBehaviour
{
    [SerializeField] float moveDuration = 0.10f; 
    // Tempo total para mover 1 tile (combina com animação do Player)

    Coroutine moveCoro;              // Referência da coroutine atual
    public bool IsMoving { get; private set; }   // Indica se está atualmente em movimento

    /// <summary>
    /// Cancela movimento atual (se houver) e teleporta instantaneamente para o target.
    /// Usado em resets, undo instantâneo ou recarregar level.
    /// </summary>
    public void CancelAndSnap(Vector3 target) {
        if (moveCoro != null)
            StopCoroutine(moveCoro);

        IsMoving = false;
        transform.position = target;
    }

    /// <summary>
    /// Move o player suavemente de 1 tile para outro,
    /// baseado em um deslocamento inteiro (dir.x, dir.y).
    /// Ex.: dir = (1,0) → move 1 tile para direita.
    /// </summary>
    public IEnumerator MoveBy(Vector2Int dir, System.Action onComplete = null) 
    {
        if (IsMoving)
            yield break;       // impede sobreposição de movimentos

        IsMoving = true;

        Vector3 start = transform.position;
        Vector3 end   = start + new Vector3(dir.x, dir.y, 0f);

        float t = 0f;
        float dur = Mathf.Max(0.0001f, moveDuration); // evita divisão por zero

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
            yield return null;
        }

        // garante que a posição final é exatamente o ponto alvo
        transform.position = end;
        IsMoving = false;

        onComplete?.Invoke(); // callback opcional
    }
}