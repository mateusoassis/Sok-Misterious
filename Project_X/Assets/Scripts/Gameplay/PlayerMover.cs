using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// grid, 1 input = 1 célula somente
// 4 direções (sem diagonal)

public class PlayerMover : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("1 unit = 1 tile se tile = 32px")]
    public float cellSize = 1f;

    // movimentação e controle do player
    private Vector2Int gridPos;
    private InputAction moveAction;
    private Vector2Int heldDir = Vector2Int.zero;

    private void Awake()
    {
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");

        moveAction.AddBinding("<Gamepad>/dpad");
        moveAction.AddBinding("<Gamepad>/leftStick");

        moveAction.Enable();
        SnapToGrid();
    }

    private void OnDestroy()
    {
        moveAction.Disable();
    }

    void Update()
    {
        Vector2 v = moveAction.ReadValue<Vector2>();
        Vector2Int rawDir = Vector2Int.zero;
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            rawDir = new Vector2Int((int)Mathf.Sign(v.x), 0);
        }
        else if (Mathf.Abs(v.y) > 0f)
        {
            rawDir = new Vector2Int(0, (int)Mathf.Sign(v.y));
        }

        if (rawDir != Vector2Int.zero && heldDir == Vector2Int.zero)
        {
            Step(rawDir);
        }

        heldDir = rawDir;
    }

    private void Step(Vector2Int dir)
    {
        gridPos += dir;
        transform.position = GridToWorld(gridPos);
    }

    private void SnapToGrid()
    {
        float inv = 1f / cellSize;
        int gx = Mathf.RoundToInt(transform.position.x * inv);
        int gy = Mathf.RoundToInt(transform.position.y * inv);
        gridPos = new Vector2Int(gx, gy);
        transform.position = GridToWorld(gridPos);
    }

    private Vector3 GridToWorld(Vector2Int gp)
    {
        return new Vector3(gp.x * cellSize, gp.y * cellSize, transform.position.z);
    }
}
