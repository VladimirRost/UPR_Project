using UnityEngine;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    public enum DoorType
    {
        Rotate,
        Slide
    }

    [Header("Door Type")]
    public DoorType doorType;

    [Header("Rotation settings")]
    public float openAngle = 120f;
    public Vector3 rotationAxis = Vector3.up;

    [Header("Slide settings")]
    public Vector3 slideDirection = Vector3.right;
    public float slideDistance = 1f;

    [Header("Speed")]
    public float speed = 2f;

    [Header("Auto Close")]
    [Tooltip("Если включено — дверь автоматически закроется")]
    public bool autoClose;

    [Tooltip("Через сколько секунд закрывать")]
    public float autoCloseDelay = 3f;

 

    [Header("Handle Animation")]
    public Transform doorHandle;

    public Vector3 handleAxis = Vector3.right; // вокруг какой оси вращать
    public float handleAngle = -40f;
    public float handleSpeed = 6f;

    Quaternion handleStartRotation;
    Quaternion handlePressedRotation;




    private bool isOpen;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private float autoCloseTimer;

    void Start()
    {
        closedRotation = transform.localRotation;
        closedPosition = transform.localPosition;

        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
        openPosition = closedPosition + slideDirection.normalized * slideDistance;

        if (doorHandle)
        {
            handleStartRotation = doorHandle.localRotation;

            handlePressedRotation =
                handleStartRotation *
                Quaternion.AngleAxis(handleAngle, handleAxis);
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

    public void OnFocus() { }

    public void OnLoseFocus() { }
}