using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMenu : MonoBehaviour
{
    [SerializeField] Button fullScreenButton;
    [SerializeField] Button changeResolutionButton;
    [SerializeField] TMP_Text infoText;
    [SerializeField] TMP_Text urlTest;

    void Awake()
    {
        fullScreenButton.onClick.AddListener(ChangeFullScreenMode);
        changeResolutionButton.onClick.AddListener(ChangeResolution);
    }

    void ChangeFullScreenMode()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    void ChangeResolution()
    {
        //Switch between 1920x1080 and 1280x720
        if (Screen.currentResolution.height == 1000)
        {
            Screen.SetResolution(1000, 2000, Screen.fullScreen);
        }
        else
        {
            Screen.SetResolution(1000, 1000, Screen.fullScreen);
        }

    }

    void Update()
    {
        infoText.text = Screen.currentResolution.ToString();
        urlTest.text = Application.absoluteURL;
    }
}
