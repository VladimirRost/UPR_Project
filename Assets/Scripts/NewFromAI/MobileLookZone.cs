using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLookZone : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    public CameraLook cameraLook;

    bool isDragging;

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
        
       // Debug.Log("Look delta: " + eventData.delta);
        
        cameraLook.Look(eventData.delta);
    }
}