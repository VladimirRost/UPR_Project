using UnityEngine;
using UnityEngine.UI; // Для работы с UI Text (если используете стандартный Text)
using TMPro; // Раскомментируйте, если используете TextMeshPro
using System.Collections;

/// <summary>
/// Скрипт для подсчёта и отображения FPS (кадров в секунду)
/// Подходит для Unity 6 и выше
/// </summary>
public class FPS_Counter : MonoBehaviour, IFPSHandler
{
    [Header("Настройки отображения")]
    [SerializeField] private Text fpsText; // Для стандартного UI Text
    [SerializeField] [Range(0f, 1f)] private float alpha = 0.8f; // Степень прозрачности
    [SerializeField] private bool showOnStart = true; // Показывать счётчик при запуске
    // [SerializeField] private TextMeshProUGUI fpsText; // Для TextMeshPro (раскомментируйте эту строку и закомментируйте верхнюю)

    [Header("Настройки подсчёта")]
    [SerializeField] private float updateInterval = 0.5f; // Интервал обновления текста (секунды)
    [SerializeField] private int sampleCount = 60; // Количество кадров для усреднения (чем больше, тем стабильнее показатель)

    // Приватные переменные для подсчёта
    private float[] frameTimeBuffer; // Буфер для хранения времени кадров
    private int currentFrameIndex; // Индекс текущего кадра в буфере
    private float totalFrameTime; // Суммарное время всех кадров в буфере
    private float timeSinceLastUpdate; // Время с последнего обновления текста
    private int frameCounter; // Счётчик кадров между обновлениями

    [SerializeField] private PlayerController player;



    void Start()
    {
        // Инициализация буфера для усреднения FPS
        frameTimeBuffer = new float[sampleCount];
        currentFrameIndex = 0;
        totalFrameTime = 0f;
        timeSinceLastUpdate = 0f;
        frameCounter = 0;



        // Проверка, назначен ли компонент Text
        if (fpsText == null)
        {
            Debug.LogWarning("FPS Text не назначен! Скрипт будет работать, но текст не будет отображаться.");
        }

        // Опционально: можно запустить корутину для более точного обновления
        // StartCoroutine(UpdateFPSRoutine());
    }

    void Update()
    {
        // Подсчёт FPS с усреднением по времени кадров (более точный метод)
        CalculateSmoothFPS();

        // Альтернативный метод: обновление текста по времени (работает вместе с любым методом выше)
        UpdateFPSDisplay();
    }

    /// <summary>
    /// Метод для подсчёта FPS с усреднением по времени кадров
    /// Более точный и стабильный метод
    /// </summary>
    private void CalculateSmoothFPS()
    {
        // Получаем время, затраченное на последний кадр (в секундах)
        float currentFrameTime = Time.unscaledDeltaTime; // unscaledDeltaTime игнорирует Time.timeScale

        // Обновляем буфер: вычитаем старое значение, если буфер уже заполнен
        if (frameTimeBuffer[currentFrameIndex] > 0)
        {
            totalFrameTime -= frameTimeBuffer[currentFrameIndex];
        }

        // Добавляем новое значение в буфер
        frameTimeBuffer[currentFrameIndex] = currentFrameTime;
        totalFrameTime += currentFrameTime;

        // Переходим к следующему индексу (циклический буфер)
        currentFrameIndex = (currentFrameIndex + 1) % sampleCount;

        // Увеличиваем счётчик кадров
        frameCounter++;
    }



    /// <summary>
    /// Обновление отображения FPS с заданным интервалом
    /// </summary>
    private void UpdateFPSDisplay()
    {
        // Обновляем текст только через указанные интервалы для оптимизации
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            // Вычисляем средний FPS на основе данных в буфере
            float averageFrameTime = totalFrameTime / sampleCount;
            float fps = 1f / averageFrameTime;

            // Добавляем проверку на бесконечность (если averageFrameTime = 0)
            if (float.IsInfinity(fps) || float.IsNaN(fps))
            {
                fps = 0f;
            }

            UpdateFPSText(Mathf.RoundToInt(fps));

            // Сброс счётчика времени обновления
            timeSinceLastUpdate = 0f;
        }
    }

    /// <summary>
    /// Обновление текста с FPS и добавление цветовой индикации
    /// </summary>
    /// <param name="fps">Значение FPS</param>
    private void UpdateFPSText(int fps)
    {
        if (fpsText == null) return;

        // Форматируем текст с FPS
        string fpsString = $"FPS: {fps}";

        // Добавляем цветовую индикацию в зависимости от производительности
        Color textColor;
        if (fps >= 60)
        {
            textColor = new Color(0f, 1f, 0f, alpha); // Зелёный с прозрачностью
        }
        else if (fps >= 30)
        {
            textColor = new Color(1f, 1f, 0f, alpha); // Жёлтый с прозрачностью
        }
        else
        {
            textColor = new Color(1f, 0f, 0f, alpha); // Красный с прозрачностью
        }

        // Применяем цвет к тексту
        // Для стандартного UI Text
        fpsText.color = textColor;
        fpsText.text = fpsString;


        //Debug.Log(fps);

    }

 

    /// <summary>
    /// Метод для получения текущего FPS (может быть полезен для других скриптов)
    /// </summary>
    /// <returns>Текущее значение FPS</returns>
    public float GetCurrentFPS()
    {
        float averageFrameTime = totalFrameTime / sampleCount;
        if (averageFrameTime > 0)
        {
            return 1f / averageFrameTime;
        }
        return 0f;
    }

    /// <summary>
    /// Метод для получения FPS с округлением
    /// </summary>
    public int GetCurrentFPSRounded()
    {
        return Mathf.RoundToInt(GetCurrentFPS());
    }


    //   Ниже комметарии - которые не работают. !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    void OnEnable()
    {
        if (player != null)
        {
            player.RegisterFPSHandler(this);
            OnFPSToggle(player.IsFPSVisible);

            
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.UnregisterFPSHandler(this);
        }
    }

    public void OnFPSToggle(bool isEnabled)
    {
        if (fpsText != null)
            fpsText.gameObject.SetActive(isEnabled);
    }


}