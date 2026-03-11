using UnityEngine;
using UnityEngine.InputSystem;

// Этот скрипт висит на игроке или камере.
// Он ищет интерактивные объекты перед игроком.

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast settings")]

    // дальность взаимодействия
    public float interactDistance = 3f;

    // камера игрока
    public Camera playerCamera;

    // ссылка на систему ввода
    private PlayerController playerController;

    // текущий объект под прицелом
    private IInteractable currentInteractable;

    void Start()
    {
        // получаем главный контроллер игрока
        playerController = FindFirstObjectByType<PlayerController>();

        // если камера не назначена
        if (!playerCamera)
            playerCamera = Camera.main;
    }

    void Update()
    {

        CheckInteractable();  // Проверяем взгляд на объект
        // Проверяем кнопку взаимодействия
        if (playerController.Input.PlayerActionControl.PressLeftButton.WasPressedThisFrame())
        {
            TryInteract();

        }
    }

    void TryInteract()
    {
        // создаём луч из центра камеры
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        // проверяем попадание
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // ищем интерфейс IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            // если объект поддерживает взаимодействие
            if (interactable != null)
            {
                interactable.Interact();
                // Ниже сам пишу..................................
                //interactable.OnFocus(); // Работает вызов
            }
        }
    }

    //Проверка объекта под прицелом
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

                Debug.Log("Объект под прицелом");

                return;
            }
        }

        // если никуда не смотрим
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }

    }

   

}