using UnityEngine;

public class PlatformInputSwitcher : MonoBehaviour
{
    public MonoBehaviour desktopInput;
    public MonoBehaviour mobileInput;

    public GameObject mobileUI;

    void Start()
    {
        bool mobile = Application.isMobilePlatform;

        desktopInput.enabled = !mobile;
        mobileInput.enabled = mobile;

        if (mobileUI)
            mobileUI.SetActive(mobile);
    }
}
