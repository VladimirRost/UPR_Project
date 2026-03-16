using RimuruDev;
using UnityEngine;
using UnityEngine.UI;

public class MobilePlatformDetector : MonoBehaviour
{
    public GameObject mobileUI;
    public MonoBehaviour mobileMovement;


    void Start()
    {
        var detector = GetComponent<DeviceTypeDetector>();

        if (detector.CurrentDeviceType == CurrentDeviceType.WebMobile)
        {
            Debug.Log("Это мобильное устройство!");
            mobileUI.SetActive(true);
            mobileMovement.enabled = true;
            //desktopMovement.enabled = false;
        }
        else
        {
            Debug.Log("Это компьютер!");
            mobileUI.SetActive(false);
            mobileMovement.enabled = false;
            //desktopMovement.enabled = true;
        }
    }



    //public Text text;


    ////public MonoBehaviour desktopMovement;

    //private bool mobile;

    //void Start()
    //{
    //    switch (Application.platform)
    //    {
    //        case RuntimePlatform.Android:
    //        case RuntimePlatform.IPhonePlayer:
    //            mobile = true;
    //            break;
    //        case RuntimePlatform.WindowsPlayer:
    //        case RuntimePlatform.WindowsEditor:
    //        case RuntimePlatform.OSXPlayer:
    //        case RuntimePlatform.LinuxPlayer:
    //        default:
    //            mobile = false;
    //            break;
    //    }

    //    Debug.Log("Platform: " + Application.platform);
    //    Debug.Log("Mobile Version? " + mobile);
    //    text.text = Application.platform.ToString();

    //    //mobileUI.SetActive(mobile);

    //    //mobileMovement.enabled = mobile;

    //    //  Временно включаю мобильный интерфейс !!!!!!!!!!!!!!!
    //    mobileUI.SetActive(true);

    //    mobileMovement.enabled = true;





    //    // desktopMovement.enabled = !mobile;
    //}
}
