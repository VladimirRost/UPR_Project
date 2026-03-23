using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour,
    IDragHandler,
    IPointerUpHandler,
    IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;

    Vector2 inputVector;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;


    private Vector2 startTouchPos;
    public float deadZone = 0.1f; // чувствительность (подбирается)
    private bool isDragging = false;
    public float responseCurve = 2f; // 1 = линейно, >1 = плавный старт

    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);



    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
               background,
               eventData.position,
               eventData.pressEventCamera,
               out startTouchPos
           );

        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out currentPos))
        {
            Vector2 delta = currentPos - startTouchPos;

            // Нормализуем относительно размера джойстика
            Vector2 normalizedDelta = new Vector2(
                delta.x / background.sizeDelta.x,
                delta.y / background.sizeDelta.y
            );

            // Проверка dead zone
            if (!isDragging)
            {
                if (normalizedDelta.magnitude < deadZone)
                {
                    inputVector = Vector2.zero;
                    handle.anchoredPosition = Vector2.zero;
                    return;
                }

                isDragging = true;
            }

            // Основная логика движения

            Vector2 rawInput = new Vector2(normalizedDelta.x * 2, normalizedDelta.y * 2);
            rawInput = Vector2.ClampMagnitude(rawInput, 1);

            // Магнитуда (насколько сильно отклонили)
            float magnitude = rawInput.magnitude;

            // Применяем кривую (ключевая строка)
            float curvedMagnitude = speedCurve.Evaluate(magnitude);
            //float curvedMagnitude = Mathf.Pow(magnitude, responseCurve);

            // Восстанавливаем направление с новой "силой"
            inputVector = rawInput.normalized * curvedMagnitude;

            //inputVector = new Vector2(normalizedDelta.x * 2, normalizedDelta.y * 2);
            //inputVector = Vector2.ClampMagnitude(inputVector, 1);

            handle.anchoredPosition =
                new Vector2(
                    inputVector.x * (background.sizeDelta.x / 3),
                    inputVector.y * (background.sizeDelta.y / 3)
                );
        }


    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        isDragging = false;
    }
}
