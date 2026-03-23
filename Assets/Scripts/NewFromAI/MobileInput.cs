using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public MobileJoystick joystick;
    public float speed = 3f;
    public float acceleration = 1f;
    private Vector3 currentVelocity;

    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 inputDir =
            transform.forward * joystick.Vertical +
            transform.right * joystick.Horizontal;

        // целевая скорость
        Vector3 targetVelocity = inputDir * speed;

        // плавный разгон
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        controller.Move(currentVelocity * Time.deltaTime);
    }
}