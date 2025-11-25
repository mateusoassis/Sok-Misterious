using UnityEngine;
using System.Collections;

public class PlayerAnimatorBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;                // Animator do player (controla estados e blend trees)
    [SerializeField] SpriteRenderer sr;            // Sprite principal do player (para flip e troca visual)

    [Header("Flip X (opcional)")]
    [SerializeField] bool flipXEnabled = true;     // se true, sprite vira pra esquerda/direita
    [SerializeField] bool keepFacingOnVertical = true; // mantém a direção horizontal ao mover só no eixo Y
    int lastFacingX = 1; // 1 = direita, -1 = esquerda (inicio padrão)

    void Awake() {
        // fallback automático se não for arrastado no inspector
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!sr)   sr   = GetComponentInChildren<SpriteRenderer>(true);
    }

    // Liga/desliga o bool "isMoving" no Animator
    public void SetMoving(bool v) => anim.SetBool("isMoving", v);

    // Atualiza direção X/Y no Animator e controla flip horizontal
    public void SetDirection(Vector2Int dir) {
        anim.SetFloat("dirX", dir.x);
        anim.SetFloat("dirY", dir.y);

        // Controle do flip visual
        if (flipXEnabled && sr) {

            if (dir.x != 0) {
                lastFacingX = dir.x; // só atualiza quando há input horizontal
            } 
            else if (!keepFacingOnVertical) {
                // se desligado, personagem "desvira" ao subir/descer
                lastFacingX = 1;
            }

            // aplica flip se for voltado para a esquerda
            sr.flipX = (lastFacingX < 0);
        }
    }

    // Aciona animação de empurrar por X segundos (ex.: empurrar Box)
    public IEnumerator PulsePush(float seconds) {
        anim.SetBool("isPushing", true);
        yield return new WaitForSeconds(seconds);
        anim.SetBool("isPushing", false);
    }

    // Reset geral de estado (usado ao reiniciar nível, parar movimento brusco, etc.)
    public void ResetAll() {
        anim.SetBool("isMoving", false);
        anim.SetBool("isPushing", false);
        // Mantém flip atual para preservar a direção
    }

    // Permite ativar/desativar flip X via opções do jogo
    public void SetFlipXEnabled(bool enabled) => flipXEnabled = enabled;
    public bool GetFlipXEnabled() => flipXEnabled;
}