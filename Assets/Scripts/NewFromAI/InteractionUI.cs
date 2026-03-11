using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    public GameObject interactIcon;

    void Awake()
    {
        Instance = this;

        if (interactIcon)
            interactIcon.SetActive(false);
    }

    public void Show()
    {
        if (interactIcon)
           // interactIcon.SetActive(true);
        interactIcon.SetActive(false);
    }

    public void Hide()
    {
        if (interactIcon)
            interactIcon.SetActive(false);
    }
}