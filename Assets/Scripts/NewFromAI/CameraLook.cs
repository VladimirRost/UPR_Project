using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform cameraPivot;

    public float sensitivity = 120f;
    public float smooth = 10f;

    float yaw;
    float pitch;

    Vector2 currentLook;
    Vector2 targetLook;

    void Start()
    {
        yaw = cameraPivot.transform.eulerAngles.y;
    }

    public void Look(Vector2 delta)
    {
        targetLook += delta * sensitivity * Time.deltaTime;
    }

    void Update()
    {
        currentLook = Vector2.Lerp(
            currentLook,
            targetLook,
            smooth * Time.deltaTime
        );

        yaw += currentLook.x;
        pitch -= currentLook.y;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        cameraPivot.localRotation =
        Quaternion.Euler(pitch, yaw, 0f);
        targetLook = Vector2.zero;
    }
}




//using UnityEngine;

//public class CameraLook : MonoBehaviour
//{
//    public Transform player;

//    [Header("Sensitivity")]
//    public float sensitivity = 0.15f;

//    [Header("Camera inertia")]
//    public float smoothTime = 0.05f;

//    float xRotation = 0f;

//    Vector2 currentVelocity;
//    Vector2 currentDelta;

//    public void Look(Vector2 inputDelta)
//    {
//        // сглаживание движения
//        currentDelta = Vector2.SmoothDamp(
//            currentDelta,
//            inputDelta,
//            ref currentVelocity,
//            smoothTime
//        );

//        float mouseX = currentDelta.x * sensitivity * 100f * Time.deltaTime;
//        float mouseY = currentDelta.y * sensitivity * 100f * Time.deltaTime;

//        xRotation -= mouseY;
//        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

//        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

//        player.Rotate(Vector3.up * mouseX);
//    }
//}
