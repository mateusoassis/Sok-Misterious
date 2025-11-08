using UnityEngine;
using System.Collections;

public class PlayerAnimatorBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;                // arrasta o Animator do PlayerSprite
    [SerializeField] SpriteRenderer sr;            // arrasta o SpriteRenderer do PlayerSprite

    [Header("Flip X (opcional)")]
    [SerializeField] bool flipXEnabled = true;
    [SerializeField] bool keepFacingOnVertical = true; // mantém direção anterior quando mover só pra cima/baixo
    int lastFacingX = 1; // 1 = direita, -1 = esquerda

    void Awake() {
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!sr)   sr   = GetComponentInChildren<SpriteRenderer>(true);
    }

    public void SetMoving(bool v) => anim.SetBool("isMoving", v);

    public void SetDirection(Vector2Int dir) {
        anim.SetFloat("dirX", dir.x);
        anim.SetFloat("dirY", dir.y);

        // Flip X opcional (só fará efeito quando você marcar no Inspector)
        if (flipXEnabled && sr) {
            if (dir.x != 0) {
                lastFacingX = dir.x;                // atualiza quando há input horizontal
            } else if (!keepFacingOnVertical) {
                // se preferir, pode "desflipar" no vertical; por padrão mantemos a última face
                lastFacingX = 1;
            }
            sr.flipX = (lastFacingX < 0);
        }
    }

    public IEnumerator PulsePush(float seconds) {
        anim.SetBool("isPushing", true);
        yield return new WaitForSeconds(seconds);
        anim.SetBool("isPushing", false);
    }

    public void ResetAll() {
        anim.SetBool("isMoving", false);
        anim.SetBool("isPushing", false);
        // não mexe no flip aqui; mantém orientação atual
    }

    // Helpers pra ligar/desligar no runtime (se quiser via menu de opções)
    public void SetFlipXEnabled(bool enabled) => flipXEnabled = enabled;
    public bool GetFlipXEnabled() => flipXEnabled;
}
