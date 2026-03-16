using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLookZone : MonoBehaviour,
    IDragHandler
{
    public CameraLook cameraLook;

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter.name == "Joystick") return;
        cameraLook.Look(eventData.delta);
    }
}
