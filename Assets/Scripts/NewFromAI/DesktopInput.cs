using UnityEngine;

public class DesktopInput : MonoBehaviour
{
    public PlayerController playerController;
    public CameraLook cameraLook;

    void Update()
    {
        Vector2 look =
        playerController.Input.PlayerActionControl.Look.ReadValue<Vector2>();

        cameraLook.Look(look);
    }
}