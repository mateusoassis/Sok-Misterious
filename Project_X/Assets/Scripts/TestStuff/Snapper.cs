using UnityEngine;

/// <summary>
/// Ferramenta bem simples de editor:
/// - Alinha TODOS os filhos deste GameObject para o grid de 1 unidade.
/// - Útil pra arrumar level feito “no olho” e deixar tudo certinho em coordenadas inteiras.
/// 
/// A chamada é feita via menu de contexto no Inspector.
/// </summary>
public class Snapper : MonoBehaviour
{
    /// <summary>
    /// ContextMenu → aparece no Inspector do componente.
    /// Quando clicado, faz snap de todos os filhos para o grid (1 unidade).
    /// </summary>
    [ContextMenu("Snap children to grid (1 unit)")]
    void SnapChildren()
    {
        // Percorre todos os Transforms deste objeto e de todos os filhos (inclusive desativados)
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            // ignora o próprio objeto raiz, só queremos mexer nos filhos
            if (t == transform) continue;

            var p = t.position;

            // Arredonda X e Y para o inteiro mais próximo.
            // Mantém Z como está (pra não mexer em profundidade/camadas).
            t.position = new Vector3(
                Mathf.Round(p.x),
                Mathf.Round(p.y),
                p.z
            );
        }

        Debug.Log("[Snapper] Done."); // feedback no Console pra confirmar que rodou
    }
}