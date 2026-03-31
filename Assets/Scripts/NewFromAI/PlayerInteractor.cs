
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast settings")]
    public float interactDistance = 3f;
    public Camera playerCamera;

    [Header("Debug")]
    public bool showDebugRay = false;

    private IInteractable currentInteractable;

    private float lastInteractTime;
    public float interactCooldown = 0.2f;


    void Start()
    {
        if (!playerCamera)
            playerCamera = Camera.main;
    }

    void Update()
    {
        CheckInteractable(); // подсветка
        HandleInput();       // ввод

        if (showDebugRay)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.yellow);
        }
    }

    // ================= INPUT =================

    void HandleInput()
    {
        bool pressed = false;
        Vector2 screenPosition = Vector2.zero;

        // 🖥 МЫШЬ
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressed = true;
            screenPosition = Mouse.current.position.ReadValue();
        }

        // 📱 TOUCH (если не было мыши)
        if (!pressed && Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                pressed = true;
                screenPosition = touch.position.ReadValue();
            }
        }

        if (!pressed) return;

        // игнор UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (currentInteractable == null)
            return;

        TryInteractStrict(screenPosition);
    }
    void TryInteractStrict(Vector2 screenPosition)
    {
        if (Time.time - lastInteractTime < interactCooldown)
            return;

        lastInteractTime = Time.time;

        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = null;

            if (!hit.collider.TryGetComponent(out interactable))
            {
                hit.collider.transform.parent?.TryGetComponent(out interactable);
            }

            // 🔥 КЛЮЧЕВАЯ ПРОВЕРКА
            if (interactable != null && interactable == currentInteractable)
            {
                interactable.Interact();
            }
        }
    }
    // ================= MOBILE INTERACT =================

    void TryInteractFromScreen(Vector2 screenPosition)
    {
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = null;

            if (!hit.collider.TryGetComponent(out interactable))
            {
                hit.collider.transform.parent?.TryGetComponent(out interactable);
            }

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    // ================= RAYCAST (FOCUS) =================

    void CheckInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = null;

            if (!hit.collider.TryGetComponent(out interactable))
            {
                hit.collider.transform.parent?.TryGetComponent(out interactable);
            }

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

        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    // ================= UI BUTTON =================

    public void InteractButton()
    {
        currentInteractable?.Interact();
    }
}








//-------------------------------------------------------------------------------------------------------------------------------------------------


//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;

//public class PlayerInteractor : MonoBehaviour
//{
//    [Header("Raycast settings")]
//    public float interactDistance = 3f;
//    public Camera playerCamera;

//    [Header("Debug")]
//    public bool showDebugRay = true;
//    public Color rayColor = Color.green;

//    private PlayerController playerController;
//    private IInteractable currentInteractable;

//    float lastInteractTime;
//    public float interactCooldown = 0.2f;

//    void Start()
//    {
//        playerController = FindFirstObjectByType<PlayerController>();

//        if (!playerCamera)
//            playerCamera = Camera.main;

//        Debug.Log($"Платформа: {Application.platform}, Screen: {Screen.width}x{Screen.height}");
//    }

//    void Update()
//    {
//        CheckInteractable();
//        HandleInput();

//        // Визуализация центрального луча для подсветки
//        if (showDebugRay)
//        {
//            Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
//            Debug.DrawRay(centerRay.origin, centerRay.direction * interactDistance, Color.yellow);
//        }
//    }

//    void HandleInput()
//    {
//        bool pressed = false;
//        Vector2 screenPosition = Vector2.zero;

//        // Универсальный способ получения позиции нажатия
//        // Проверяем касания на мобильных устройствах
//        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
//        {
//            for (int i = 0; i < Touchscreen.current.touches.Count; i++)
//            {
//                var touch = Touchscreen.current.touches[i];
//                if (touch.press.wasPressedThisFrame)
//                {
//                    pressed = true;
//                    screenPosition = touch.position.ReadValue();
//                    Debug.Log($"📱 Касание #{i} в позиции: {screenPosition}");
//                    break;
//                }
//            }
//        }
//        // Если нет касаний, проверяем мышь (для ПК и симулятора)
//        else if (Mouse.current != null)
//        {
//            // Используем левую кнопку мыши
//            if (Mouse.current.leftButton.wasPressedThisFrame)
//            {
//                pressed = true;
//                screenPosition = Mouse.current.position.ReadValue();
//                Debug.Log($"🖱️ Клик мыши в позиции: {screenPosition}");
//            }
//        }

//        // Альтернативный вариант через Input System (если есть Attack action)
//        if (!pressed && playerController != null &&
//            playerController.Input.PlayerActionControl.Attack.WasPressedThisFrame())
//        {
//            pressed = true;
//            // Если нет информации о позиции, используем центр экрана
//            if (screenPosition == Vector2.zero)
//            {
//                screenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
//                Debug.Log($"🎮 Attack action, используем центр экрана: {screenPosition}");
//            }
//        }

//        if (!pressed) return;

//        // Проверяем, не попали ли мы в UI
//        if (IsPointerOverUI(screenPosition))
//        {
//            Debug.Log("⚠️ Попали в UI элемент, игнорируем взаимодействие");
//            return;
//        }

//        // 🔥 КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: взаимодействие только если объект подсвечен
//        if (currentInteractable == null)
//        {
//            Debug.Log("❌ Нет подсвеченного объекта - взаимодействие невозможно");
//            return;
//        }

//        // Взаимодействие по позиции касания/мыши
//        TryInteractAtScreenPosition(screenPosition);
//    }

//    bool IsPointerOverUI(Vector2 screenPosition)
//    {
//        // Проверяем, не нажали ли мы на UI элемент
//        if (EventSystem.current == null) return false;

//        var pointerEventData = new PointerEventData(EventSystem.current);
//        pointerEventData.position = screenPosition;

//        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
//        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

//        return raycastResults.Count > 0;
//    }

//    void TryInteractAtScreenPosition(Vector2 screenPosition)
//    {
//        if (Time.time - lastInteractTime < interactCooldown)
//            return;

//        lastInteractTime = Time.time;

//        // Проверяем, что координаты корректны
//        if (screenPosition == Vector2.zero ||
//            screenPosition.x < 0 || screenPosition.x > Screen.width ||
//            screenPosition.y < 0 || screenPosition.y > Screen.height)
//        {
//            Debug.LogWarning($"⚠️ Некорректные координаты: {screenPosition}, Screen: {Screen.width}x{Screen.height}");
//            // Используем центр экрана как fallback
//            screenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
//            Debug.Log($"🔄 Используем центр экрана: {screenPosition}");
//        }

//        // Создаём луч из точки касания
//        Ray ray = playerCamera.ScreenPointToRay(screenPosition);

//        Debug.Log($"🔍 Луч из точки {screenPosition}, направление: {ray.direction}");

//        // Визуализация луча взаимодействия
//        if (showDebugRay)
//        {
//            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red, 1f);
//        }

//        RaycastHit hit;

//        // Пробуем с разными LayerMask, если нужно
//        int layerMask = Physics.DefaultRaycastLayers;

//        if (Physics.Raycast(ray, out hit, interactDistance, layerMask))
//        {
//            Debug.Log($"🎯 Луч попал в: {hit.collider.name} (Tag: {hit.collider.tag}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Distance: {hit.distance})");

//            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

//            // Проверяем также на родительских объектах
//            if (interactable == null && hit.collider.transform.parent != null)
//            {
//                interactable = hit.collider.transform.parent.GetComponent<IInteractable>();
//                if (interactable != null)
//                    Debug.Log($"📦 Нашли IInteractable на родителе: {hit.collider.transform.parent.name}");
//            }

//            // 🔥 ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: взаимодействуем только с подсвеченным объектом
//            if (interactable != null && interactable == currentInteractable)
//            {
//                Debug.Log($"✅ Взаимодействие с подсвеченным объектом {hit.collider.name}");
//                interactable.Interact();
//            }
//            else if (interactable != null && interactable != currentInteractable)
//            {
//                Debug.Log($"⚠️ Найден объект {hit.collider.name}, но он не подсвечен. Взаимодействие отменено.");
//            }
//            else
//            {
//                Debug.Log($"❌ Объект {hit.collider.name} не имеет компонента IInteractable");
//            }
//        }
//        else
//        {
//            Debug.Log($"❌ Луч никуда не попал (дистанция: {interactDistance})");
//        }
//    }

//    void CheckInteractable()
//    {
//        // Для подсветки используем центральный луч
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit, interactDistance))
//        {
//            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

//            // Проверяем также на родительских объектах
//            if (interactable == null && hit.collider.transform.parent != null)
//            {
//                interactable = hit.collider.transform.parent.GetComponent<IInteractable>();
//            }

//            if (interactable != null)
//            {
//                if (currentInteractable != interactable)
//                {
//                    currentInteractable?.OnLoseFocus();
//                    currentInteractable = interactable;
//                    currentInteractable.OnFocus();
//                    Debug.Log($"✨ Подсветка: {hit.collider.name}");
//                }

//                return;
//            }
//        }

//        // если ничего нет
//        if (currentInteractable != null)
//        {
//            currentInteractable.OnLoseFocus();
//            currentInteractable = null;
//            Debug.Log("✨ Подсветка снята");
//        }
//    }

//    // для UI-кнопки (мобилка)
//    public void InteractButton()
//    {
//        // 🔥 Для кнопки тоже проверяем наличие подсвеченного объекта
//        if (currentInteractable == null)
//        {
//            Debug.Log("❌ Нет подсвеченного объекта - взаимодействие через кнопку невозможно");
//            return;
//        }

//        Vector2 centerScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
//        Debug.Log($"🔘 Нажата UI кнопка, центр экрана: {centerScreen}");
//        TryInteractAtScreenPosition(centerScreen);
//    }
//}

//----------------------------------------------------------------------------------------------------------------------------------------------

//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;



//public class PlayerInteractor : MonoBehaviour
//{
//    [Header("Raycast settings")]
//    public float interactDistance = 3f;
//    public Camera playerCamera;

//    private PlayerController playerController;
//    private IInteractable currentInteractable;

//    float lastInteractTime;
//    public float interactCooldown = 0.2f;
//    void Start()
//    {
//        playerController = FindFirstObjectByType<PlayerController>();

//        if (!playerCamera)
//            playerCamera = Camera.main;
//    }

//    void Update()
//    {
//        CheckInteractable();
//        HandleInput();
//    }

//    void HandleInput()
//    {
//        bool pressed = false;

//#if UNITY_ANDROID || UNITY_IOS
//    if (Touchscreen.current != null &&
//        Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
//    {
//        pressed = true;
//    }
//#else
//        if (playerController != null &&
//            playerController.Input.PlayerActionControl.Attack.WasPressedThisFrame())
//        {
//            pressed = true;
//        }
//#endif

//        if (!pressed) return;

//        // 🔥 КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: проверяем, есть ли сейчас подсвеченный объект
//        if (currentInteractable != null)
//        {
//            TryInteract();
//        }
//        //--------------------------------------------------------------------
//    }

//    void TryInteract()
//    {
//        if (Time.time - lastInteractTime < interactCooldown)
//            return;

//        lastInteractTime = Time.time;

//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit, interactDistance))
//        {
//            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

//            if (interactable != null)
//            {
//                interactable.Interact();
//            }
//        }
//    }

//    void CheckInteractable()
//    {
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit, interactDistance))
//        {
//            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

//            if (interactable != null)
//            {
//                if (currentInteractable != interactable)
//                {
//                    currentInteractable?.OnLoseFocus();
//                    currentInteractable = interactable;
//                    currentInteractable.OnFocus();
//                }

//                return;
//            }
//        }

//        // если ничего нет
//        if (currentInteractable != null)
//        {
//            currentInteractable.OnLoseFocus();
//            currentInteractable = null;
//        }
//    }

//    // для UI-кнопки (мобилка)
//    public void InteractButton()
//    {
//        // 🔥 Для кнопки тоже проверяем наличие подсвеченного объекта
//        if (currentInteractable != null)
//        {
//            TryInteract();
//        }
//    }
//}