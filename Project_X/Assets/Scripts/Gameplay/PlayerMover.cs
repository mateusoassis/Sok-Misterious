using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// grid, 1 input = 1 célula; 4 direções (sem diagonal)
// push de caixa (objetos na Layer "Solid" com BoxIdentifier)
// UNDO (Z no teclado, Y no gamepad)
public class PlayerMover : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("1 unit = 1 tile; se tile=32px e PPU=32, deixe 1.")]
    public float cellSize = 1f;

    [Header("Repeat")]
    [Tooltip("Se ligado, segurar direção repete passos.")]
    public bool enableRepeat = true;
    public float firstRepeatDelay = 0.25f;
    public float repeatInterval   = 0.12f;

    [Header("Colisão e Push")]
    [Tooltip("Camadas que bloqueiam (ex.: Solid). A Box também deve estar nessa Layer.")]
    public LayerMask solidMask;

    // estado lógico do player (em células)
    private Vector2Int gridPos;

    // Input System (criadas em código)
    private InputAction moveAction;
    private InputAction undoAction;

    // controle de repeat
    private Vector2Int heldDir = Vector2Int.zero;
    private float nextRepeatTime = 0f;

    // ---------- UNDO ----------
    private struct MoveRecord
    {
        public Vector2Int playerFrom, playerTo;
        public BoxIdentifier box; // lembrar que é null se não houve push
        public Vector2Int boxFrom, boxTo;
    }
    private readonly List<MoveRecord> history = new List<MoveRecord>(256);
    // ---------------------------
    [Header("Animator")]
    [SerializeField] PlayerLerpMotion motion;
    [SerializeField] PlayerAnimatorBridge bridge;

    private void Awake()
    {
        // movimento (Vector2) -> teclado + gamepad
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/dpad");
        moveAction.AddBinding("<Gamepad>/leftStick");

        // UNDO: Z teclado, Y (norte) no gamepad
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
        moveAction.Disable();
        undoAction.Disable();
    }

    private void Update()
    {
        // UNDO primeiro pra não misturar com movimento
        if (undoAction.WasPressedThisFrame())
        {
            // travar enquanto houver lerp rolando
            if (motion != null && motion.IsMoving) return;
            if (AnyBoxIsMoving()) return; // helper abaixo

            UndoLast();

            // zerar repeat imediatamente
            heldDir = Vector2Int.zero;
            nextRepeatTime = Time.time + firstRepeatDelay; // “reseta” o cronômetro
            return;
        }

        // se está animando um movimento, não aceita novo input
        if (motion != null && motion.IsMoving)
        {
            heldDir = Vector2Int.zero;     // evita repeat armar passo extra
            return;
        }

        // lê input e transforma pra 4 direções (sem diagonal)
        Vector2 v = moveAction.ReadValue<Vector2>();
        Vector2Int rawDir = Vector2Int.zero;
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            rawDir = new Vector2Int((int)Mathf.Sign(v.x), 0);
        else if (Mathf.Abs(v.y) > 0f)
            rawDir = new Vector2Int(0, (int)Mathf.Sign(v.y));

        if (!enableRepeat)
        {
            // 1 toque = 1 passo
            if (rawDir != Vector2Int.zero && heldDir == Vector2Int.zero)
                TryStep(rawDir);
            heldDir = rawDir;
            return;
        }

        // com repeat
        if (rawDir != Vector2Int.zero)
        {
            bool justPressed = (heldDir == Vector2Int.zero) || (rawDir != heldDir);
            if (justPressed || Time.time >= nextRepeatTime)
            {
                TryStep(rawDir);
                nextRepeatTime = Time.time + (justPressed ? firstRepeatDelay : repeatInterval);
                heldDir = rawDir;
            }
        }
        else
        {
            heldDir = Vector2Int.zero;
        }
    }

    private bool AnyBoxIsMoving()
    {
        var motions = FindObjectsOfType<BoxLerpMotion>();
        foreach (var m in motions) if (m != null && m.IsMoving) return true;
        return false;
    }

    // check se o goal é a célula alvo
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

    public bool CanUndoNow()
    {
        if (!enabled) return false;                          // pausado → PlayerMover desabilitado
        if (motion != null && motion.IsMoving) return false; // lerp do player
        var boxes = FindObjectsOfType<BoxLerpMotion>();
        foreach (var b in boxes) if (b != null && b.IsMoving) return false; // lerp de box
        return history.Count > 0;
    }

    public void TryUndoFromUI()
    {
        if (!CanUndoNow()) return;
        UndoLast();
        heldDir = Vector2Int.zero;
        nextRepeatTime = Time.time + firstRepeatDelay; // evita “andar sozinho” depois do undo
    }

    // movimenta 1 célula, bloqueia se parede, empurra caixa se atrás estiver livre
    private void TryStep(Vector2Int dir)
    {
        if (motion != null && motion.IsMoving) return;
        Vector2Int target = gridPos + dir;
        bridge?.SetDirection(dir);
        Vector2 worldTarget = GridToWorld(target);

        // bounds bloquear player andando
        var lm = LevelManager.Instance;
        bool hasBounds = (lm != null && lm.CurrentBounds != null);
        if (hasBounds && !lm.InsideBounds(worldTarget))
        {
            return;
        }

        if (solidMask.value != 0)
        {
            // ocupante no CENTRO da célula destino?
            var hit = Physics2D.OverlapPoint(worldTarget, solidMask);
            if (hit != null)
            {
                // acertou caixa?
                var box = hit.GetComponent<BoxIdentifier>() ?? hit.GetComponentInParent<BoxIdentifier>();

                // parede/qualquer sólido não-caixa -> bloqueia
                if (box == null)
                {
                    return;
                }

                // tentar empurrar a caixa, célula ATRÁS dela precisa estar livre
                Vector2Int boxGrid = WorldToGrid(box.transform.position);
                Vector2Int boxTarget = boxGrid + dir;
                Vector2 worldBoxTarget = GridToWorld(boxTarget);

                // bloqueio de empurrar box através de BOUNDS
                if (hasBounds && !lm.InsideBounds(worldBoxTarget))
                {
                    return;
                }

                // checa ocupantes atrás, ignorando a PRÓPRIA caixa
                var hitsBehind = Physics2D.OverlapPointAll(worldBoxTarget, solidMask);
                bool behindBlocked = false;
                foreach (var h in hitsBehind)
                {
                    if (h == null) continue;
                    if (h.transform == box.transform || h.transform.IsChildOf(box.transform)) continue;
                    behindBlocked = true; break;
                }
                if (behindBlocked) return;

                // registra no histórico (para UNDO)
                history.Add(new MoveRecord
                {
                    playerFrom = gridPos,
                    playerTo = target,
                    box = box,
                    boxFrom = boxGrid,
                    boxTo = boxTarget
                });

                // empurra a caixa com LERP e move o player com LERP (sincronizados)
                var boxMotion = box.GetComponent<BoxLerpMotion>();
                gridPos = target;

                // player move
                bridge?.SetMoving(true);
                StartCoroutine(motion.MoveBy(dir, () => bridge.SetMoving(false)));

                // caixa move
                if (boxMotion != null)
                {
                    StartCoroutine(boxMotion.MoveTo(worldBoxTarget, () =>
                    {
                        // >>> (A) AQUI: terminou o lerp da Box
                        bool onGoal = IsGoalCell(WorldToGrid(worldBoxTarget));
                        var goalState = box.GetComponent<BoxGoalState>();
                        if (goalState != null) goalState.SetOnGoal(onGoal);

                        GameEvents.RaiseGoalsMaybeChanged();
                    }));
                }
                else
                {
                    // fallback (sem BoxLerpMotion)
                    box.transform.position = worldBoxTarget;
                    bool onGoal = IsGoalCell(WorldToGrid(worldBoxTarget));
                    var goalState = box.GetComponent<BoxGoalState>();
                    if (goalState != null) goalState.SetOnGoal(onGoal);
                    GameEvents.RaiseGoalsMaybeChanged();
                }

                // feedback “push”
                StartCoroutine(bridge.PulsePush(0.15f));

                // métricas
                GameEvents.RaiseMove();
                GameEvents.RaisePush();
                return;
            }
        }

        // player livre -> passo simples + registra para UNDO (com LERP no player)
        history.Add(new MoveRecord { playerFrom = gridPos, playerTo = target, box = null });
        gridPos = target;

        bridge?.SetMoving(true);
        StartCoroutine(motion.MoveBy(dir, () => bridge.SetMoving(false)));

        GameEvents.RaiseMove();
    }

    // desfaz o último passo (e o empurrão, se houve)
    private void UndoLast()
    {
        if (history.Count == 0)
        {
            return;
        }

        int i = history.Count - 1;
        var rec = history[i];
        history.RemoveAt(i);

        // --- PLAYER: cancela lerp e "snapa" ---
        gridPos = rec.playerFrom;
        Vector3 playerTarget = GridToWorld(gridPos);

        if (motion != null) motion.CancelAndSnap(playerTarget);
        else transform.position = playerTarget;

        bridge?.ResetAll();

        if (rec.box != null && rec.box.gameObject != null)
        {
            var target = GridToWorld(rec.boxFrom);

            var boxMotion = rec.box.GetComponent<BoxLerpMotion>();
            if (boxMotion != null) boxMotion.CancelAndSnap(target);
            else rec.box.transform.position = target;

            // atualiza o highlight depois do Undo
            var goalState = rec.box.GetComponent<BoxGoalState>();
            if (goalState != null) goalState.SetOnGoal(IsGoalCell(rec.boxFrom));
        }
        Physics2D.SyncTransforms();
        GameEvents.RaiseUndo();
        GameEvents.RaiseGoalsMaybeChanged();
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

    // desenha a célula atual (só pra referência visual)
    private void OnDrawGizmosSelected()
    {
        if (solidMask.value == 0) return;
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
    }
#endif
}