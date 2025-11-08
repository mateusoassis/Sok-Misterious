using UnityEngine;

/// <summary>
/// Ferramenta de authoring/validação para o prefab de Level.
/// - Checa se tem Bounds com BoxCollider2D.
/// - Checa se tem Goals e Boxes suficientes.
/// - Checa se Goals/Boxes estão dentro dos Bounds.
/// - Checa se Goals/Boxes estão alinhados ao grid local (multiplo de cellSize).
/// - Opcional: faz snap pro grid local (localPosition) em todos os filhos (exceto root e Bounds).
/// 
/// Uso:
/// 1) Coloque este componente na raiz do prefab de level (mesmo GO que tem "Bounds" como filho).
/// 2) No Inspector, use o menu de contexto "Validate Level" (···) ou o botão do inspector se você quiser expor.
/// </summary>
public class LevelValidator : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("Tamanho da célula em unidades (deve bater com o cellSize do Player).")]
    public float cellSize = 1f;

    [Tooltip("Se true, o ValidateLevel também faz snap pro grid local (use com cuidado).")]
    public bool snapToGridOnValidate = false;

    [Header("Debug")]
    [Tooltip("Se true, loga detalhes da validação no Console.")]
    public bool verbose = true;

    // ---------- MENU DE CONTEXTO ----------

    [ContextMenu("Validate Level")]
    public void ValidateLevel()
    {
        int errors = 0;
        int warnings = 0;

        if (verbose)
            Debug.Log($"[LevelValidator] Validando level '{name}'...");

        // 1) Bounds
        var boundsTf = transform.Find("Bounds");
        BoxCollider2D boundsCol = null;
        if (boundsTf == null)
        {
            Debug.LogError($"[LevelValidator] ({name}) Objeto filho 'Bounds' não encontrado.");
            errors++;
        }
        else
        {
            boundsCol = boundsTf.GetComponent<BoxCollider2D>();
            if (!boundsCol)
            {
                Debug.LogError($"[LevelValidator] ({name}) 'Bounds' encontrado, mas sem BoxCollider2D.");
                errors++;
            }
        }

        // 2) Goals e Boxes
        var goals = GetComponentsInChildren<GoalIdentifier>(true);
        var boxes = GetComponentsInChildren<BoxIdentifier>(true);

        if (goals.Length == 0)
        {
            Debug.LogError($"[LevelValidator] ({name}) Nenhum GoalIdentifier encontrado (precisa de pelo menos 1 goal).");
            errors++;
        }

        if (boxes.Length == 0)
        {
            Debug.LogWarning($"[LevelValidator] ({name}) Nenhum BoxIdentifier encontrado. " +
                             "É um level sem caixas? (ok se for proposital).");
            warnings++;
        }

        if (goals.Length > 0 && boxes.Length > goals.Length)
        {
            Debug.LogWarning($"[LevelValidator] ({name}) boxes({boxes.Length}) > goals({goals.Length}). " +
                             "Pode ser ok, mas confira se é intencional.");
            warnings++;
        }

        // 3) Fora dos Bounds
        if (boundsCol != null)
        {
            foreach (var box in boxes)
            {
                if (!boundsCol.OverlapPoint(box.transform.position))
                {
                    Debug.LogWarning($"[LevelValidator] ({name}) Box '{box.name}' está fora dos Bounds.");
                    warnings++;
                }
            }

            foreach (var g in goals)
            {
                if (!boundsCol.OverlapPoint(g.transform.position))
                {
                    Debug.LogWarning($"[LevelValidator] ({name}) Goal '{g.name}' está fora dos Bounds.");
                    warnings++;
                }
            }
        }

        // 4) Checagem de alinhamento no grid local (NOVA)
        int misaligned = CheckGridAlignment(goals, boxes);
        warnings += misaligned;

        // 5) Snap opcional pro grid local
        if (snapToGridOnValidate && cellSize > 0f)
        {
            int snapped = SnapChildrenToGrid();
            if (snapped > 0)
            {
                Debug.Log($"[LevelValidator] ({name}) SnapToGrid aplicado em {snapped} objetos (cellSize={cellSize}).");
            }
        }

        // Resultado final
        if (errors == 0 && warnings == 0)
        {
            Debug.Log($"[LevelValidator] ({name}) OK ✔ (sem erros nem warnings).");
        }
        else
        {
            Debug.Log($"[LevelValidator] ({name}) Resultado: {errors} erros, {warnings} avisos.");
        }
    }

    [ContextMenu("Snap Children To Grid (only)")]
    public void ContextSnapChildrenToGrid()
    {
        int count = SnapChildrenToGrid();
        Debug.Log($"[LevelValidator] ({name}) SnapToGrid manual: {count} objetos ajustados (cellSize={cellSize}).");
    }

    // ---------- IMPLEMENTAÇÃO ----------

    /// <summary>
    /// Checa se Goals e Boxes estão alinhados ao grid local (multiplo de cellSize).
    /// Não altera nada, só gera warnings.
    /// </summary>
    int CheckGridAlignment(GoalIdentifier[] goals, BoxIdentifier[] boxes)
    {
        if (cellSize <= 0f) return 0;

        int warnings = 0;
        float eps = 0.01f; // tolerância

        void CheckTransform(Transform t, string label)
        {
            var local = t.localPosition;

            float gx = local.x / cellSize;
            float gy = local.y / cellSize;

            float rx = Mathf.Round(gx);
            float ry = Mathf.Round(gy);

            float dx = Mathf.Abs(gx - rx);
            float dy = Mathf.Abs(gy - ry);

            if (dx > eps || dy > eps)
            {
                Debug.LogWarning(
                    $"[LevelValidator] ({name}) {label} '{t.name}' fora do grid local: " +
                    $"pos=({local.x:F2}, {local.y:F2}), " +
                    $"grid=({gx:F2}, {gy:F2}) → deveria estar perto de " +
                    $"({(rx * cellSize):F2}, {(ry * cellSize):F2})");
                warnings++;
            }
        }

        foreach (var g in goals)
            CheckTransform(g.transform, "Goal");

        foreach (var b in boxes)
            CheckTransform(b.transform, "Box");

        return warnings;
    }

    /// <summary>
    /// Faz snap de TODOS os filhos ao grid local, exceto:
    /// - o próprio root
    /// - o objeto "Bounds"
    /// </summary>
    int SnapChildrenToGrid()
    {
        if (cellSize <= 0f) return 0;

        int changed = 0;

        foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
        {
            // pula o root do level
            if (child == transform)
                continue;

            // pula o Bounds (não queremos mexer no colliderzão)
            if (child.name == "Bounds")
                continue;

            var local = child.localPosition;

            // converte pra "coordenada de grid"
            float gx = local.x / cellSize;
            float gy = local.y / cellSize;

            // arredonda tipo 4.97 -> 5, 3.06 -> 3
            gx = Mathf.Round(gx);
            gy = Mathf.Round(gy);

            float sx = gx * cellSize;
            float sy = gy * cellSize;

            if (!Mathf.Approximately(local.x, sx) || !Mathf.Approximately(local.y, sy))
            {
                child.localPosition = new Vector3(sx, sy, local.z);
                changed++;
            }
        }
        return changed;
    }
}