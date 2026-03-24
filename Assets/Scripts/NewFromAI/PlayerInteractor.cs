using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;



public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast settings")]
    public float interactDistance = 3f;
    public Camera playerCamera;

    private PlayerController playerController;
    private IInteractable currentInteractable;

    float lastInteractTime;
    public float interactCooldown = 0.2f;
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        if (!playerCamera)
            playerCamera = Camera.main;
    }

    void Update()
    {
        CheckInteractable();
        HandleInput();
    }

    void HandleInput()
    {
        bool pressed = false;

#if UNITY_ANDROID || UNITY_IOS
    if (Touchscreen.current != null &&
        Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
    {
        pressed = true;
    }
#else
        if (playerController != null &&
            playerController.Input.PlayerActionControl.Attack.WasPressedThisFrame())
        {
            pressed = true;
        }
#endif

        if (!pressed) return;

        TryInteract();
    }

    void TryInteract()
    {
        if (Time.time - lastInteractTime < interactCooldown)
            return;

        lastInteractTime = Time.time;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    void CheckInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable?.OnLoseFocus();
                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                }

                return;
            }
        }

        // если ничего нет
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    // для UI-кнопки (мобилка)
    public void InteractButton()
    {
        TryInteract();
    }
}