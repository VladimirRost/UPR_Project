using UnityEngine;

public class TouchLook : MonoBehaviour
{
    public float sensitivity = 0.2f;

    float rotX;
    float rotY;

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            rotX += t.deltaPosition.x * sensitivity;
            rotY -= t.deltaPosition.y * sensitivity;

            transform.localRotation =
                Quaternion.Euler(rotY, rotX, 0);
        }
    }
}
