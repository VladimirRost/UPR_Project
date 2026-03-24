using UnityEngine;
using System.Collections;

public class LightSystemControl : MonoBehaviour
{
    [Header("Lights")]
    public Light[] lights;

    [Header("Meshes (лампочки)")]
    public Renderer[] lampMeshes;

    [Header("Materials")]
    public Material materialOn;
    public Material materialOff;

    [Header("Settings")]
    public bool isOn;
    public bool useFade = true;
    public float fadeDuration = 1f;

    float[] originalIntensity;
    Material[] runtimeMats;

    void Start()
    {
        // сохраняем интенсивности
        originalIntensity = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i])
                originalIntensity[i] = lights[i].intensity;
        }

        InitMaterials();
        ApplyInstant(isOn);
    }

    void InitMaterials()
    {
        runtimeMats = new Material[lampMeshes.Length];

        for (int i = 0; i < lampMeshes.Length; i++)
        {
            if (lampMeshes[i])
            {
                runtimeMats[i] = new Material(materialOff);
                lampMeshes[i].material = runtimeMats[i];
            }
        }
    }

    public void Toggle()
    {
        Set(!isOn);
    }

    public void Set(bool state)
    {
        if (useFade)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(state));
        }
        else
        {
            ApplyInstant(state);
        }

        isOn = state;
    }

    void ApplyInstant(bool state)
    {
        // свет
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i])
            {
                lights[i].enabled = state;
                lights[i].intensity = state ? originalIntensity[i] : 0;
            }
        }

        // материалы
        for (int i = 0; i < runtimeMats.Length; i++)
        {
            if (runtimeMats[i])
            {
                Material src = state ? materialOn : materialOff;
                runtimeMats[i].CopyPropertiesFromMaterial(src);
            }
        }
    }

    IEnumerator FadeRoutine(bool turnOn)
    {
        float t = 0;

        // стартовые значения
        float[] startInt = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            if (lights[i])
                startInt[i] = lights[i].intensity;

        Color targetEmission = materialOn.GetColor("_EmissionColor");

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;

            // свет
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i])
                {
                    float target = turnOn ? originalIntensity[i] : 0;
                    lights[i].intensity = Mathf.Lerp(startInt[i], target, t);

                    if (turnOn)
                        lights[i].enabled = true;
                }
            }

            // эмиссия
            foreach (var mat in runtimeMats)
            {
                if (mat && mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");

                    Color col = turnOn
                        ? Color.Lerp(Color.black, targetEmission, t)
                        : Color.Lerp(targetEmission, Color.black, t);

                    mat.SetColor("_EmissionColor", col);
                }
            }

            yield return null;
        }

        if (!turnOn)
        {
            foreach (var l in lights)
                if (l) l.enabled = false;
        }
    }
}