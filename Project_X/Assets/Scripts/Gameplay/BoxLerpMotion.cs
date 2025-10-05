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