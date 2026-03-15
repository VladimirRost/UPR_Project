using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;

    Vector2 inputVector;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            pos.x /= background.sizeDelta.x;
            pos.y /= background.sizeDelta.y;

            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            inputVector = Vector2.ClampMagnitude(inputVector, 1);

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
    }
}
