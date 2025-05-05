using Proyecto26;
using TMPro;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    [SerializeField] TMP_Text debugInfo;
    [SerializeField] string testUserID = "TestId";

    const string databaseUrl = "https://lumeei-bubbleshooter-default-rtdb.europe-west1.firebasedatabase.app/";
    const string playerPrefsKey = "userData";

    static string userId = null;
    static User currentUser = null;
    public static User CurrentUser => currentUser;

    void Awake()
    {
        // Load User data from PlayerPrefs
        if (PlayerPrefs.HasKey(playerPrefsKey))
            currentUser = JsonUtility.FromJson<User>(PlayerPrefs.GetString(playerPrefsKey));
        else
            currentUser = new User();

        // Load User data from Database
        string url = Application.absoluteURL;

        const string paramName = "userid=";
        int index = url.IndexOf(paramName);
        if (index != -1)
        {
            userId = url[(index + paramName.Length)..];
            userId = userId[..userId.IndexOf("&")];
        }
        else if (!string.IsNullOrEmpty(testUserID))
        {
            userId = testUserID;
        }

        RestClient.Get<User>(databaseUrl + userId + ".json", OnGetResponse);

        debugInfo.text =
            "User: " + userId + "\n" +
            "Coins: " + currentUser.coins + "\n" +
            "Lives: " + currentUser.lives + "\n" +
            "Level: " + currentUser.unlockedNextLevel;

    }

    public static void SaveUserData()
    {
        if(userId!= null)
            RestClient.Put(databaseUrl + userId + ".json", currentUser);

        PlayerPrefs.SetString(playerPrefsKey, JsonUtility.ToJson(currentUser));
    }

    void OnGetResponse(RequestException exception, ResponseHelper helper, User user)
    {
        if (exception == null && user != null)
            currentUser.MergeWith(user);
    }
}
