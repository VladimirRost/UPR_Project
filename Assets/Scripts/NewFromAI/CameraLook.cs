using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform player;

    [Header("Sensitivity")]
    public float sensitivity = 0.15f;

    [Header("Camera inertia")]
    public float smoothTime = 0.05f;

    float xRotation = 0f;

    Vector2 currentVelocity;
    Vector2 currentDelta;

    public void Look(Vector2 inputDelta)
    {
        // сглаживание движения
        currentDelta = Vector2.SmoothDamp(
            currentDelta,
            inputDelta,
            ref currentVelocity,
            smoothTime
        );

        float mouseX = currentDelta.x * sensitivity * 100f * Time.deltaTime;
        float mouseY = currentDelta.y * sensitivity * 100f * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        player.Rotate(Vector3.up * mouseX);
    }
}
