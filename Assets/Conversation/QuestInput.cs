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
    public static InputAction LeftPosition, LeftRotation, LeftTracked, RightPosition, RightRotation, RightTracked;
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
        LeftPosition = Action("Left position", "<XRController>{LeftHand}/devicePosition", InputActionType.Value);
        LeftRotation = Action("Left rotation", "<XRController>{LeftHand}/deviceRotation", InputActionType.Value);
        LeftTracked = Action("Left tracked", "<XRController>{LeftHand}/isTracked");
        RightPosition = Action("Right position", "<XRController>{RightHand}/devicePosition", InputActionType.Value);
        RightRotation = Action("Right rotation", "<XRController>{RightHand}/deviceRotation", InputActionType.Value);
        RightTracked = Action("Right tracked", "<XRController>{RightHand}/isTracked");
        actions.Enable();
    }
    static InputAction Action(string name, string binding, InputActionType type = InputActionType.Button) => actions.AddAction(name, type, binding);
    public static Vector2 Movement => DeadZone(Move?.ReadValue<Vector2>() ?? Vector2.zero);
    public static Vector2 Travel => new Vector2(0, Axis(Move?.ReadValue<Vector2>().y ?? 0));
    public static float Yaw => Mathf.Clamp(Axis(Move?.ReadValue<Vector2>().x ?? 0)+Axis(Turn?.ReadValue<Vector2>().x ?? 0),-1,1);
    static float Axis(float value) => Mathf.Sign(value)*Mathf.Clamp01((Mathf.Abs(value)-.22f)/.78f);
    public static bool Talking => Active ? Talk != null && Talk.IsPressed() : Input.GetKey(KeyCode.E);
    public static bool IsTracked(UnityEngine.XR.XRNode node)
    {
        var device=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
        return device.isValid&&device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool tracked)&&tracked;
    }
    public static Vector2 DeadZone(Vector2 value)
    {
        float length = value.magnitude;
        return length <= .18f ? Vector2.zero : value.normalized * Mathf.Clamp01((length - .18f) / .82f);
    }
}
