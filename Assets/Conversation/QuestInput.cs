using UnityEngine;
using UnityEngine.InputSystem;

// OpenXR actions are enabled before the session starts. Desktop controls stay separate.
public static class QuestInput
{
    public static bool Active => Application.platform == RuntimePlatform.Android;
    static InputActionMap actions;
    public static int SuppressJumpFrame=-1;
    public static bool JumpPressed => Time.frameCount!=SuppressJumpFrame&&Jump!=null&&Jump.WasPressedThisFrame();
    public static InputAction Move, Turn, Jump, Talk, Boost, Switch, Menu, Sound, HeadPosition, HeadRotation, HeadTracked;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        actions?.Dispose();
        if (!Active) return;
        actions = new InputActionMap("BB8 Quest");
        Move = Action("Move", "<XRController>{LeftHand}/primary2DAxis", InputActionType.Value);
        Turn = Action("Turn", "<XRController>{RightHand}/primary2DAxis", InputActionType.Value);
        Jump = Action("Jump", "<XRController>{RightHand}/primaryButton");
        Talk = Action("Talk", "<XRController>{RightHand}/trigger");
        Boost = Action("Boost", "<XRController>{LeftHand}/trigger");
        Switch = Action("Character", "<XRController>{LeftHand}/primaryButton");
        Menu = Action("Menu", "<XRController>{RightHand}/secondaryButton");
        Sound = Action("Sound", "<XRController>{LeftHand}/secondaryButton");
        HeadPosition = Action("Head position", "<XRHMD>/centerEyePosition", InputActionType.Value);
        HeadRotation = Action("Head rotation", "<XRHMD>/centerEyeRotation", InputActionType.Value);
        HeadTracked = Action("Head tracked", "<XRHMD>/isTracked");
        actions.Enable();
    }
    static InputAction Action(string name, string binding, InputActionType type = InputActionType.Button) => actions.AddAction(name, type, binding);
    public static Vector2 Movement => DeadZone(Move?.ReadValue<Vector2>() ?? Vector2.zero);
    public static bool Talking => Active ? Talk != null && Talk.IsPressed() : Input.GetKey(KeyCode.E);
    public static Vector2 DeadZone(Vector2 value)
    {
        float length = value.magnitude;
        return length <= .18f ? Vector2.zero : value.normalized * Mathf.Clamp01((length - .18f) / .82f);
    }
}
