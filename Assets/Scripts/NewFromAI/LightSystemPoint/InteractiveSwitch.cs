using UnityEngine;

public class InteractiveSwitch : MonoBehaviour, IInteractable
{
    [Header("Lights")]
    public LightSystemControl[] lightSystems;

    [Header("Handle animation")]
    public Transform handle;
    public float angle = -30f;
    public float speed = 6f;

    Quaternion startRot;
    Quaternion onRot;

    bool isOn;

    Outline outline;
    bool isFocused;

    void Start()
    {
        if (handle)
        {
            startRot = handle.localRotation;
            onRot = startRot * Quaternion.Euler(angle, 0, 0);
        }

        outline = GetComponentInChildren<Outline>();
    }

    public void Interact()
    {
        isOn = !isOn;

        foreach (var l in lightSystems)
            if (l) l.Set(isOn);

        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    System.Collections.IEnumerator Animate()
    {
        Quaternion from = handle.localRotation;
        Quaternion to = isOn ? onRot : startRot;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            handle.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
    }

    public void OnFocus()
    {
        isFocused = true;

        if (outline)
        {
            outline.enabled = true;
            outline.EmissionOn();
        }

        if (InteractionUI.Instance)
            InteractionUI.Instance.Show();
    }

    public void OnLoseFocus()
    {
        isFocused = false;

        if (outline)
        {
            outline.enabled = false;
            outline.EmissionOff();
        }

        if (InteractionUI.Instance)
            InteractionUI.Instance.Hide();
    }
}
