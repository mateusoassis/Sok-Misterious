using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Controla movimento em grid do player:
// - 4 direções, 1 input = 1 célula
// - Empurrar caixas (BoxIdentifier na Layer "Solid")
// - UNDO (Z no teclado, Y no gamepad)
public class PlayerMover : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("1 unit = 1 tile; se tile=32px e PPU=32, deixe 1.")]
    public float cellSize = 1f;

    [Header("Repeat")]
    [Tooltip("Se ligado, segurar direção repete passos.")]
    public bool enableRepeat = true;
    public float firstRepeatDelay = 0.25f; // delay inicial do repeat
    public float repeatInterval   = 0.12f; // intervalo entre repetições

    [Header("Colisão e Push")]
    [Tooltip("Camadas que bloqueiam (ex.: Solid). A Box também deve estar nessa Layer.")]
    public LayerMask solidMask;

    // Posição lógica do player em coordenadas de grid
    private Vector2Int gridPos;

    // Input System (criados em código)
    private InputAction moveAction;
    private InputAction undoAction;

    // Controle de repeat
    private Vector2Int heldDir = Vector2Int.zero;
    private float nextRepeatTime = 0f;

    // ---------- UNDO ----------
    private struct MoveRecord
    {
        public Vector2Int playerFrom, playerTo;
        public BoxIdentifier box; // null se não houve push
        public Vector2Int boxFrom, boxTo;
    }

    // Histórico de movimentos (para UNDO)
    private readonly List<MoveRecord> history = new List<MoveRecord>(256);
    // ---------------------------

    [Header("Animator")]
    [SerializeField] PlayerLerpMotion motion;      // componente de movimento suave
    [SerializeField] PlayerAnimatorBridge bridge;  // ponte para Animator / flip do sprite

    private void Awake()
    {
        // Configura input de movimento (teclado + gamepad) via Input System
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/dpad");
        moveAction.AddBinding("<Gamepad>/leftStick");

        // Input de UNDO: Z teclado, Y (norte) no gamepad
        undoAction = new InputAction("Undo", InputActionType.Button);
        undoAction.AddBinding("<Keyboard>/z");
        undoAction.AddBinding("<Gamepad>/buttonNorth");

        moveAction.Enable();
        undoAction.Enable();

        SnapToGrid();     // alinha ao grid e define gridPos
        history.Clear();  // limpa histórico do nível atual
    }

    private void OnDestroy()
    {
        // Boas práticas: desabilita actions ao destruir
        moveAction.Disable();
        undoAction.Disable();
    }

    private void Update()
    {
        // 1) UNDO tem prioridade sobre input de movimento
        if (undoAction.WasPressedThisFrame())
        {
            // trava UNDO se houver animação de movimento rolando
            if (motion != null && motion.IsMoving) return;
            if (AnyBoxIsMoving()) return; // helper abaixo

            UndoLast();

            // reset do repeat depois do undo
            heldDir = Vector2Int.zero;
            nextRepeatTime = Time.time + firstRepeatDelay;
            return;
        }

        // 2) Se está animando um movimento, não aceita novo input
        if (motion != null && motion.IsMoving)
        {
            heldDir = Vector2Int.zero; // evita repeat ficar armado
            return;
        }

        // 3) Lê input bruto e converte para 4 direções (sem diagonal)
        Vector2 v = moveAction.ReadValue<Vector2>();
        Vector2Int rawDir = Vector2Int.zero;

        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            rawDir = new Vector2Int((int)Mathf.Sign(v.x), 0);
        else if (Mathf.Abs(v.y) > 0f)
            rawDir = new Vector2Int(0, (int)Mathf.Sign(v.y));

        // 4) Modo sem repeat → 1 toque = 1 passo
        if (!enableRepeat)
        {
            if (rawDir != Vector2Int.zero && heldDir == Vector2Int.zero)
                TryStep(rawDir);   // só dispara no "edge" do input

            heldDir = rawDir;
            return;
        }

        // 5) Modo com repeat (segurar direção faz andar sozinho)
        if (rawDir != Vector2Int.zero)
        {
            bool justPressed = (heldDir == Vector2Int.zero) || (rawDir != heldDir);

            if (justPressed || Time.time >= nextRepeatTime)
            {
                TryStep(rawDir);

                // Primeiro passo tem delay maior, depois entra no intervalo fixo
                nextRepeatTime = Time.time + (justPressed ? firstRepeatDelay : repeatInterval);
                heldDir = rawDir;
            }
        }
        else
        {
            // Soltou input → para repeat
            heldDir = Vector2Int.zero;
        }
    }

    // Verifica se qualquer Box ainda está em movimento (lerp)
    private bool AnyBoxIsMoving()
    {
        var motions = FindObjectsOfType<BoxLerpMotion>();
        foreach (var m in motions)
            if (m != null && m.IsMoving) return true;

        return false;
    }

    // Checa se uma célula de grid é um Goal
    private bool IsGoalCell(Vector2Int gridCell)
    {
        // 1) Tenta achar por física (se Goal tiver Collider2D)
        Vector3 wp = GridToWorld(gridCell);
        var cols = Physics2D.OverlapPointAll(wp);
        if (cols != null)
        {
            foreach (var c in cols)
            {
                if (c == null) continue;
                var gi = c.GetComponent<GoalIdentifier>() ?? c.GetComponentInParent<GoalIdentifier>();
                if (gi != null) return true;
            }
        }

        // 2) Fallback: compara por posição (se Goal NÃO tiver collider)
        var goals = UnityEngine.Object.FindObjectsOfType<GoalIdentifier>(); // poucas instâncias, ok para MVP
        foreach (var gi in goals)
        {
            if (gi == null) continue;
            if (WorldToGrid(gi.transform.position) == gridCell)
                return true;
        }

        return false;
    }

    // Checagem de estado de UNDO para UI/botões
    public bool CanUndoNow()
    {
        if (!enabled) return false;                          // se PlayerMover está desativado (pausa, etc.)
        if (motion != null && motion.IsMoving) return false; // player animando
        var boxes = FindObjectsOfType<BoxLerpMotion>();
        foreach (var b in boxes)
            if (b != null && b.IsMoving) return false;       // caixa animando

        return history.Count > 0;
    }

    // Chamado pela UI para tentar desfazer via botão
    public void TryUndoFromUI()
    {
        if (!CanUndoNow()) return;

        UndoLast();
        heldDir = Vector2Int.zero;
        nextRepeatTime = Time.time + firstRepeatDelay; // evita andar sozinho depois do undo
    }

    // Movimenta 1 célula: bloqueia se parede; empurra caixa se atrás estiver livre
    private void TryStep(Vector2Int dir)
    {
        if (motion != null && motion.IsMoving) return;

        Vector2Int target = gridPos + dir;
        bridge?.SetDirection(dir); // informa direção pro Animator
        Vector2 worldTarget = GridToWorld(target);

        // 1) Verifica Bounds via LevelManager (não deixa sair da área válida)
        var lm = LevelManager.Instance;
        bool hasBounds = (lm != null && lm.CurrentBounds != null);
        if (hasBounds && !lm.InsideBounds(worldTarget))
        {
            return;
        }

        // 2) Verifica colisão na célula alvo usando LayerMask sólido
        if (solidMask.value != 0)
        {
            var hit = Physics2D.OverlapPoint(worldTarget, solidMask);
            if (hit != null)
            {
                // Tentativa de empurrar caixa
                var box = hit.GetComponent<BoxIdentifier>() ?? hit.GetComponentInParent<BoxIdentifier>();

                // Se não é caixa → sólido puro (parede), bloqueia
                if (box == null)
                {
                    return;
                }

                // Cálculo da célula atrás da caixa (onde ela vai ser empurrada)
                Vector2Int boxGrid = WorldToGrid(box.transform.position);
                Vector2Int boxTarget = boxGrid + dir;
                Vector2 worldBoxTarget = GridToWorld(boxTarget);

                // 2.1) Não deixa empurrar caixa para fora dos Bounds
                if (hasBounds && !lm.InsideBounds(worldBoxTarget))
                {
                    return;
                }

                // 2.2) Checa se célula atrás da caixa está livre (ignorando a própria caixa)
                var hitsBehind = Physics2D.OverlapPointAll(worldBoxTarget, solidMask);
                bool behindBlocked = false;
                foreach (var h in hitsBehind)
                {
                    if (h == null) continue;
                    if (h.transform == box.transform || h.transform.IsChildOf(box.transform)) continue;
                    behindBlocked = true;
                    break;
                }
                if (behindBlocked) return;

                // 3) Registra movimento no histórico (para UNDO)
                history.Add(new MoveRecord
                {
                    playerFrom = gridPos,
                    playerTo = target,
                    box = box,
                    boxFrom = boxGrid,
                    boxTo = boxTarget
                });

                // 4) Empurra caixa (com LERP) e move player (com LERP) de forma sincronizada
                var boxMotion = box.GetComponent<BoxLerpMotion>();
                gridPos = target;

                // Player move (com animação de "andando")
                bridge?.SetMoving(true);
                StartCoroutine(motion.MoveBy(dir, () => bridge.SetMoving(false)));

                // Caixa move
                if (boxMotion != null)
                {
                    StartCoroutine(boxMotion.MoveTo(worldBoxTarget, () =>
                    {
                        // Ao terminar o lerp da Box, atualiza estado de goal
                        bool onGoal = IsGoalCell(WorldToGrid(worldBoxTarget));
                        var goalState = box.GetComponent<BoxGoalState>();
                        if (goalState != null) goalState.SetOnGoal(onGoal);

                        GameEvents.RaiseGoalsMaybeChanged();
                    }));
                }
                else
                {
                    // Fallback sem BoxLerpMotion: teleporta
                    box.transform.position = worldBoxTarget;
                    bool onGoal = IsGoalCell(WorldToGrid(worldBoxTarget));
                    var goalState = box.GetComponent<BoxGoalState>();
                    if (goalState != null) goalState.SetOnGoal(onGoal);
                    GameEvents.RaiseGoalsMaybeChanged();
                }

                // Feedback de “empurrando”
                StartCoroutine(bridge.PulsePush(0.15f));

                // Eventos de telemetria/métrica
                GameEvents.RaiseMove();
                GameEvents.RaisePush();
                return;
            }
        }

        // 3) Se célula destino está livre → passo simples + histórico para UNDO
        history.Add(new MoveRecord { playerFrom = gridPos, playerTo = target, box = null });
        gridPos = target;

        bridge?.SetMoving(true);
        StartCoroutine(motion.MoveBy(dir, () => bridge.SetMoving(false)));

        GameEvents.RaiseMove();
    }

    // Desfaz o último passo (e o empurrão, se houve)
    private void UndoLast()
    {
        if (history.Count == 0)
        {
            return;
        }

        int i = history.Count - 1;
        var rec = history[i];
        history.RemoveAt(i);

        // --- PLAYER: cancela LERP e "snapa" ---
        gridPos = rec.playerFrom;
        Vector3 playerTarget = GridToWorld(gridPos);

        if (motion != null) motion.CancelAndSnap(playerTarget);
        else transform.position = playerTarget;

        bridge?.ResetAll();

        // --- BOX (se houve push nesse movimento) ---
        if (rec.box != null && rec.box.gameObject != null)
        {
            var target = GridToWorld(rec.boxFrom);

            var boxMotion = rec.box.GetComponent<BoxLerpMotion>();
            if (boxMotion != null) boxMotion.CancelAndSnap(target);
            else rec.box.transform.position = target;

            // Atualiza o highlight/goals após o Undo
            var goalState = rec.box.GetComponent<BoxGoalState>();
            if (goalState != null) goalState.SetOnGoal(IsGoalCell(rec.boxFrom));
        }

        // Garante que física/transform estejam sincronizados
        Physics2D.SyncTransforms();

        GameEvents.RaiseUndo();
        GameEvents.RaiseGoalsMaybeChanged();

        // Reset do repeat depois do undo
        heldDir = Vector2Int.zero;
        nextRepeatTime = Time.time + firstRepeatDelay;
    }

    // ---- utilitários de gridPos ----
    private void SnapToGrid()
    {
        gridPos = WorldToGrid(transform.position);
        transform.position = GridToWorld(gridPos);
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float inv = 1f / cellSize;
        int gx = Mathf.RoundToInt(worldPos.x * inv);
        int gy = Mathf.RoundToInt(worldPos.y * inv);
        return new Vector2Int(gx, gy);
    }

    private Vector3 GridToWorld(Vector2Int gp)
    {
        return new Vector3(gp.x * cellSize, gp.y * cellSize, transform.position.z);
    }

#if UNITY_EDITOR
    // Desenha a célula atual do player no Scene View quando selecionado
    private void OnDrawGizmosSelected()
    {
        if (solidMask.value == 0) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
    }
#endif
}