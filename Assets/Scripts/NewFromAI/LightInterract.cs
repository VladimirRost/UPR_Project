using UnityEngine;
using System.Collections;

public class ZoneLightController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Все источники света на сцене")]
    public Light[] lights;

    [Tooltip("Все рендереры (меши лампочек)")]
    public Renderer[] lampMeshes;

    [Header("Materials")]
    public Material materialOn;   // Материал с включенной эмиссией (оригинал)
    public Material materialOff;  // Материал без эмиссии (оригинал)

    [Header("Settings")]
    public bool startOn = false;
    public bool useFade = false;
    public float fadeDuration = 1f;

    private float[] originalIntensities;
    private bool isPlayerInside = false;
    private Material[] instanceMaterialsOn;  // Экземпляры материалов для плавного затухания
    private bool materialsAreInstanced = false;

    void Start()
    {
        // Если массивы не заполнены, ищем автоматически
        if (lights == null || lights.Length == 0)
        {
            lights = GetComponentsInChildren<Light>();
        }

        if (lampMeshes == null || lampMeshes.Length == 0)
        {
            lampMeshes = GetComponentsInChildren<Renderer>();
        }

        // Сохраняем оригинальные интенсивности
        if (lights != null && lights.Length > 0)
        {
            originalIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    originalIntensities[i] = lights[i].intensity;
                    lights[i].enabled = startOn;

                    if (!startOn && useFade)
                    {
                        lights[i].intensity = 0;
                    }
                }
            }
        }

        // Устанавливаем начальное состояние материалов
        SetInitialMaterials();

        Debug.Log($"Найдено источников света: {(lights != null ? lights.Length : 0)}");
        Debug.Log($"Найдено рендереров: {(lampMeshes != null ? lampMeshes.Length : 0)}");
    }

    void SetInitialMaterials()
    {
        if (lampMeshes == null || lampMeshes.Length == 0) return;

        Material initialMaterial = startOn ? materialOn : materialOff;
        if (initialMaterial == null) return;

        foreach (Renderer mesh in lampMeshes)
        {
            if (mesh != null)
            {
                // Создаем экземпляр материала для каждого рендерера
                mesh.material = new Material(initialMaterial);

                // Если это материал "включено" и используется fade, сохраняем экземпляр
                if (initialMaterial == materialOn && useFade)
                {
                    if (instanceMaterialsOn == null)
                    {
                        instanceMaterialsOn = new Material[lampMeshes.Length];
                    }
                    // Сохраняем экземпляр для последующего управления эмиссией
                    instanceMaterialsOn[System.Array.IndexOf(lampMeshes, mesh)] = mesh.material;
                }
            }
        }

        materialsAreInstanced = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerInside)
        {
            isPlayerInside = true;

            if (useFade)
            {
                // Плавное включение
                StartCoroutine(FadeLights(true));
                StartCoroutine(FadeMaterials(true));
            }
            else
            {
                // Мгновенное включение
                SetLightsState(true);
                SetMaterialsState(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isPlayerInside)
        {
            isPlayerInside = false;

            if (useFade)
            {
                // Плавное выключение
                StartCoroutine(FadeLights(false));
                StartCoroutine(FadeMaterials(false));
            }
            else
            {
                // Мгновенное выключение
                SetLightsState(false);
                SetMaterialsState(false);
            }
        }
    }

    private void SetLightsState(bool isOn)
    {
        if (lights == null) return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].enabled = isOn;
                if (isOn && originalIntensities != null)
                {
                    lights[i].intensity = originalIntensities[i];
                }
            }
        }
    }

    private void SetMaterialsState(bool isOn)
    {
        if (lampMeshes == null || lampMeshes.Length == 0) return;

        Material targetMaterial = isOn ? materialOn : materialOff;
        if (targetMaterial == null) return;

        foreach (Renderer mesh in lampMeshes)
        {
            if (mesh != null)
            {
                // Создаем новый экземпляр материала
                mesh.material = new Material(targetMaterial);

                // Если это материал "включено" и используется fade, обновляем экземпляры
                if (isOn && useFade)
                {
                    if (instanceMaterialsOn == null)
                    {
                        instanceMaterialsOn = new Material[lampMeshes.Length];
                    }
                    instanceMaterialsOn[System.Array.IndexOf(lampMeshes, mesh)] = mesh.material;

                    // Сбрасываем эмиссию на максимальное значение
                    if (mesh.material.HasProperty("_EmissionColor"))
                    {
                        mesh.material.EnableKeyword("_EMISSION");
                        Color fullColor = materialOn.GetColor("_EmissionColor");
                        mesh.material.SetColor("_EmissionColor", fullColor);
                    }
                }
            }
        }

        materialsAreInstanced = true;
    }

    private IEnumerator FadeLights(bool fadeIn)
    {
        if (lights == null) yield break;

        if (fadeIn)
        {
            // Включаем все источники
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = true;
                    lights[i].intensity = 0;
                }
            }

            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null && originalIntensities != null)
                    {
                        lights[i].intensity = Mathf.Lerp(0, originalIntensities[i], t);
                    }
                }
                yield return null;
            }

            // Финальная установка
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && originalIntensities != null)
                {
                    lights[i].intensity = originalIntensities[i];
                }
            }
        }
        else
        {
            float elapsed = 0;
            float[] startIntensities = new float[lights.Length];

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    startIntensities[i] = lights[i].intensity;
                }
            }

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                    {
                        lights[i].intensity = Mathf.Lerp(startIntensities[i], 0, t);
                    }
                }
                yield return null;
            }

            // Выключаем источники
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = false;
                    lights[i].intensity = 0;
                }
            }
        }
    }

    private IEnumerator FadeMaterials(bool fadeIn)
    {
        if (materialOn == null || materialOff == null) yield break;
        if (lampMeshes == null || lampMeshes.Length == 0) yield break;

        if (fadeIn)
        {
            // Создаем экземпляры материалов и настраиваем их для плавного включения
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                Renderer mesh = lampMeshes[i];
                if (mesh != null)
                {
                    // Создаем новый экземпляр материала On
                    Material newMaterial = new Material(materialOn);
                    mesh.material = newMaterial;

                    // Сохраняем экземпляр для управления
                    if (instanceMaterialsOn == null)
                    {
                        instanceMaterialsOn = new Material[lampMeshes.Length];
                    }
                    instanceMaterialsOn[i] = newMaterial;

                    // Включаем эмиссию и устанавливаем начальное значение 0
                    if (newMaterial.HasProperty("_EmissionColor"))
                    {
                        newMaterial.EnableKeyword("_EMISSION");
                        newMaterial.SetColor("_EmissionColor", Color.black);
                    }
                }
            }

            // Плавно увеличиваем эмиссию
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                for (int i = 0; i < lampMeshes.Length; i++)
                {
                    if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                    {
                        Material mat = instanceMaterialsOn[i];
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            Color targetColor = materialOn.GetColor("_EmissionColor");
                            float intensity = targetColor.maxColorComponent;
                            if (intensity > 0)
                            {
                                Color baseColor = targetColor / intensity;
                                Color currentColor = baseColor * (intensity * t);
                                mat.SetColor("_EmissionColor", currentColor);
                            }
                        }
                    }
                }
                yield return null;
            }

            // Финальная установка полной эмиссии
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                {
                    Material mat = instanceMaterialsOn[i];
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        Color targetColor = materialOn.GetColor("_EmissionColor");
                        mat.SetColor("_EmissionColor", targetColor);
                    }
                }
            }
        }
        else
        {
            // Плавное выключение - уменьшаем эмиссию до 0
            float elapsed = 0;

            // Сохраняем начальные значения эмиссии
            float[] startIntensities = new float[lampMeshes.Length];
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                {
                    Material mat = instanceMaterialsOn[i];
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        Color startColor = mat.GetColor("_EmissionColor");
                        startIntensities[i] = startColor.maxColorComponent;
                    }
                }
            }

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                for (int i = 0; i < lampMeshes.Length; i++)
                {
                    if (instanceMaterialsOn != null && i < instanceMaterialsOn.Length && instanceMaterialsOn[i] != null)
                    {
                        Material mat = instanceMaterialsOn[i];
                        if (mat.HasProperty("_EmissionColor") && startIntensities[i] > 0)
                        {
                            Color targetColor = materialOn.GetColor("_EmissionColor");
                            Color baseColor = targetColor / startIntensities[i];
                            float intensity = Mathf.Lerp(startIntensities[i], 0, t);
                            Color currentColor = baseColor * intensity;
                            mat.SetColor("_EmissionColor", currentColor);
                        }
                    }
                }
                yield return null;
            }

            // Меняем материал на выключенный (создаем новый экземпляр)
            for (int i = 0; i < lampMeshes.Length; i++)
            {
                Renderer mesh = lampMeshes[i];
                if (mesh != null && materialOff != null)
                {
                    mesh.material = new Material(materialOff);
                }
            }

            // Очищаем массив экземпляров
            instanceMaterialsOn = null;
        }
    }
}