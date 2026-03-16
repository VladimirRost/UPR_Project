using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public MobileJoystick joystick;
    public float speed = 3f;

    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 move =
            transform.forward * joystick.Vertical +
            transform.right * joystick.Horizontal;

        controller.Move(move * speed * Time.deltaTime);
    }
}