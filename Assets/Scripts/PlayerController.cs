using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    public bool IsFlyMode => _flyMode;

    [Header("Objects")]
    [SerializeField] private CharacterController _Character_controller;
    //[SerializeField] private Transform _Camera_transfomm;
    [SerializeField] private Transform _Check_ground; // проверка касания земли 
    [SerializeField] private LayerMask _Ground_mask;
    public GameObject PanelChangeLight; // Панель с режимами освещения
    public GameObject PanelStartWindow; // Стартовая панель с инструкиями
    public GameObject PanelExitWindow; // Выходная панель с инструкиями
    public Button ButtonSwitchFly;  //  Кнопка смены режима полёт / ходьба
    public string textButtonFlyON = "ПОЛЁТ";
    public string textButtonFlyOFF = "ХОДЬБА";
    private Text textComponentButton; // компонент доступка до текста кнопки
    public Button ButtonExit; //Кнопка выхода
    public Button ButtonSky; // Кнопка неба
    public Button ButtonSunOnly; // Кнопка солнца
    public Button ButtonNone; // Кнопка нейтрали
    public Button ButtonDark; // Кнопка темноты
    
    public string webAddress = "https://veter64.ru/Visualization.html";
    public GameObject SunDiraction; // управление солнцем
    public Scrollbar ScrollRectSunPosition; // Объект прокрутки
    // Добавляем управление с джойстика
    public MobileJoystick mobileJoystick;
    [SerializeField] private GameObject MobileWindow;

    [Header("Settings")]
    [SerializeField] private float _check_radius_sphere = 0.2f;
    [SerializeField] private float _gravity = -14f;
    [SerializeField] public float minPitch = -89f;
    [SerializeField] public float maxPitch = 89f;
    [SerializeField] private float _carrentPositionSun; //Исходная позиция солнца (0 - 210) утро - день - вечер - ночь

    [Header("Movement smoothing")]
    [SerializeField] private float _acceleration = 6f;
    [SerializeField] private float _deceleration = 10f;
    Vector3 _currentVelocity;
    [SerializeField] private float _speed_walk = 4f;
    [SerializeField] private float _speed_run = 7f;
    [SerializeField] private float _jump_height = 1f;

    [Header("Look smoothing")]
    [SerializeField] private float _lookSmooth = 12f;

    private float _targetYaw;
    private float _targetPitch;

    [Range(1f, 100f)]
    [SerializeField] private float _sensitivity_mouse;

    
    // --------------------------------------------------------------------------------------------------------------
    
    [Header("Day/Night System")]

    // Skybox
    [SerializeField] private float _skyMinExposure = 0.2f;
    [SerializeField] private float _skyMaxExposure = 1.3f;
    [SerializeField] private Material _proceduralSkyboxMaterial; // Обычное пустое небо
    [SerializeField] private Material _nightSkyboxMaterial; // Звёздное небо

    // Ambient
    [SerializeField] private float _ambientMin = 0.1f;
    [SerializeField] private float _ambientMax = 1f;

    // Sun Light
    [SerializeField] private Light _sunLight;
    [SerializeField] private float _sunMinIntensity = 0f;
    [SerializeField] private float _sunMaxIntensity = 1.2f;

    // Цвет солнца
    [SerializeField] private Gradient _sunColor;

    [SerializeField] private GameObject SunVisual;

    public enum BackgroundMode
    {
        Skybox,
        SunOnly,
        Neutral,
        Dark
    }

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Color _neutralColor = new Color(0.5f, 0.5f, 0.5f);


    private BackgroundMode _currentMode = BackgroundMode.Skybox; //Режим skyBox по умолчанию
    private Material _originalSkybox; // Сохранение skybox


    [SerializeField] private Text _buttonSkyText;
    [SerializeField] private Text _buttonNeutralText;
    [SerializeField] private Text _buttonDarkText;
    [SerializeField] private Text _buttonSunOnlyText;








    bool _start_button;  //Старт визуализации

    bool _flyMode;
    float temp;
    bool isGrounded;
    Vector3 velosity; // Сила гравмиации
    Vector3 move; // векирп перемещения


    private float yaw;
    private float pitch;



    private PlayerClassControl input;


    // Открытие управления для дпугих скриптов   ---------------------------------
    public PlayerClassControl Input => input;
    //    ---------------------------------








    private void Awake()
    {
        input = new PlayerClassControl();
        input.Enable();
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

        float deltaX = mouseDelta.x;
        float deltaY = mouseDelta.y;

        _targetYaw += deltaX * _sensitivity_mouse * Time.deltaTime;
        _targetPitch -= deltaY * _sensitivity_mouse * Time.deltaTime;
        _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);

        yaw = Mathf.Lerp(yaw, _targetYaw, _lookSmooth * Time.deltaTime);
        pitch = Mathf.Lerp(pitch, _targetPitch, _lookSmooth * Time.deltaTime);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);


        //Vector2 mouseDelta = input.PlayerActionControl.Look.ReadValue<Vector2>();

        //float deltaX = mouseDelta.x;
        //float deltaY = mouseDelta.y;

        //yaw += deltaX * _sensitivity_mouse * Time.deltaTime;
        //pitch -= deltaY * _sensitivity_mouse * Time.deltaTime;
        //pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        //transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // Перемещение игрока
    private void Move()
    {

        Vector2 movekey;
        
        if (mobileJoystick != null && MobileWindow.activeInHierarchy)  // Если мобильная версия - джойстик не работает
        {
           
            movekey = new Vector2(mobileJoystick.Horizontal, mobileJoystick.Vertical);
        }
        else
        {
            movekey = input.PlayerActionControl.Move.ReadValue<Vector2>();
        }

        float temMoveX = movekey.x; // +1/-1   влево в право
        float temMoveY = movekey.y;// +1/-1   вперёд назад
  
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        move = forward * temMoveY + right * temMoveX;
        if (!_flyMode)
        {
            move.y = 0;
        }
        move.Normalize();

        //  Блок плавного движения Начало
        float targetSpeed;

        if (input.PlayerActionControl.Boost.IsPressed() && (temMoveX != 0 || temMoveY != 0))
            targetSpeed = _speed_run;
        else
            targetSpeed = _speed_walk;

        Vector3 targetVelocity = move * targetSpeed;

        // плавное ускорение
        _currentVelocity = Vector3.Lerp(
            _currentVelocity,
            targetVelocity,
            _acceleration * Time.deltaTime
        );

        _Character_controller.Move(_currentVelocity * Time.deltaTime);
        
        //  Блок плавного движения Конец

    }

    private void Velocity()  //  Физика
    {
        isGrounded = Physics.CheckSphere(_Check_ground.transform.position, _check_radius_sphere, _Ground_mask); // Создаём сферу невидимую с радиусом и если она соприкачается с землёй - true
        if (isGrounded && velosity.y < 0)
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

        Debug.Log("Полёт");
        _flyMode = true;
        _gravity = 0;
    }
    private void FlyModeOff()
    {
        Debug.Log("Ходьба");
        _flyMode = false;
        _gravity = temp;
    }

    public void Switch_FlyMode() // Стартует от кнопки
    {

        if (_flyMode == false)
        {
            FlyModeON();
            //textComponentButton = ButtonSwitchFly.GetComponentInChildren<Text>();
            textComponentButton.text = textButtonFlyOFF;
        }

        else if (_flyMode == true)
        {
            FlyModeOff();
            //textComponentButton = ButtonSwitchFly.GetComponentInChildren<Text>();
            textComponentButton.text = textButtonFlyON;
        }

    }

    public void StartGame() //нажата кнопа Начать
    {
        _start_button = true;
        PanelStartWindow.SetActive(false); // убираем панель старта
        PanelChangeLight.SetActive(true);

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

    public void F_SunPosistion()
    {

        float t = ScrollRectSunPosition.value;

        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // вращение солнца
        float angleX = Mathf.Lerp(0f, 210f, t);
        SunDiraction.transform.rotation = Quaternion.Euler(angleX, 0f, 0f);

        // высота солнца
        float sunHeight = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));

        // солнце всегда работает
        _sunLight.intensity = Mathf.Lerp(_sunMinIntensity, _sunMaxIntensity, sunHeight);
        _sunLight.color = _sunColor.Evaluate(t);

        Vector3 dir = SunDiraction.transform.forward;
        SunVisual.transform.position = -dir * 100f; // расстояние подбери (например 50–500)
        SunVisual.transform.position = _mainCamera.transform.position - dir * 100f;
        SunVisual.transform.rotation = SunDiraction.transform.rotation;

        // 🔥 РАЗДЕЛЕНИЕ РЕЖИМОВ
        switch (_currentMode)
        {
            case BackgroundMode.Skybox:
                if (RenderSettings.skybox != null)
                {
                    RenderSettings.skybox.SetFloat("_Exposure",
                        Mathf.Lerp(_skyMinExposure, _skyMaxExposure, sunHeight));
                }

                RenderSettings.ambientIntensity =
                    Mathf.Lerp(_ambientMin, _ambientMax, sunHeight);

                DynamicGI.UpdateEnvironment();
                break;

            case BackgroundMode.SunOnly:
                _proceduralSkyboxMaterial.SetFloat("_AtmosphereThickness",
                Mathf.Lerp(1.5f, 0.8f, sunHeight));
                RenderSettings.ambientIntensity = 0.6f;



                break;

            case BackgroundMode.Neutral:
                RenderSettings.ambientIntensity = 0.7f;
                break;

            case BackgroundMode.Dark:
                _nightSkyboxMaterial.SetFloat("_AtmosphereThickness",
                Mathf.Lerp(1.5f, 0.8f, sunHeight));
                RenderSettings.ambientIntensity = 0.6f;
                break;


        }
    }

    public void SetBackgroundMode(int modeIndex)  // Переключение режимов освещения
    {
        _currentMode = (BackgroundMode)modeIndex;

        switch (_currentMode)
        {
            case BackgroundMode.Skybox:
                _mainCamera.clearFlags = CameraClearFlags.Skybox;
                // ВОССТАНАВЛИВАЕМ skybox
                RenderSettings.skybox = _originalSkybox;
                RenderSettings.fog = false;
                SunVisual.SetActive(false);   // 👈 выключаем диск солнца
                SunDiraction.SetActive(true);
                Debug.Log("Режим SkyBox");
                break;

            case BackgroundMode.SunOnly:

                _mainCamera.clearFlags = CameraClearFlags.Skybox;

                RenderSettings.skybox = _proceduralSkyboxMaterial;
                RenderSettings.fog = true;
                RenderSettings.fogColor = new Color(0.75f, 0.8f, 0.9f);
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = 0.002f;

                SunVisual.SetActive(true);
                SunDiraction.SetActive(true);
                Debug.Log("Режим SunOnly");
                break;

            case BackgroundMode.Neutral:
                _mainCamera.clearFlags = CameraClearFlags.SolidColor;
                _mainCamera.backgroundColor = _neutralColor;
                SunVisual.SetActive(false);   // 👈 выключаем диск солнца
                Debug.Log("Режим None");
                break;

            case BackgroundMode.Dark:
                _mainCamera.clearFlags = CameraClearFlags.Skybox;
                RenderSettings.skybox = _nightSkyboxMaterial;

                //_mainCamera.clearFlags = CameraClearFlags.SolidColor;
                //_mainCamera.backgroundColor = _darkColor;
                SunVisual.SetActive(false);   // 👈 выключаем диск солнца
                SunDiraction.SetActive(false);
                Debug.Log("Режим Dark");

                break;


        }
        DynamicGI.UpdateEnvironment();
        UpdateButtons(); // Обновляем свечения кнопок
    }

    private void UpdateButtons() // изменение цвета кнопок
    {
        _buttonSkyText.color = (_currentMode == BackgroundMode.Skybox) ? Color.white : Color.gray;
        _buttonNeutralText.color = (_currentMode == BackgroundMode.Neutral) ? Color.white : Color.gray;
        _buttonDarkText.color = (_currentMode == BackgroundMode.Dark) ? Color.white : Color.gray;
        _buttonSunOnlyText.color = (_currentMode == BackgroundMode.SunOnly) ? Color.white : Color.gray;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        SunVisual.SetActive(false);
        Vector3 _temSun = SunVisual.transform.position;
        _temSun.y = SunDiraction.transform.position.y * 10f; // размещение солнца
        SunVisual.transform.position = _temSun;


        _originalSkybox = RenderSettings.skybox;  // Сохраняем skybox



        UpdateButtons();

        //Debug.Log(MobileWindow.activeInHierarchy);

        // установка названия кнопки ХОДЬБА
        textComponentButton = ButtonSwitchFly.GetComponentInChildren<Text>();


        PanelExitWindow.SetActive(false); // Убираем панель выхода
        PanelChangeLight.SetActive(false); //Убираем панел с режимами освещения



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