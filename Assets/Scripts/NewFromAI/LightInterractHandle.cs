using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class LightInterractHandle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Все источники света на сцене")]
    public Light[] lights;

    [Tooltip("Все рендереры (меши лампочек)")]
    public Renderer[] lampMeshes;

    [Tooltip("Объект-переключатель (например, выключатель на стене)")]
    public GameObject switchObject;

    [Header("Materials")]
    [Tooltip("Материал с включенной эмиссией (свечение)")]
    public Material materialOn;

    [Tooltip("Материал без эмиссии (выключенное состояние)")]
    public Material materialOff;

    [Header("Settings")]
    [Tooltip("Включен ли свет изначально при старте сцены")]
    public bool startOn = false;

    [Tooltip("Использовать плавное включение/выключение")]
    public bool useFade = false;

    [Tooltip("Длительность плавного перехода в секундах")]
    public float fadeDuration = 1f;

    [Header("Interaction Settings")]
    [Tooltip("Радиус взаимодействия с переключателем (дистанция, с которой можно нажать)")]
    public float interactionRange = 3f;

    [Header("Visual Feedback")]
    [Tooltip("Цвет подсветки переключателя при наведении")]
    public Color highlightColor = Color.yellow;

    // Приватные поля для хранения состояния и ссылок
    private float[] originalIntensities;           // Оригинальные интенсивности источников света
    private bool isLightOn;                         // Текущее состояние света (вкл/выкл)
    private bool isNearSwitch = false;              // Находится ли игрок рядом с переключателем
    private Renderer switchRenderer;                // Рендерер переключателя для изменения материала
    private Material originalSwitchMaterial;        // Оригинальный материал переключателя
    private Material highlightMaterial;             // Материал для подсветки переключателя
    private Camera mainCamera;                      // Главная камера для Raycast (мобильные устройства)
    private Material[] instanceMaterialsOn;         // Экземпляры материалов для плавного затухания
    private GameObject player;                      // Ссылка на игрока для оптимизации

    // Событие для UI (можно подписаться на это событие для отображения состояния)
    public System.Action<bool> OnLightStateChanged;

    // Input Actions для новой системы ввода
    private InputAction interactAction;              // Действие для кнопки E
    private InputAction tapAction;                   // Действие для тапа (клика) на мобильных устройствах

    void Awake()
    {
        // ========== НАСТРОЙКА INPUT SYSTEM ==========
        // Создаем Input Action для кнопки E на клавиатуре (для ПК и WebGL)
        // Привязываем к клавише E
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");

        // Подписываемся на событие нажатия кнопки
        interactAction.performed += OnInteractPerformed;

        // Создаем Input Action для тапа (клика) на мобильных устройствах
        // Привязываем к нажатию указателя (палец на экране или мышь)
        tapAction = new InputAction("Tap", binding: "<Pointer>/press");

        // Подписываемся на событие тапа
        tapAction.performed += OnTapPerformed;

        // Включаем обработку тапов (активируем Action)
        tapAction.Enable();

        // Примечание: interactAction будет включен в OnEnable, а выключен в OnDisable
    }

    void Start()
    {
        // Получаем ссылку на главную камеру для Raycast (нужно для мобильных устройств)
        mainCamera = Camera.main;

        // Находим игрока один раз для оптимизации
        player = GameObject.FindGameObjectWithTag("Player");

        // ========== ПОИСК ИСТОЧНИКОВ СВЕТА ==========
        // Если массив lights не заполнен в инспекторе, ищем все источники света
        // на дочерних объектах этого GameObject
        if (lights == null || lights.Length == 0)
        {
            lights = GetComponentsInChildren<Light>();
            Debug.Log($"Автоматически найдено {lights.Length} источников света");
        }

        // ========== ПОИСК МЕШЕЙ ЛАМПОЧЕК ==========
        // Если массив lampMeshes не заполнен, ищем все рендереры (меши) на дочерних объектах
        if (lampMeshes == null || lampMeshes.Length == 0)
        {
            lampMeshes = GetComponentsInChildren<Renderer>();
            Debug.Log($"Автоматически найдено {lampMeshes.Length} мешей лампочек");
        }

        // ========== НАСТРОЙКА ПЕРЕКЛЮЧАТЕЛЯ ==========
        SetupSwitch();

        // ========== ИНИЦИАЛИЗАЦИЯ СОСТОЯНИЯ СВЕТА ==========
        // Устанавливаем начальное состояние (включен/выключен)
        isLightOn = startOn;

        // ========== СОХРАНЕНИЕ ИНТЕНСИВНОСТЕЙ ИСТОЧНИКОВ СВЕТА ==========
        // Сохраняем оригинальные значения интенсивности, чтобы потом вернуть их при включении
        if (lights != null && lights.Length > 0)
        {
            originalIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    // Сохраняем оригинальную интенсивность
                    originalIntensities[i] = lights[i].intensity;

                    // Устанавливаем начальное состояние источника света
                    lights[i].enabled = isLightOn;

                    // Если используется плавное включение и свет выключен, устанавливаем интенсивность 0
                    if (!isLightOn && useFade)
                    {
                        lights[i].intensity = 0;
                    }
                }
            }
        }

        // ========== УСТАНОВКА НАЧАЛЬНЫХ МАТЕРИАЛОВ ==========
        SetInitialMaterials();

        // Выводим информацию о состоянии системы в консоль
        Debug.Log($"Система готова. Свет {(isLightOn ? "включен" : "выключен")}. " +
                  $"Нажмите E или коснитесь выключателя (радиус {interactionRange}м)");
    }

    /// <summary>
    /// Настройка переключателя: получение компонентов и создание материала для подсветки
    /// </summary>
    void SetupSwitch()
    {
        if (switchObject != null)
        {
            // Получаем компонент Renderer переключателя для изменения внешнего вида
            switchRenderer = switchObject.GetComponent<Renderer>();

            if (switchRenderer != null)
            {
                // Сохраняем оригинальный материал, чтобы вернуть его после подсветки
                originalSwitchMaterial = switchRenderer.material;

                // Создаем новый материал для подсветки на основе оригинального
                highlightMaterial = new Material(originalSwitchMaterial);
                highlightMaterial.color = highlightColor;

                // Если материал поддерживает эмиссию (свечение), добавляем эффект свечения
                if (highlightMaterial.HasProperty("_EmissionColor"))
                {
                    highlightMaterial.EnableKeyword("_EMISSION");
                    highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.5f);
                }
            }
        }
    }

    void OnEnable()
    {
        // Включаем обработку клавиши E (активируем Action)
        interactAction?.Enable();
    }

    void OnDisable()
    {
        // Выключаем обработку клавиши E
        interactAction?.Disable();
    }

    void OnDestroy()
    {
        // Очищаем ресурсы: отписываемся от событий и освобождаем память
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction.Dispose();
        }

        if (tapAction != null)
        {
            tapAction.performed -= OnTapPerformed;
            tapAction.Dispose();
        }

        // Возвращаем переключателю оригинальный материал
        if (switchRenderer != null && originalSwitchMaterial != null)
        {
            switchRenderer.material = originalSwitchMaterial;
        }
    }

    void Update()
    {
        // Каждый кадр проверяем, находится ли игрок рядом с переключателем
        UpdateNearSwitch();

        // Обновляем визуальную подсветку переключателя (вкл/выкл в зависимости от расстояния)
        UpdateSwitchHighlight();

        // Примечание: обработка тапа на мобильных устройствах происходит в OnTapPerformed
        // Update здесь нужен только для постоянных проверок состояния
    }

    /// <summary>
    /// Проверка расстояния от игрока до переключателя
    /// </summary>
    void UpdateNearSwitch()
    {
        if (switchObject == null) return;

        // Если ссылка на игрока потеряна, ищем заново
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        // Вычисляем расстояние между игроком и переключателем
        float distance = Vector3.Distance(player.transform.position, switchObject.transform.position);

        // Если расстояние меньше или равно радиусу взаимодействия, игрок рядом
        isNearSwitch = distance <= interactionRange;
    }

    /// <summary>
    /// Обновление внешнего вида переключателя (подсветка при нахождении рядом)
    /// </summary>
    void UpdateSwitchHighlight()
    {
        if (switchRenderer == null || highlightMaterial == null || originalSwitchMaterial == null) return;

        // Если игрок рядом - подсвечиваем переключатель, иначе возвращаем обычный материал
        if (isNearSwitch)
        {
            switchRenderer.material = highlightMaterial;
        }
        else
        {
            switchRenderer.material = originalSwitchMaterial;
        }
    }

    /// <summary>
    /// Обработчик нажатия клавиши E (для ПК и WebGL)
    /// </summary>
    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Проверяем, что игрок находится рядом с переключателем
        if (isNearSwitch)
        {
            ToggleLight();
        }
    }

    /// <summary>
    /// Обработчик тапа (клика) для мобильных устройств
    /// Исправленная версия с корректной проверкой UI через EventSystem.current
    /// </summary>
    void OnTapPerformed(InputAction.CallbackContext context)
    {
        // ВАЖНО: В колбэках InputSystem IsPointerOverGameObject() не работает корректно
        // Поэтому мы используем другой подход: откладываем проверку на следующий кадр

        // Запускаем корутину, которая выполнит проверку на следующем кадре
        StartCoroutine(CheckTapAndInteract());
    }

    /// <summary>
    /// Корутина для проверки тапа на следующем кадре (чтобы корректно работала проверка UI)
    /// </summary>
    private IEnumerator CheckTapAndInteract()
    {
        // Ждем один кадр, чтобы UI успел обновиться
        yield return null;

        // Теперь можно корректно проверить, был ли тап по UI элементу
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Тап был по UI элементу (кнопка, панель и т.д.) - игнорируем
            yield break;
        }

        // Проверяем, что камера существует
        if (mainCamera == null) yield break;

        // Получаем позицию тапа (клика) на экране
        // Mouse.current.position работает как для мыши, так и для тапа на мобильных устройствах
        Vector2 touchPosition = Mouse.current.position.ReadValue();

        // Создаем луч из камеры в направлении тапа
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        // Выполняем Raycast (проверка, во что попал луч)
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Проверяем, попали ли мы именно в переключатель
            if (hit.collider.gameObject == switchObject)
            {
                // Проверяем, что игрок находится рядом с переключателем
                if (isNearSwitch)
                {
                    ToggleLight();
                }
            }
        }
    }

    /// <summary>
    /// Переключение света (включить/выключить)
    /// </summary>
    public void ToggleLight()
    {
        if (useFade)
        {
            // Режим плавного включения/выключения
            if (!isLightOn)
            {
                // Включаем свет с плавным затуханием
                StartCoroutine(FadeLights(true));
                StartCoroutine(FadeMaterials(true));
            }
            else
            {
                // Выключаем свет с плавным затуханием
                StartCoroutine(FadeLights(false));
                StartCoroutine(FadeMaterials(false));
            }
        }
        else
        {
            // Режим мгновенного переключения
            isLightOn = !isLightOn;
            SetLightsState(isLightOn);
            SetMaterialsState(isLightOn);
        }

        // Вызываем событие для UI (если кто-то подписался)
        OnLightStateChanged?.Invoke(isLightOn);

        // Логируем действие в консоль
        Debug.Log($"Свет {(isLightOn ? "включен" : "выключен")}");
    }

    /// <summary>
    /// Установка начальных материалов для всех лампочек
    /// </summary>
    void SetInitialMaterials()
    {
        if (lampMeshes == null || lampMeshes.Length == 0) return;

        // Выбираем материал в зависимости от начального состояния света
        Material initialMaterial = isLightOn ? materialOn : materialOff;
        if (initialMaterial == null) return;

        // Для каждого меша лампочки создаем экземпляр материала
        // Создание экземпляра (instance) важно, чтобы изменения не затрагивали оригинальный материал
        foreach (Renderer mesh in lampMeshes)
        {
            if (mesh != null)
            {
                // Создаем копию материала для этого конкретного объекта
                mesh.material = new Material(initialMaterial);

                // Если используется плавное включение и свет включен, сохраняем экземпляр для управления
                if (isLightOn && useFade)
                {
                    if (instanceMaterialsOn == null)
                    {
                        instanceMaterialsOn = new Material[lampMeshes.Length];
                    }
                    instanceMaterialsOn[System.Array.IndexOf(lampMeshes, mesh)] = mesh.material;
                }
            }
        }
    }

    /// <summary>
    /// Мгновенное включение/выключение источников света
    /// </summary>
    /// <param name="isOn">Включить (true) или выключить (false)</param>
    private void SetLightsState(bool isOn)
    {
        if (lights == null) return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                // Включаем или выключаем источник света
                lights[i].enabled = isOn;

                // Если включаем, устанавливаем оригинальную интенсивность
                if (isOn && originalIntensities != null)
                {
                    lights[i].intensity = originalIntensities[i];
                }
            }
        }
    }

    /// <summary>
    /// Мгновенное включение/выключение материалов лампочек
    /// </summary>
    /// <param name="isOn">Включить (true) или выключить (false)</param>
    private void SetMaterialsState(bool isOn)
    {
        if (lampMeshes == null || lampMeshes.Length == 0) return;

        // Выбираем целевой материал в зависимости от состояния
        Material targetMaterial = isOn ? materialOn : materialOff;
        if (targetMaterial == null) return;

        // Для каждого меша создаем новый экземпляр материала
        foreach (Renderer mesh in lampMeshes)
        {
            if (mesh != null)
            {
                // Создаем копию материала
                mesh.material = new Material(targetMaterial);

                // Если включаем и используется плавное затухание, сохраняем экземпляр
                if (isOn && useFade)
                {
                    if (instanceMaterialsOn == null)
                    {
                        instanceMaterialsOn = new Material[lampMeshes.Length];
                    }
                    instanceMaterialsOn[System.Array.IndexOf(lampMeshes, mesh)] = mesh.material;

                    // Устанавливаем полную эмиссию (максимальное свечение)
                    if (mesh.material.HasProperty("_EmissionColor"))
                    {
                        mesh.material.EnableKeyword("_EMISSION");
                        Color fullColor = materialOn.GetColor("_EmissionColor");
                        mesh.material.SetColor("_EmissionColor", fullColor);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Корутина для плавного включения/выключения источников света
    /// </summary>
    /// <param name="fadeIn">true - включение, false - выключение</param>
    private IEnumerator FadeLights(bool fadeIn)
    {
        if (lights == null) yield break;

        if (fadeIn)
        {
            // ========== ПЛАВНОЕ ВКЛЮЧЕНИЕ СВЕТА ==========
            isLightOn = true;

            // Включаем все источники света, но с нулевой интенсивностью
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = true;
                    lights[i].intensity = 0;
                }
            }

            // Плавно увеличиваем интенсивность от 0 до оригинального значения
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration; // t от 0 до 1 за время fadeDuration

                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null && originalIntensities != null)
                    {
                        // Линейная интерполяция между 0 и оригинальной интенсивностью
                        lights[i].intensity = Mathf.Lerp(0, originalIntensities[i], t);
                    }
                }
                yield return null; // Ждем следующий кадр
            }

            // Финальная установка точных значений
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && originalIntensities != null)
                {
                    lights[i].intensity = originalIntensities[i];
                }
            }
        }
        else
        {
            // ========== ПЛАВНОЕ ВЫКЛЮЧЕНИЕ СВЕТА ==========
            isLightOn = false;

            float elapsed = 0;
            // Сохраняем текущие интенсивности перед началом анимации
            float[] startIntensities = new float[lights.Length];

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    startIntensities[i] = lights[i].intensity;
                }
            }

            // Плавно уменьшаем интенсивность до 0
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                    {
                        lights[i].intensity = Mathf.Lerp(startIntensities[i], 0, t);
                    }
                }
                yield return null;
            }

            // Выключаем источники света полностью
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = false;
                    lights[i].intensity = 0;
                }
            }
        }
    }

    /// <summary>
    /// Корутина для плавного включения/выключения эмиссии материалов (для URP)
    /// </summary>
    /// <param name="fadeIn">true - включение, false - выключение</param>
    private IEnumerator FadeMaterials(bool fadeIn)
    {
        if (materialOn == null || materialOff == null) yield break;
        if (lampMeshes == null || lampMeshes.Length == 0) yield break;

        if (fadeIn)
        {
            // ========== ПЛАВНОЕ ВКЛЮЧЕНИЕ ЭМИССИИ МАТЕРИАЛОВ ==========
            // Создаем экземпляры материалов ON для каждого меша
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                Renderer mesh = lampMeshes[i];
                if (mesh != null)
                {
                    // Создаем новый экземпляр материала с эмиссией
                    Material newMaterial = new Material(materialOn);
                    mesh.material = newMaterial;

                    // Сохраняем экземпляр для управления во время анимации
                    if (instanceMaterialsOn == null)
                    {
                        instanceMaterialsOn = new Material[lampMeshes.Length];
                    }
                    instanceMaterialsOn[i] = newMaterial;

                    // Начинаем с нулевой эмиссии (черный цвет)
                    if (newMaterial.HasProperty("_EmissionColor"))
                    {
                        newMaterial.EnableKeyword("_EMISSION");
                        newMaterial.SetColor("_EmissionColor", Color.black);
                    }
                }
            }

            // Плавно увеличиваем эмиссию от 0 до полной
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration; // t от 0 до 1

                for (int i = 0; i < lampMeshes.Length; i++)
                {
                    if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                    {
                        Material mat = instanceMaterialsOn[i];
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            // Получаем целевой цвет эмиссии из оригинального материала
                            Color targetColor = materialOn.GetColor("_EmissionColor");
                            float intensity = targetColor.maxColorComponent; // Яркость цвета

                            if (intensity > 0)
                            {
                                // Вычисляем базовый цвет (без интенсивности)
                                Color baseColor = targetColor / intensity;
                                // Текущий цвет = базовый цвет * (интенсивность * прогресс)
                                Color currentColor = baseColor * (intensity * t);
                                mat.SetColor("_EmissionColor", currentColor);
                            }
                        }
                    }
                }
                yield return null;
            }

            // Финальная установка полной эмиссии
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                {
                    Material mat = instanceMaterialsOn[i];
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        Color targetColor = materialOn.GetColor("_EmissionColor");
                        mat.SetColor("_EmissionColor", targetColor);
                    }
                }
            }
        }
        else
        {
            // ========== ПЛАВНОЕ ВЫКЛЮЧЕНИЕ ЭМИССИИ МАТЕРИАЛОВ ==========
            float elapsed = 0;
            // Сохраняем начальные интенсивности эмиссии перед анимацией
            float[] startIntensities = new float[lampMeshes.Length];

            for (int i = 0; i < lampMeshes.Length; i++)
            {
                if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                {
                    Material mat = instanceMaterialsOn[i];
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        Color startColor = mat.GetColor("_EmissionColor");
                        startIntensities[i] = startColor.maxColorComponent;
                    }
                }
            }

            // Плавно уменьшаем эмиссию до 0
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                for (int i = 0; i < lampMeshes.Length; i++)
                {
                    if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                    {
                        Material mat = instanceMaterialsOn[i];
                        if (mat.HasProperty("_EmissionColor") && startIntensities[i] > 0)
                        {
                            Color targetColor = materialOn.GetColor("_EmissionColor");
                            Color baseColor = targetColor / startIntensities[i];
                            float intensity = Mathf.Lerp(startIntensities[i], 0, t);
                            Color currentColor = baseColor * intensity;
                            mat.SetColor("_EmissionColor", currentColor);
                        }
                    }
                }
                yield return null;
            }

            // Меняем материал на выключенный (без эмиссии)
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                Renderer mesh = lampMeshes[i];
                if (mesh != null && materialOff != null)
                {
                    mesh.material = new Material(materialOff);
                }
            }

            // Очищаем массив экземпляров, так как они больше не нужны
            instanceMaterialsOn = null;
        }
    }

    /// <summary>
    /// Внешний метод для принудительного включения света
    /// </summary>
    public void TurnOn()
    {
        if (!isLightOn) ToggleLight();
    }

    /// <summary>
    /// Внешний метод для принудительного выключения света
    /// </summary>
    public void TurnOff()
    {
        if (isLightOn) ToggleLight();
    }

    /// <summary>
    /// Получить текущее состояние света
    /// </summary>
    public bool IsLightOn()
    {
        return isLightOn;
    }

    /// <summary>
    /// Визуализация радиуса взаимодействия в редакторе (для удобства настройки)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (switchObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(switchObject.transform.position, interactionRange);
        }
    }
}