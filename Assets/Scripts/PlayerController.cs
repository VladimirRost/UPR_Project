using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.Animations;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private CharacterController _Character_controller;
    //[SerializeField] private Transform _Camera_transfomm;
    [SerializeField] private Transform _Check_ground; // проверка касания земли 
    [SerializeField] private LayerMask _Ground_mask;

    [Header("Settings")]
    [SerializeField] private float _check_radius_sphere = 0.2f;
    [SerializeField] private float _gravity = -14f;
    [SerializeField] private float _speed_walk = 4f;
    [SerializeField] private float _speed_run = 7f;
    [SerializeField] private float _jump_height = 1f;
    [SerializeField] public float minPitch = -89f;
    [SerializeField] public float maxPitch = 89f;
    [SerializeField] private float _carrentPositionSun; //Исходная позиция солнца (0 - 210) утро - день - вечер - ночь

    [Range(1f, 100f)]
    [SerializeField] private float _sensitivity_mouse;



    public GameObject PanelStartWindow; // Стартовая панель с инструкиями
    public GameObject PanelExitWindow; // Выходная панель с инструкиями
    public Button ButtonSwitchFly;  //  Кнопка смены режима полёт / ходьба
    public string textButtonFlyON = "ПОЛЁТ";
    public string textButtonFlyOFF = "ХОДЬБА";
    private Text textComponentButton; // компонент доступка до текста кнопки

    public Button ButtonExit; //Кнопка выхода
    public string webAddress = "https://veter64.ru/Visualization.html";

    public GameObject SunDiraction; // управление солнцем
    public Scrollbar ScrollRectSunPosition; // Объект прокрутки




    bool _start_button;  //Старт визуализации

    bool _flyMode;
    float temp;
    bool isGrounded;
    Vector3 velosity; // Сила гравмиации
    Vector3 move; // векирп перемещения


    private float yaw;
    private float pitch;

    private PlayerClassControl input;

    private void Awake()
    {
       input = new PlayerClassControl();

    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }




    // Поворот игрока с камерой
    private void Rotate()
    {
            Vector2 mouseDelta = input.PlayerActionControl.Look.ReadValue<Vector2>();

        //float deltaX = mouseDelta.x / Screen.width*10000f;
        //float deltaY = mouseDelta.y / Screen.height*10000f;

        float deltaX = mouseDelta.x;
        float deltaY = mouseDelta.y;

        yaw += deltaX * _sensitivity_mouse * Time.deltaTime;
            pitch -= deltaY * _sensitivity_mouse * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // Перемещение игрока
    private void Move()
    {
        Vector2 movekey = input.PlayerActionControl.Move.ReadValue<Vector2>();
        float temMoveX = movekey.x; // +1/-1   влево в право
        float temMoveY = movekey.y;// +1/-1   вперёд назад
                                //    Debug.Log("MoveX " + temMoveX);
                                //    Debug.Log("MoveY " + temMoveY);
        move = transform.forward * temMoveY + transform.right * temMoveX;

        if (input.PlayerActionControl.Boost.IsPressed() && (temMoveX!=0 || temMoveY!=0)) //проверка на ускорение
        {
            _Character_controller.Move(move * _speed_run * Time.deltaTime);
           
        }
        else
        {
            _Character_controller.Move(move * +_speed_walk * Time.deltaTime);
          
        }


    }

    private void Velocity()  //  Физика
    {
        isGrounded = Physics.CheckSphere(_Check_ground.transform.position, _check_radius_sphere, _Ground_mask); // Создаём сферу невидимую с радиусом и если она соприкачается с землёй - true
        if (isGrounded && velosity.y <0)
        {
            velosity.y = -2f;
        }
        velosity.y += Time.deltaTime * _gravity;
        if (input.PlayerActionControl.Jump.IsPressed() && isGrounded)
        {
            velosity.y = Mathf.Sqrt(_jump_height * -2f * _gravity);
        }
        _Character_controller.Move(velosity * Time.deltaTime);
    }

    private void FlyModeON()   
    {
        _flyMode = true;
        _gravity = 0;
    }
    private void FlyModeOff ()
    {
        _flyMode = false;
        _gravity = temp;
    }

    public void Switch_FlyMode() // Стартует от кнопки
    {

            if (_flyMode == false)
            {
                FlyModeON();
            textComponentButton = ButtonSwitchFly.GetComponentInChildren<Text>();
            textComponentButton.text = textButtonFlyOFF;
        }

            else if (_flyMode == true)
            {
                FlyModeOff();
            textComponentButton = ButtonSwitchFly.GetComponentInChildren<Text>();
            textComponentButton.text = textButtonFlyON;
        }

    }
  
    public void StartGame() //нажата кнопа Начать
    {
        _start_button = true;
        PanelStartWindow.SetActive(false); // убираем панель старта

    }

    public void ButtonExitGame()  //  Выход на страницу сайта
    {
        //Application.OpenURL(webAddress);
        Application.ExternalEval("window.open('" + webAddress + "','_self')");
        Application.Quit();
    }

    public void ButtonCancelGame()  //  Возврат в игру
    {
        _start_button = true;
        PanelExitWindow.SetActive(false);
    }
    public void ExitGame()  // Главная кнопка выхода на главной панели
    {
        _start_button = false;
        PanelExitWindow.SetActive(true);
      
    }

    public void F_SunPosistion() // Управление движением солнца
    {
        // Получаем текущее значение Slider'а (от 0 до 1)
        float normalizedValue = ScrollRectSunPosition.value;

        // Преобразуем нормализованное значение в угол вращения (пример: диапазон от -90° до 90° градусов)
        float angleX = Mathf.Lerp(0f, 210f, normalizedValue);

        // Преобразование в мировые координаты
        Quaternion rot = Quaternion.Euler(new Vector3(angleX, 0f, 0f));
        SunDiraction.transform.rotation = rot;

       
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // установка названия кнопки ХОДЬБА

        PanelExitWindow.SetActive(false); // Убираем панель выхода

        textComponentButton = ButtonSwitchFly.GetComponentInChildren<Text>(); // Считываем компонент текст с кнопки режима
        textComponentButton.text = textButtonFlyON;
        
        _start_button = false;
        _flyMode = false;
        temp = _gravity;
        Vector3 angles = transform.eulerAngles;
        Vector3 position = transform.position;
        transform.position = position;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

    }

    // Update is called once per frame
    void Update()
    {

        if (_start_button)
        {

            if (input.PlayerActionControl.PressMouseButton.ReadValue<float>() == -1)  // если нажата правая кнопка мышки - Обзор и перемещение (+1 - левая кнопка)
            {
                Rotate();
            }
            Move();
            F_SunPosistion(); // Перемещение солнца



            if (!_flyMode) Velocity(); // Физика работает только если нет режима полётов
           


        }




    }
}
