using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public InputAction MoveAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction ToggleCameraAction { get; private set; }
    public InputAction SnapPhotoAction { get; private set; }
    public InputAction ZoomAction { get; private set; }
    public InputAction ToggleGuideAction { get; private set; }
    public InputAction MousePositionAction { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MoveAction = new InputAction("Move", InputActionType.Value);
        MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        JumpAction = new InputAction("Jump", InputActionType.Button,
            "<Keyboard>/space");

        ToggleCameraAction = new InputAction("ToggleCamera", InputActionType.Button,
            "<Keyboard>/tab");

        SnapPhotoAction = new InputAction("SnapPhoto", InputActionType.Button,
            "<Mouse>/leftButton");

        ZoomAction = new InputAction("Zoom", InputActionType.Value,
            "<Mouse>/scroll/y");

        ToggleGuideAction = new InputAction("ToggleGuide", InputActionType.Button,
            "<Keyboard>/g");

        MousePositionAction = new InputAction("MousePosition", InputActionType.Value,
            "<Mouse>/position");

        EnableAll();
    }

    private void OnEnable() => EnableAll();
    private void OnDisable() => DisableAll();

    private void EnableAll()
    {
        MoveAction?.Enable();
        JumpAction?.Enable();
        ToggleCameraAction?.Enable();
        SnapPhotoAction?.Enable();
        ZoomAction?.Enable();
        ToggleGuideAction?.Enable();
        MousePositionAction?.Enable();
    }

    private void DisableAll()
    {
        MoveAction?.Disable();
        JumpAction?.Disable();
        ToggleCameraAction?.Disable();
        SnapPhotoAction?.Disable();
        ZoomAction?.Disable();
        ToggleGuideAction?.Disable();
        MousePositionAction?.Disable();
    }

    private void OnDestroy()
    {
        MoveAction?.Dispose();
        JumpAction?.Dispose();
        ToggleCameraAction?.Dispose();
        SnapPhotoAction?.Dispose();
        ZoomAction?.Dispose();
        ToggleGuideAction?.Dispose();
        MousePositionAction?.Dispose();
    }
}
