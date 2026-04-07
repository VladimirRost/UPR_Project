using UnityEngine;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    public enum DoorType
    {
        Rotate,
        Slide
    }

    [Header("Тип двери")]
    public DoorType doorType;

    [Header("Установка поворота")]
    public float openAngle = 120f;
    public Vector3 rotationAxis = Vector3.up;

    [Header("Установка сдвига")]
    public Vector3 slideDirection = Vector3.right;
    public float slideDistance = 1f;

    [Header("Скорость")]
    public float speed = 2f;

    [Header("Автозакрытие")]
    [Tooltip("Если включено — дверь автоматически закроется")]
    public bool autoClose;
    [Tooltip("Через сколько секунд закрывать")]
    public float autoCloseDelay = 3f;

    [Header("Анимация ручки двери")]
    public Transform doorHandle;
    public Vector3 handleAxis = Vector3.right; // вокруг какой оси вращать
    public float handleAngle = -40f;
    public float handleSpeed = 6f;

    [Header("Звуковое сопровождение")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource; // Ссылка на источник звука

    Quaternion handleStartRotation;
    Quaternion handlePressedRotation;

    [Header("Объект для подсветки")]
    public Renderer[] renderers;
    private Color highlightColor = Color.yellow;
    private Color[] originalEmission;
    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private float autoCloseTimer;

    Outline outline;  // Подсветка по контуру 

    [Header("Цвет наведения")]
    [SerializeField] Shader outlineMaskShader;
    [SerializeField] Shader outlineFillShader;
    public float outlineWidth = 4f;
    public float outlineAppearSpeed = 10f;
    public float pulseAmplitude = 0.5f;
    public float pulseSpeed = 3f;
    bool isFocused;

    void Start()
    {
        // Инициализация AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; // Чтобы звук не играл сам по себе при запуске сцены

        closedRotation = transform.localRotation;
        closedPosition = transform.localPosition;
        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
        openPosition = closedPosition + slideDirection.normalized * slideDistance;

        if (doorHandle)
        {
            handleStartRotation = doorHandle.localRotation;
            handlePressedRotation = handleStartRotation * Quaternion.AngleAxis(handleAngle, handleAxis);
        }

        outline = GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
            outline.OutlineWidth = 5f;
        }
    }

    void Update()
    {
        // Логика движения двери
        if (doorType == DoorType.Rotate)
        {
            Quaternion target = isOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                target,
                speed * 200 * Time.deltaTime
            );
        }
        else
        {
            Vector3 target = isOpen ? openPosition : closedPosition;
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                target,
                speed * Time.deltaTime
            );
        }

        // Логика авто закрытия
        if (autoClose && isOpen)
        {
            autoCloseTimer -= Time.deltaTime;
            if (autoCloseTimer <= 0)
            {
                isOpen = false;
                PlayDoorSound(); // Добавлено: звук при автозакрытии
            }
        }

        // Поворот ручки
        if (doorHandle)
        {
            Quaternion target = isOpen ? handlePressedRotation : handleStartRotation;
            doorHandle.localRotation = Quaternion.Slerp(
                doorHandle.localRotation,
                target,
                handleSpeed * Time.deltaTime
            );
        }

        // Анимация контура подсветки
        if (outline != null)
        {
            float targetWidth = isFocused ? outlineWidth : 0f;
            outline.OutlineWidth = Mathf.Lerp(
                outline.OutlineWidth,
                targetWidth,
                Time.deltaTime * outlineAppearSpeed
            );

            if (isFocused)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                outline.OutlineWidth += pulse;
            }
        }
    }

    public void Interact()
    {
        isOpen = !isOpen;

        // Добавлено: воспроизведение звука при взаимодействии
        PlayDoorSound();

        if (isOpen && autoClose)
        {
            autoCloseTimer = autoCloseDelay;
        }

        if (doorHandle)
        {
            doorHandle.localRotation = Quaternion.Euler(handleAngle, 0, 0);
        }
    }

    // Вспомогательный метод для звука
    private void PlayDoorSound()
    {
        if (audioSource == null) return;

        if (isOpen)
        {
            if (openSound != null) audioStreamPlay(openSound);
        }
        else
        {
            if (closeSound != null) audioStreamPlay(closeSound);
        }
    }

    // Метод для чистого проигрывания (используем PlayOneShot, чтобы звуки не обрывали друг друга)
    private void audioStreamPlay(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void OnFocus()
    {
        if (outline != null)
        {
            outline.enabled = true;
            outline.EmissionOn();
        }
        isFocused = true;
        if (InteractionUI.Instance) InteractionUI.Instance.Show();
    }

    public void OnLoseFocus()
    {
        if (outline != null)
        {
            outline.enabled = false;
            outline.EmissionOff();
        }
        isFocused = false;
        if (InteractionUI.Instance) InteractionUI.Instance.Hide();
    }

    // ... остальной код без изменений
}
