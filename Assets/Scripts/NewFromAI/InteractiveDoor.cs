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

    Quaternion handleStartRotation;
    Quaternion handlePressedRotation;


    // Старая подсветка объекта
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


    Outline outline;  // Подсветка по контуру  ------------------------------------------

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
        closedRotation = transform.localRotation;
        closedPosition = transform.localPosition;

        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
        openPosition = closedPosition + slideDirection.normalized * slideDistance;

        if (doorHandle)  //  ручка двери установка начальных значений
        {
            handleStartRotation = doorHandle.localRotation;

            handlePressedRotation =
                handleStartRotation *
                Quaternion.AngleAxis(handleAngle, handleAxis);
        }

        if (renderers != null && renderers.Length > 0)   // сохраняем исходный цвет Emission объекта
        {
            originalEmission = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material.HasProperty("_EmissionColor"))
                {
                    originalEmission[i] =
                        renderers[i].material.GetColor("_EmissionColor");
                }


                

            }

            

        }

        outline = GetComponentInChildren<Outline>();  // инициализация контурной подсветки
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineWidth = 0;
        }





    }

    void Update()
    {
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

        // логика авто закрытия
        if (autoClose && isOpen)
        {
            autoCloseTimer -= Time.deltaTime;

            if (autoCloseTimer <= 0)
            {
                isOpen = false;
            }
        }

        if (doorHandle)
        {
            Quaternion target = isOpen ? handlePressedRotation : handleStartRotation;

            doorHandle.localRotation = Quaternion.Slerp(
                doorHandle.localRotation,
                target,
                handleSpeed * Time.deltaTime
            );
        }

        // Анимация контура подсветки Начало  ------------------------------------
        if (outline == null) return;

        float targetWidth = isFocused ? outlineWidth : 0f;

        outline.OutlineWidth = Mathf.Lerp(
            outline.OutlineWidth,
            targetWidth,
            Time.deltaTime * outlineAppearSpeed
        );

        if (isFocused)
        {
            float pulse =
                Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;

            outline.OutlineWidth += pulse;
        }

        // Анимация контура подсветки Конец   ------------------------


    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (isOpen && autoClose)
        {
            autoCloseTimer = autoCloseDelay;
        }

        if (doorHandle)
        {
            doorHandle.localRotation = Quaternion.Euler(handleAngle, 0, 0);
        }
    }

    public void OnFocus() 
    {
        //Debug.Log("Он фокус");
        //Highlight(true);

        //if (InteractionUI.Instance)
        //    InteractionUI.Instance.Show();

        isFocused = true;

        if (InteractionUI.Instance)
            InteractionUI.Instance.Show();



    }

    public void OnLoseFocus()
    {
        //Debug.Log("ОУТ фокус");
        //Highlight(false);

        //if (InteractionUI.Instance)
        //    InteractionUI.Instance.Hide();

        isFocused = false;

        if (InteractionUI.Instance)
            InteractionUI.Instance.Hide();


    }
    
    // HighLight не используется
    void Highlight(bool state)  
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_EmissionColor"))
            {
                if (state)
                {
                    renderers[i].material.EnableKeyword("_EMISSION");
                    renderers[i].material.SetColor("_EmissionColor", highlightColor);
                }
                else
                {
                    renderers[i].material.SetColor("_EmissionColor", originalEmission[i]);
                }
            }
        }
    }
}