using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DatabaseTest : MonoBehaviour
{
    [SerializeField] TMP_InputField userNameText; 
    [SerializeField] Button setUserDataButton;
    [SerializeField] Button getUserDataButton;
    [SerializeField] TMP_Text resultText;

    const string dataUrl = "https://lumeei-bubbleshooter-default-rtdb.europe-west1.firebasedatabase.app/";

    User user = null;

    void Awake()
    {
        getUserDataButton.onClick.AddListener(GetUserData);
        setUserDataButton.onClick.AddListener(SetUserData);
    }
    void SetUserData()
    { 
        if(user == null)
        {
            user = new()
            {
                coins = Random.Range(0, 1000),
                unlockedNextLevel = Random.Range(0, 10)
            };
        } 

        RestClient.Put(dataUrl + userNameText.text + ".json", user, OnSetResponse);
    }

    void GetUserData()
    {
        RestClient.Get<User>(dataUrl + userNameText.text + ".json", OnGetResponse);
    }

    private void OnGetResponse(RequestException exception, ResponseHelper helper, User user)
    {
        if (exception != null)
        {
            resultText.color = Color.red;
            resultText.text = $"Get Fail: {exception.Message}";
        }
        else if (user != null)
        {
            resultText.color = Color.green;
            resultText.text = $"Get Success: {user.coins} {user.unlockedNextLevel}";
        }
    }

    void OnSetResponse(RequestException exception, ResponseHelper helper)
    {
        if (exception != null)
        {
            resultText.color = Color.red;
            resultText.text = $"Set Fail: {exception.Message}";
        }
        else if (helper != null)
        {
            resultText.color = Color.green;
            resultText.text = $"Set Success: {helper.Text}";
        }
    }
}
