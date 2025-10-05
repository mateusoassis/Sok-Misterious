using UnityEngine;
using System.Collections;

public class PlayerLerpMotion : MonoBehaviour
{
    [SerializeField] float moveDuration = 0.10f;
    Coroutine moveCoro;
    public bool IsMoving { get; private set; }

    public void CancelAndSnap(Vector3 target) {
        if (moveCoro != null) StopCoroutine(moveCoro);
        IsMoving = false;
        transform.position = target;
    }

    public IEnumerator MoveBy(Vector2Int dir, System.Action onComplete = null) {
        if (IsMoving) yield break;
        IsMoving = true;
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(dir.x, dir.y, 0f);
        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
            yield return null;
        }
        transform.position = end;
        IsMoving = false;
        onComplete?.Invoke();
    }
}