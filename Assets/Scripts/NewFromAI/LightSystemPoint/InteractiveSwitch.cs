using UnityEngine;
using System.Collections;

public class InteractiveSwitch : MonoBehaviour, IInteractable
{
    [Header("Lights")]
    public LightSystemControl[] lightSystems;

    [Header("Handle animation")]
    public Transform handle;
    public float angle = -30f;
    public float speed = 6f;

    [Header("Audio Settings")]
    public AudioClip switchOnSound;   // Звук при включении
    public AudioClip switchOffSound;  // Звук при выключении
    private AudioSource audioSource;

    Quaternion startRot;
    Quaternion onRot;
    bool isOn;
    Outline outline;
    bool isFocused;

    void Start()
    {
        // Инициализация ручки
        if (handle)
        {
            startRot = handle.localRotation;
            onRot = startRot * Quaternion.Euler(angle, 0, 0);
        }
        outline = GetComponentInChildren<Outline>();

        // Настройка AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; // Чтобы звук не играл сам по себе при старте сцены
    }

    public void Interact()
    {
        isOn = !isOn;

        // Проигрываем звук в зависимости от нового состояния
        PlaySwitchSound();

        foreach (var l in lightSystems)
            if (l) l.Set(isOn);

        if (handle)
             // Останавливаем старую анимацию, если нажали слишком быстро
                StopAllCoroutines();
        StartCoroutine(Animate());
    }

    private void PlaySwitchSound()
    {
        if (audioSource == null) return;

        if (isOn && switchOnSound != null)
        {
            audioSource.PlayOneShot(switchOnSound);
        }
        else if (!isOn && switchOffSound != null)
        {
            audioSource.PlayOneShot(switchOffSound);
        }
    }

    IEnumerator Animate()
    {
        if (!handle) yield break;

        Quaternion from = handle.localRotation;
        Quaternion to = isOn ? onRot : startRot;
        float t = 0;

        while (t < 1)
        {
            if (!handle) yield break;
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
