using UnityEngine;

public class DragMouseOrbit : MonoBehaviour
{
    public Transform Target;
    public float Distance = 5f;
    public float XSpeed = 10f, YSpeed = 10f;
    public float YMinLimit = -12f, YMaxLimit = 70f;
    public float DistanceMin = 2f, DistanceMax = 12f;
    public float SmoothTime = 6f;
    float yaw, pitch, velocityX, velocityY, actualDistance;
    Camera view;
    BbRigidbodyController controller;
    void Start()
    {
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
        actualDistance = Distance;
        view = GetComponent<Camera>();
        controller = Target.GetComponent<BbRigidbodyController>();
    }
    void LateUpdate()
    {
        if (!Target) return;
        velocityX += XSpeed * Input.GetAxis("Mouse X") * 0.02f;
        velocityY += YSpeed * Input.GetAxis("Mouse Y") * 0.02f;
        yaw += velocityX;
        pitch = Mathf.Clamp(pitch - velocityY, YMinLimit, YMaxLimit);
        Distance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel") * 5f, DistanceMin, DistanceMax);
        var rotation = Quaternion.Euler(pitch, yaw, 0);
        var focus = Target.position + Vector3.up * 0.48f;
        var direction = rotation * Vector3.back;
        float desired = Distance;
        // The droide uses layer 8. Ignore its own colliders and the energy triggers.
        if (Physics.SphereCast(focus, 0.22f, direction, out var hit, Distance, ~(1 << 8), QueryTriggerInteraction.Ignore))
            desired = Mathf.Max(0.3f, hit.distance - 0.1f);
        actualDistance = desired < actualDistance ? desired : Mathf.Lerp(actualDistance, desired, 1f - Mathf.Exp(-8f * Time.deltaTime));
        transform.SetPositionAndRotation(focus + direction * actualDistance, rotation);
        velocityX = Mathf.Lerp(velocityX, 0f, Time.deltaTime * SmoothTime);
        velocityY = Mathf.Lerp(velocityY, 0f, Time.deltaTime * SmoothTime);
        if (view != null) view.fieldOfView = Mathf.Lerp(view.fieldOfView, controller != null && controller.IsBoosting ? 64f : 59f, 1f - Mathf.Exp(-5f * Time.deltaTime));
    }
}
