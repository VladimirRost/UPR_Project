using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLookZone : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    public CameraLook cameraLook;

    bool isDragging;

    public float sensitivity = 1.5f; // усиление движения

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (cameraLook == null) return;

        cameraLook.Look(eventData.delta * sensitivity);
    }
}