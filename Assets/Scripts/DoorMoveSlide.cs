using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


public class DoorMoveSlide : MonoBehaviour
{
    // Получение доступа до системы управления из главного скрипта
    private PlayerController playerController;
    private int lastFrameLeft = -1;

    public enum open_type_ENUM { rot_to_open, move_to_open} // тип открывание
    public open_type_ENUM open_type;
    public enum door_axis_ENUM { X,Y,Z} //  ось вращения
    public door_axis_ENUM door_axis;
    public bool only_open; // если можно только открыть
    public bool can_be_opened_now; // можно ли сейчас открыть?
    public bool is_open; // признак открытой двери
    public float open_speed = 150f; // скорость открывания или для перемещения = 2
    public float open_dist_or_angle = 140f; // Угол открывания или перемещение = 1
    public enum handle_axis_ENUM { X,Y,Z}
    public handle_axis_ENUM handle_axis;
    public float handle_rot_angle = -45f;

    private Quaternion handle_start_rot;
    private float start_dist_or_angle; // начальный угол или позиция
    private bool open_close_ON; //открывается или закрывается в данный момент (что бы постоянно в апдёте не было)


    [Header("Objects")]
    [SerializeField] private GameObject interaction_image;
    [SerializeField] private GameObject door_handle;
 

    [Header("Audio Setting")]
    [SerializeField] private AudioSource move_or_rot_sound;
    [SerializeField] private AudioSource open_sound;
    [SerializeField] private AudioSource close_sound;
    [SerializeField] private AudioSource not_open_sound;






// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
    {


       // Debug.Log(playerController.Input.PlayerActionControl.PressLeftButton);
        playerController = FindFirstObjectByType<PlayerController>(); // находим систему управления







        //Debug.Log("Дверь начало");
        if (open_type == open_type_ENUM.move_to_open)
        {
            if (door_axis == door_axis_ENUM.X) start_dist_or_angle = transform.localPosition.x;
            else if (door_axis == door_axis_ENUM.Y) start_dist_or_angle = transform.localPosition.y;
            else if (door_axis == door_axis_ENUM.Z) start_dist_or_angle = transform.localPosition.z;
        }
        else
        {
            if (door_axis == door_axis_ENUM.X) start_dist_or_angle = transform.localEulerAngles.x;
            else if (door_axis == door_axis_ENUM.Y) start_dist_or_angle = transform.localEulerAngles.y;
            else if (door_axis == door_axis_ENUM.Z) start_dist_or_angle = transform.localEulerAngles.z;
        }
        if (door_handle) handle_start_rot = door_handle.transform.localRotation;
    }

    void OnMouseEnter()
    {
        Debug.Log("Есть наведение");
        if (gameObject.tag == "Door") interaction_image.SetActive(true);

    }

    void OnMouseExit()
    {
        Debug.Log("Есть уход");
        if (gameObject.tag == "Door") interaction_image.SetActive(false);
    }

    void OnMouseDown()
    {

        if (playerController.Input.PlayerActionControl.PressLeftButton.WasPressedThisFrame()
    && lastFrameLeft != Time.frameCount)
        {
            lastFrameLeft = Time.frameCount;






            if (gameObject.tag == "Door")
            {
                Debug.Log(" Объект Door");
                if (door_handle)
                {
                    if (handle_axis == handle_axis_ENUM.X) door_handle.transform.localRotation = Quaternion.Euler(handle_rot_angle, 0f, 0f);
                    else if (handle_axis == handle_axis_ENUM.Y) door_handle.transform.localRotation = Quaternion.Euler(0f, handle_rot_angle, 0f);
                    else if (handle_axis == handle_axis_ENUM.Z) door_handle.transform.localRotation = Quaternion.Euler(0f, 0f, handle_rot_angle);
                }
                Open_close();
            }
        }
    }

    void OnMouseUp()
    {
        if (door_handle) door_handle.transform.localRotation = handle_start_rot;
    }

    public void Open_close ()
    {
        if (can_be_opened_now)
        {
            if (move_or_rot_sound) move_or_rot_sound.Play();
            open_close_ON = true;
            if (is_open) is_open = false;
            else
            {
                is_open = true;
                if (open_sound) open_sound.Play();
                if (only_open)
                {
                    gameObject.tag = "Untagged"; // что бы дверь была больее не интеррактивная
                    interaction_image.SetActive(false);
                }
            }
        }
        else 
        {
            if (not_open_sound) not_open_sound.Play();
            Debug.Log("ЗАкрыто");
        }
    }

    void Stop_open_close()
    {
        open_close_ON = false;
        if (move_or_rot_sound) move_or_rot_sound.Stop();
        if(close_sound && !is_open) close_sound.Play();
    }

    // Update is called once per frame
    void Update()
    {

        if (Keyboard.current.eKey.wasPressedThisFrame) { CloseDoorSlideRotation(); Debug.Log("EEEEE"); }

        if (Keyboard.current.cKey.wasPressedThisFrame) { OpenDoorSlideRotation(); Debug.Log("CCCCCC"); }
       
        //   Вышеуказанные функции работают.
        


        

        if (playerController.Input.PlayerActionControl.PressLeftButton.WasPressedThisFrame())
        {
            // Debug.Log("Left mouse");
            //CloseDoorSlideRotation();

        }






        //if (open_close_ON)
        //{
        //    if (is_open) // Открываем двеоь  признак открытой двери
        //    {
        //        if (open_type == open_type_ENUM.move_to_open) // движение
        //        {
        //            // ---------------------------------------------------------------------------------------
        //            //Debug.Log("Начало сдвига");  // не работает !!!!!!!!

        //            if (door_axis == door_axis_ENUM.X)
        //            {
        //                float posX = Mathf.MoveTowards(transform.localPosition.x, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localPosition = new Vector3(posX, transform.localPosition.y, transform.localPosition.z);
        //                if (transform.localPosition.x == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
        //            }
        //            else if (door_axis == door_axis_ENUM.Y)
        //            {
        //                float posY = Mathf.MoveTowards(transform.localPosition.y, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localPosition = new Vector3(transform.localPosition.x, posY, transform.localPosition.z);
        //                if (transform.localPosition.y == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
        //            }
        //            else if (door_axis == door_axis_ENUM.Z)
        //            {
        //                float posZ = Mathf.MoveTowards(transform.localPosition.z, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, posZ);
        //                if (transform.localPosition.z == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
        //            }
        //        }
        //        else  // врщение
        //        {
        //            if (door_axis == door_axis_ENUM.X)
        //            {
        //                float angleX = Mathf.MoveTowardsAngle(transform.localEulerAngles.x, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localEulerAngles = new Vector3(angleX, 0, 0);
        //                if (transform.localEulerAngles.x == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
        //            }
        //            else if (door_axis == door_axis_ENUM.Y)
        //            {
        //                float angleY = Mathf.MoveTowardsAngle(transform.localEulerAngles.y, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localEulerAngles = new Vector3(0, angleY, 0);
        //                if (transform.localEulerAngles.y == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
        //            }
        //            else if (door_axis == door_axis_ENUM.Z)
        //            {
        //                float angleZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localEulerAngles = new Vector3(0, 0, angleZ);
        //                if (transform.localEulerAngles.z == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
        //            }
                    
        //        }
        //    }
        //    else  // Закрываем дверь  -----------------------   isOpen
        //    {
        //        if (open_type == open_type_ENUM.move_to_open) // движение
        //        {
        //            if (door_axis == door_axis_ENUM.X)
        //            {
        //                float posX = Mathf.MoveTowards(transform.localPosition.x,start_dist_or_angle,open_speed * Time.deltaTime);
        //                transform.localPosition = new Vector3(posX, transform.localPosition.y, transform.localPosition.z);
        //                if (transform.localPosition.x == start_dist_or_angle) Stop_open_close();
        //            }
        //            else if (door_axis == door_axis_ENUM.Y)
        //            {
        //                float posY = Mathf.MoveTowards(transform.localPosition.y, start_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localPosition = new Vector3(transform.localPosition.x, posY, transform.localPosition.z);
        //                if (transform.localPosition.y == start_dist_or_angle) Stop_open_close();

        //            }
        //            else if (door_axis == door_axis_ENUM.Z)
        //            {
        //                float posZ = Mathf.MoveTowards(transform.localPosition.z, start_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, posZ    );
        //                if (transform.localPosition.z == start_dist_or_angle) Stop_open_close();

        //            }
        //        }
        //        else  // вращение
        //        {
        //            if (door_axis == door_axis_ENUM.X)
        //            {
        //                float angleX = Mathf.MoveTowardsAngle(transform.localEulerAngles.x, start_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localEulerAngles = new Vector3(angleX, 0, 0);
        //                if (transform.localEulerAngles.x == start_dist_or_angle) Stop_open_close();
        //            }
        //            else if (door_axis == door_axis_ENUM.Y)
        //            {
        //            float angleY = Mathf.MoveTowardsAngle(transform.localEulerAngles.y, start_dist_or_angle, open_speed * Time.deltaTime);
        //            transform.localEulerAngles = new Vector3(0, angleY, 0);
        //            if (transform.localEulerAngles.y == start_dist_or_angle) Stop_open_close();

        //            }
        //            else if (door_axis == door_axis_ENUM.Z)
        //            {
        //                float angleZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, start_dist_or_angle, open_speed * Time.deltaTime);
        //                transform.localEulerAngles = new Vector3(0, 0, angleZ);
        //                if (transform.localEulerAngles.z == start_dist_or_angle) Stop_open_close();

        //            }
        //        }
        //    }
        //}
    }


    //  Переделка скрипта  -----------------------------------------------------------------------

    private void OpenDoorSlideRotation()
    {
        
        if (open_type == open_type_ENUM.move_to_open) // движение
        {
            // ---------------------------------------------------------------------------------------
            //Debug.Log("Работает OpenCloseDoorSlide");  

            if (door_axis == door_axis_ENUM.X) // Open Speed = 2 dist 2
            {
                //Debug.Log("По оси Х");
                float posX = Mathf.MoveTowards(transform.localPosition.x, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
                transform.localPosition = new Vector3(posX, transform.localPosition.y, transform.localPosition.z);
                if (transform.localPosition.x == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
            }
            else if (door_axis == door_axis_ENUM.Y)
            {
                float posY = Mathf.MoveTowards(transform.localPosition.y, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
                transform.localPosition = new Vector3(transform.localPosition.x, posY, transform.localPosition.z);
                if (transform.localPosition.y == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
            }
            else if (door_axis == door_axis_ENUM.Z)
            {
                float posZ = Mathf.MoveTowards(transform.localPosition.z, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, posZ);
                if (transform.localPosition.z == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
            }
        }
        else  // вращение
        {
            Debug.Log("Вращение");   // Open Speed = 150  angle 100
            if (door_axis == door_axis_ENUM.X)
            {
                float angleX = Mathf.MoveTowardsAngle(transform.localEulerAngles.x, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
                transform.localEulerAngles = new Vector3(angleX, 0, 0);
                if (transform.localEulerAngles.x == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
            }
            else if (door_axis == door_axis_ENUM.Y)
            {
                float angleY = Mathf.MoveTowardsAngle(transform.localEulerAngles.y, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
                transform.localEulerAngles = new Vector3(0, angleY, 0);
                if (transform.localEulerAngles.y == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
            }
            else if (door_axis == door_axis_ENUM.Z)
            {
                float angleZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, start_dist_or_angle + open_dist_or_angle, open_speed * Time.deltaTime);
                transform.localEulerAngles = new Vector3(0, 0, angleZ);
                if (transform.localEulerAngles.z == start_dist_or_angle + open_dist_or_angle) Stop_open_close();
            }

        }
    }
    private void CloseDoorSlideRotation()
    {

        {
            if (open_type == open_type_ENUM.move_to_open) // движение
            {
                if (door_axis == door_axis_ENUM.X)
                {
                    float posX = Mathf.MoveTowards(transform.localPosition.x, start_dist_or_angle, open_speed * Time.deltaTime);
                    transform.localPosition = new Vector3(posX, transform.localPosition.y, transform.localPosition.z);
                    if (transform.localPosition.x == start_dist_or_angle) Stop_open_close();
                }
                else if (door_axis == door_axis_ENUM.Y)
                {
                    float posY = Mathf.MoveTowards(transform.localPosition.y, start_dist_or_angle, open_speed * Time.deltaTime);
                    transform.localPosition = new Vector3(transform.localPosition.x, posY, transform.localPosition.z);
                    if (transform.localPosition.y == start_dist_or_angle) Stop_open_close();

                }
                else if (door_axis == door_axis_ENUM.Z)
                {
                    float posZ = Mathf.MoveTowards(transform.localPosition.z, start_dist_or_angle, open_speed * Time.deltaTime);
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, posZ);
                    if (transform.localPosition.z == start_dist_or_angle) Stop_open_close();

                }
            }
            else  // вращение
            {
                if (door_axis == door_axis_ENUM.X)
                {
                    float angleX = Mathf.MoveTowardsAngle(transform.localEulerAngles.x, start_dist_or_angle, open_speed * Time.deltaTime);
                    transform.localEulerAngles = new Vector3(angleX, 0, 0);
                    if (transform.localEulerAngles.x == start_dist_or_angle) Stop_open_close();
                }
                else if (door_axis == door_axis_ENUM.Y)
                {
                    float angleY = Mathf.MoveTowardsAngle(transform.localEulerAngles.y, start_dist_or_angle, open_speed * Time.deltaTime);
                    transform.localEulerAngles = new Vector3(0, angleY, 0);
                    if (transform.localEulerAngles.y == start_dist_or_angle) Stop_open_close();

                }
                else if (door_axis == door_axis_ENUM.Z)
                {
                    float angleZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, start_dist_or_angle, open_speed * Time.deltaTime);
                    transform.localEulerAngles = new Vector3(0, 0, angleZ);
                    if (transform.localEulerAngles.z == start_dist_or_angle) Stop_open_close();

                }
            }
        }
    }
}
