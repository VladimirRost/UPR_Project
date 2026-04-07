using UnityEngine;
using System.Collections;

public class OpenDoorAuto : MonoBehaviour
{
    // ============================================
    // ПЕРЕЧИСЛЕНИЯ (ENUMS) ДЛЯ ВЫБОРА РЕЖИМОВ
    // ============================================

    public enum DoorType
    {
        Rotate,  // Дверь открывается поворотом (как обычная дверь)
        Slide    // Дверь открывается сдвигом (раздвижная дверь)
    }

    // ============================================
    // НАСТРАИВАЕМЫЕ ПАРАМЕТРЫ (ВИДНЫ В ИНСПЕКТОРЕ)
    // ============================================
    #region
    [Header("Тип двери")]
    public DoorType doorType;               // Выбор типа анимации двери

    [Header("Настройки поворота (для Rotate)")]
    public float openAngle = 120f;          // Угол открытия в градусах
    public Vector3 rotationAxis = Vector3.up; // Ось вращения (обычно Y для вертикальных дверей)

    [Header("Настройки сдвига (для Slide)")]
    public Vector3 slideDirection = Vector3.right; // Направление сдвига
    public float slideDistance = 1f;        // Расстояние сдвига в метрах

    [Header("Скорость анимации")]
    public float speed = 2f;                // Скорость открытия/закрытия (чем выше, тем быстрее)

    [Header("Кривые анимации (Easing)")]
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // Кривая открытия
    [SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Кривая закрытия

    [Header("Анимация ручки двери")]
    public Transform doorHandle;            // Ссылка на объект ручки
    public Vector3 handleAxis = Vector3.right; // Ось вращения ручки
    public float handleAngle = -40f;        // Угол поворота ручки при нажатии
    public float handleSpeed = 6f;          // Скорость анимации ручки

    [Header("Аудио эффекты")]
    public AudioSource audioSource;         // Источник звука (нужно добавить компонент AudioSource)
    public AudioClip openSound;             // Звук открытия двери
    public AudioClip closeSound;            // Звук закрытия двери
    public AudioClip ambientSound;          //Фоновая музыка

  


    [Header("Задержка автоматического закрытия")]
    public float autoCloseDelay = 2f;       // Задержка перед автоматическим закрытием (0 = не закрывать автоматически)

    [Header("Дополнительные эффекты")]
    public ParticleSystem openParticles;    // Эффект частиц при открытии (пыль, пар и т.д.)
    public Light doorLight;                 // Свет над дверью (меняет цвет при открытии)
    public Color openLightColor = Color.green;  // Цвет света при открытой двери
    public Color closedLightColor = Color.red;  // Цвет света при закрытой двери
    #endregion
    // ============================================
    // ВНУТРЕННИЕ ПЕРЕМЕННЫЕ (НЕ ВИДНЫ В ИНСПЕКТОРЕ)
    // ============================================
    #region
    // Состояния двери
    private bool isPlayerInside = false;    // Находится ли игрок в триггере двери
    private bool isOpen = false;            // Открыта ли сейчас дверь
    private bool isAnimating = false;       // Выполняется ли анимация в данный момент

    // Начальные позиции для анимации
    private Vector3 startPosition;          // Исходная позиция двери (закрыто)
    private Quaternion startRotation;       // Исходный поворот двери (закрыто)
    private Vector3 targetPosition;         // Целевая позиция (открыто)
    private Quaternion targetRotation;      // Целевой поворот (открыто)

    // Анимация ручки
    private Quaternion handleStartRotation;     // Исходный поворот ручки
    private Quaternion handlePressedRotation;   // Поворот ручки в нажатом состоянии

    // Корoutine для автоматического закрытия
    private Coroutine autoCloseCoroutine;   // Ссылка на корутину автозакрытия (чтобы можно было отменить)
    #endregion
    // ============================================
    // МЕТОДЫ ЖИЗНЕННОГО ЦИКЛА UNITY
    // ============================================

    private void Start()
    {

        // Запуск фоновой музыки
        //audioSource.
        // Назначаем аудиоклип в источник звука
        audioSource.clip = ambientSound;

        // Включаем зацикливание
        audioSource.loop = true;

        // Начинаем воспроизведение
        audioSource.Play();

        // Сохраняем начальное состояние двери (позицию и поворот в закрытом состоянии)
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Настраиваем анимацию ручки, если она назначена
        if (doorHandle != null)
        {
            // Сохраняем исходный поворот ручки
            handleStartRotation = doorHandle.localRotation;

            // Вычисляем поворот ручки в нажатом состоянии
            // Quaternion.Euler создает поворот на заданный угол вокруг указанной оси
            // Умножение кватернионов комбинирует повороты
            handlePressedRotation = handleStartRotation * Quaternion.Euler(handleAxis * handleAngle);
        }

        // Вычисляем целевое состояние для открытия в зависимости от типа двери
        if (doorType == DoorType.Rotate)
        {
            // Для поворотной двери: меняется только поворот, позиция остается той же
            // Умножаем кватернионы: текущий поворот * поворот на угол открытия
            targetRotation = startRotation * Quaternion.Euler(rotationAxis * openAngle);
            targetPosition = startPosition;
        }
        else // Slide
        {
            // Для раздвижной двери: меняется только позиция, поворот остается тем же
            // Нормализуем направление, чтобы расстояние всегда было точным
            targetPosition = startPosition + slideDirection.normalized * slideDistance;
            targetRotation = startRotation;
        }

        // Настраиваем свет, если он используется
        if (doorLight != null)
        {
            doorLight.color = closedLightColor; // Начинаем с закрытого состояния
        }
    }

    // Вызывается, когда другой коллайдер входит в триггер двери
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что вошел игрок, дверь не анимируется, и мы еще не внутри
        if (other.CompareTag("Player") && !isAnimating && !isPlayerInside)
        {
            Debug.Log("🚪 Игрок вошел в триггер двери - начинаем открытие");
            isPlayerInside = true;

            // Отменяем запланированное автоматическое закрытие, если оно было
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
                Debug.Log("⏱️ Автозакрытие отменено (игрок вернулся)");
            }

            // Открываем дверь, если она еще не открыта
            if (!isOpen)
            {
                StartCoroutine(OpenDoorCoroutine());
            }
        }
    }

    // Вызывается, когда другой коллайдер выходит из триггера двери
    private void OnTriggerExit(Collider other)
    {
        // Проверяем, что вышел игрок, дверь не анимируется, и мы были внутри
        if (other.CompareTag("Player") && !isAnimating && isPlayerInside)
        {
            Debug.Log("🚪 Игрок вышел из триггера двери");
            isPlayerInside = false;

            // Если нужно автоматическое закрытие и дверь открыта
            if (autoCloseDelay > 0 && isOpen && !isAnimating)
            {
                // Запускаем корутину автозакрытия (если еще не запущена)
                if (autoCloseCoroutine == null)
                {
                    autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
                }
            }
            else if (autoCloseDelay == 0 && isOpen)
            {
                // Если автозакрытие отключено (задержка 0), закрываем сразу
                StartCoroutine(CloseDoorCoroutine());
            }
        }
    }

    // ============================================
    // ОСНОВНЫЕ КОРУТИНЫ АНИМАЦИИ
    // ============================================

    // Корутина открытия двери
    private IEnumerator OpenDoorCoroutine()
    {
        // Блокируем возможность запуска другой анимации
        isAnimating = true;

        Debug.Log("🚪 Начинаем анимацию открытия двери");

        // Запускаем анимацию ручки (параллельно с движением двери)
        StartCoroutine(AnimateHandleCoroutine(true));

        // Воспроизводим звук открытия, если он назначен
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Запускаем эффект частиц при открытии
        if (openParticles != null)
        {
            openParticles.Play();
        }

        // Анимируем свет, если он есть (плавно меняем цвет)
        if (doorLight != null)
        {
            StartCoroutine(AnimateLightCoroutine(openLightColor));
        }

        // Основная анимация двери
        float elapsed = 0f;                         // Прошедшее время
        float duration = 1f / speed;                // Длительность = 1 / скорость

        // Запоминаем текущие позицию и поворот (на случай, если анимация началась не из исходного положения)
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        // Цикл анимации: выполняется, пока не пройдет вся длительность
        while (elapsed < duration)
        {
            // Вычисляем прогресс анимации (0 в начале, 1 в конце)
            float t = elapsed / duration;

            // Применяем кривую анимации для нелинейного движения
            // Например: EaseInOut делает движение плавным в начале и конце
            float easedT = openCurve.Evaluate(t);

            // Плавно меняем позицию и поворот двери
            // Vector3.Lerp - линейная интерполяция позиции
            // Quaternion.Slerp - сферическая интерполяция поворота (дает плавное вращение)
            transform.position = Vector3.Lerp(currentPos, targetPosition, easedT);
            transform.rotation = Quaternion.Slerp(currentRot, targetRotation, easedT);

            // Увеличиваем счетчик времени на время, прошедшее с последнего кадра
            elapsed += Time.deltaTime;

            // Выходим из корутины до следующего кадра
            yield return null;
        }

        // Фиксируем конечное положение (чтобы избежать ошибок округления)
        transform.position = targetPosition;
        transform.rotation = targetRotation;

        // Обновляем состояние двери
        isOpen = true;
        isAnimating = false;

        Debug.Log("🚪 Дверь полностью открыта");
    }

    // Корутина закрытия двери
    private IEnumerator CloseDoorCoroutine()
    {
        // Блокируем возможность запуска другой анимации
        isAnimating = true;

        Debug.Log("🚪 Начинаем анимацию закрытия двери");

        // Запускаем анимацию ручки (возврат в исходное положение)
        StartCoroutine(AnimateHandleCoroutine(false));

        // Воспроизводим звук закрытия, если он назначен
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        // Анимируем свет обратно к закрытому состоянию
        if (doorLight != null)
        {
            StartCoroutine(AnimateLightCoroutine(closedLightColor));
        }

        // Основная анимация двери
        float elapsed = 0f;
        float duration = 1f / speed;

        // Запоминаем текущие позицию и поворот
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = closeCurve.Evaluate(t);

            // Интерполируем обратно к начальным значениям
            transform.position = Vector3.Lerp(currentPos, startPosition, easedT);
            transform.rotation = Quaternion.Slerp(currentRot, startRotation, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Фиксируем конечное положение
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Обновляем состояние
        isOpen = false;
        isAnimating = false;

        Debug.Log("🚪 Дверь полностью закрыта");
    }

    // ============================================
    // ВСПОМОГАТЕЛЬНЫЕ КОРУТИНЫ ДЛЯ ЭФФЕКТОВ
    // ============================================

    // Корутина анимации ручки двери
    private IEnumerator AnimateHandleCoroutine(bool pressing)
    {
        // Если ручка не назначена, выходим
        if (doorHandle == null) yield break;

        // Определяем начальный и конечный поворот ручки
        Quaternion startHandleRot = doorHandle.localRotation;
        Quaternion endHandleRot = pressing ? handlePressedRotation : handleStartRotation;

        // Анимация ручки происходит быстрее двери для реалистичности
        float elapsed = 0f;
        float duration = 0.2f;  // Фиксированная короткая длительность

        while (elapsed < duration)
        {
            // Плавно интерполируем поворот ручки
            float t = elapsed / duration;
            doorHandle.localRotation = Quaternion.Slerp(startHandleRot, endHandleRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Фиксируем конечный поворот
        doorHandle.localRotation = endHandleRot;
    }

    // Корутина плавного изменения цвета света
    private IEnumerator AnimateLightCoroutine(Color targetColor)
    {
        if (doorLight == null) yield break;

        Color startColor = doorLight.color;
        float elapsed = 0f;
        float duration = 0.5f;  // Полсекунды на смену цвета

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            doorLight.color = Color.Lerp(startColor, targetColor, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        doorLight.color = targetColor;
    }

    // Корутина автоматического закрытия с задержкой
    private IEnumerator AutoCloseCoroutine()
    {
        // Ждем указанное количество секунд
        Debug.Log($"⏱️ Дверь закроется через {autoCloseDelay} секунд");
        yield return new WaitForSeconds(autoCloseDelay);

        // Проверяем, не вернулся ли игрок в триггер за время ожидания
        // и не началась ли уже другая анимация
        if (!isPlayerInside && isOpen && !isAnimating)
        {
            Debug.Log("⏱️ Задержка прошла, закрываем дверь");
            StartCoroutine(CloseDoorCoroutine());
        }
        else if (isPlayerInside)
        {
            Debug.Log("⏱️ Автозакрытие отменено - игрок вернулся");
        }

        // Сбрасываем ссылку на корутину
        autoCloseCoroutine = null;
    }

    // ============================================
    // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ВЫЗОВА ИЗ ДРУГИХ СКРИПТОВ
    // ============================================

    // Метод для принудительного открытия двери (можно вызвать из другого скрипта)
    public void ForceOpen()
    {
        if (!isAnimating && !isOpen)
        {
            StopAllCoroutines(); // Останавливаем все текущие анимации
            autoCloseCoroutine = null;
            StartCoroutine(OpenDoorCoroutine());
        }
    }

    // Метод для принудительного закрытия двери
    public void ForceClose()
    {
        if (!isAnimating && isOpen)
        {
            StopAllCoroutines();
            autoCloseCoroutine = null;
            StartCoroutine(CloseDoorCoroutine());
        }
    }

    // Метод для переключения состояния двери
    public void ToggleDoor()
    {
        if (!isAnimating)
        {
            if (isOpen)
                ForceClose();
            else
                ForceOpen();
        }
    }

    // ============================================
    // ВИЗУАЛИЗАЦИЯ В РЕДАКТОРЕ (ДЛЯ УДОБСТВА НАСТРОЙКИ)
    // ============================================

    // Этот метод рисует подсказки в окне Scene (только в редакторе, не в игре)
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Показываем предпросмотр движения двери
            Gizmos.color = Color.green;

            if (doorType == DoorType.Rotate)
            {
                // Для поворотной двери рисуем дугу открытия
                Vector3 center = transform.position;
                Vector3 axis = transform.TransformDirection(rotationAxis);
                Gizmos.DrawRay(center, axis);

                // Рисуем начальное направление
                Vector3 startDir = transform.forward;
                Quaternion rotation = Quaternion.AngleAxis(openAngle, axis);
                Vector3 endDir = rotation * startDir;

                // Рисуем два направления: закрыто и открыто
                Gizmos.DrawRay(center, startDir);
                Gizmos.DrawRay(center, endDir);
            }
            else // Slide
            {
                // Для раздвижной двери рисуем линию сдвига
                Vector3 direction = slideDirection.normalized * slideDistance;
                Gizmos.DrawLine(transform.position, transform.position + direction);

                // Рисуем куб в конечной точке (полупрозрачный)
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawWireCube(transform.position + direction, transform.localScale);
            }
        }
    }
}