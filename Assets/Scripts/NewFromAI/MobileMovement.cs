
using UnityEngine;

public class MobileMovement : MonoBehaviour
{
    public MobileJoystick joystick;
    public float speed = 3f;

    CharacterController controller;


    void Start()
    {

        controller = GetComponent<CharacterController>();

        Debug.Log("Joystick = " + joystick);
        Debug.Log("Controller = " + controller);



        controller = GetComponent<CharacterController>();


    }

    void Update()
    {
        Vector3 move =
            transform.right * joystick.Horizontal +
            transform.forward * joystick.Vertical;

        controller.Move(move * speed * Time.deltaTime);

    }
}