using UnityEngine;

public class MobileMovement : MonoBehaviour
{
    public MobileJoystick joystick;

    public float speed = 3f;
    public float acceleration = 5f;

    [Header("Joystick tuning")]
    public float joystickPower = 0.7f;     // ограничение силы
    public float responseCurve = 2f;       // прогрессивность

    CharacterController controller;
    Vector3 currentVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = joystick.Horizontal * joystickPower;
        float y = joystick.Vertical * joystickPower;

        // прогрессивная чувствительность
        x = Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), responseCurve);
        y = Mathf.Sign(y) * Mathf.Pow(Mathf.Abs(y), responseCurve);

        Vector3 targetMove =
            transform.right * x +
            transform.forward * y;

        targetMove = Vector3.ClampMagnitude(targetMove, 1f);
        targetMove *= speed;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetMove,
            acceleration * Time.deltaTime
        );

        controller.Move(currentVelocity * Time.deltaTime);
    }
}





//using UnityEngine;

//public class MobileMovement : MonoBehaviour
//{
//    public MobileJoystick joystick;
//    public float speed = 3f;

//    CharacterController controller;


//    void Start()
//    {

//        controller = GetComponent<CharacterController>();

//        Debug.Log("Joystick = " + joystick);
//        Debug.Log("Controller = " + controller);



//        controller = GetComponent<CharacterController>();


//    }

//    void Update()
//    {
//        Vector3 move =
//            transform.right * joystick.Horizontal +
//            transform.forward * joystick.Vertical;

//        controller.Move(move * speed * Time.deltaTime);

//    }
//}