using UnityEngine;

public class LightSystem : MonoBehaviour
{
    public Light[] lights;
    public Renderer[] lampMeshes;

    public Material materialOn;
    public Material materialOff;

    public bool isOn;

    public void Toggle()
    {
        isOn = !isOn;
        ApplyState();
    }

    public void Set(bool state)
    {
        isOn = state;
        ApplyState();
    }

    void Start()
    {
        ApplyState();
    }

    void ApplyState()
    {
        foreach (var l in lights)
            if (l) l.enabled = isOn;

        Material target = isOn ? materialOn : materialOff;

        foreach (var r in lampMeshes)
            if (r) r.material = target;
    }
}