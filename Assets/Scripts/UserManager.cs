using BubbleShooterKit;
using EasyEditor;
using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserManager : MonoBehaviour
{
    enum UserState
    {
        None,             // Start state
        NoUserId,         // No User Id found, Not Synchronizing
        Local,            // Database not reached, Only locally stored data loaded
        Database,         // Database accessed, data from the database loaded
    }

    [SerializeField] TMP_Text debugInfo;
    [SerializeField] TMP_Text warningText;
    [SerializeField] string testUserID = "TestId";
    [SerializeField] string sceneToLoad = "HomeScreen";
    [SerializeField] GameConfiguration gameConfig;
    [Space]
    [SerializeField, ReadOnly] EasyProperty userIDProperty = new(nameof(userId));
    [SerializeField, ReadOnly] EasyProperty userStateProperty = new(nameof(userState));
    [SerializeField] User currentUser = null;
    [SerializeField] EasyButton trySaveButton = new(nameof(TrySaveUserData));

    const string databaseUrl = "https://lumeei-bubbleshooter-default-rtdb.europe-west1.firebasedatabase.app/";
    const string userSaveKeyBase = "userData-";
    const string urlParamName = "userid=";
    const string urlParamEnd = "&";

    static UserManager singleInstance;
    static UserState userState = UserState.None;
    static string userId = null;

    public static User CurrentUser
    {
        get
        {
            if (singleInstance == null)
                singleInstance = FindAnyObjectByType<UserManager>();

            return singleInstance.currentUser;
        }
    }

    static string CurrentUserSaveKey => userId == null ? userSaveKeyBase + "TEMP_USER" : userSaveKeyBase + userId;

    void Awake()
    {
        singleInstance = this;  // Singleton
        DontDestroyOnLoad(gameObject);

        warningText.gameObject.SetActive(false);

        //Try get User ID
        string url = Application.absoluteURL;

        if (!string.IsNullOrEmpty(url))
        {
            int index = url.IndexOf(urlParamName);
            if (index != -1)
            {
                userId = url[(index + urlParamName.Length)..];
                userId = userId[..userId.IndexOf(urlParamEnd)];
            }
        }

        if (string.IsNullOrEmpty(userId) && Application.isEditor && !string.IsNullOrEmpty(testUserID))
        {
            userId = testUserID;
        }

        if(userId == null)
        {
            currentUser = new User(gameConfig);
            userState = UserState.NoUserId;
            warningText.gameObject.SetActive(true);
            warningText.text = "No User ID found";

            FreshDebugText();
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            RestClient.Get<User>(databaseUrl + userId + ".json", OnDatabaseGetResponse);
        }

        void OnDatabaseGetResponse(RequestException exception, ResponseHelper helper, User user)
        {
            if (exception == null && user != null)
            {
                currentUser = user;
                userState = UserState.Database;
            }
            else if(exception is RequestException requestException)
            {
                if (!requestException.IsHttpError && !requestException.IsNetworkError)
                {
                    // The Database is accessed but the UserID is not in there
                    currentUser = new(gameConfig);
                    userState = UserState.Database;
                }
            }

            // Is User is not in the database, try to load from local data
            if (currentUser == null)
            {
                string playerPrefsUserKey = CurrentUserSaveKey;
                if (PlayerPrefs.HasKey(playerPrefsUserKey))
                {
                    string userJson = PlayerPrefs.GetString(playerPrefsUserKey);
                    currentUser = JsonUtility.FromJson<User>(userJson);
                    userState = UserState.Local;
                }

                warningText.gameObject.SetActive(true);
                warningText.text = "Server not reached";
            }

            // If User is not in the database and not in local data, create a new User
            if (currentUser == null)
            {
                currentUser = new User(gameConfig);
                userState = UserState.Local;
            }

            FreshDebugText();
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public static void TrySaveUserData()
    {
        if (userState is UserState.Database)
            RestClient.Put(databaseUrl + userId + ".json", CurrentUser, OnDatabasePutResponse);

        if(userState is UserState.Database or UserState.Local) 
            PlayerPrefs.SetString(CurrentUserSaveKey, JsonUtility.ToJson(CurrentUser));

        if (userState is UserState.Local)
            RestClient.Get<User>(databaseUrl + userId + ".json", OnDatabaseGetResponse);

        singleInstance.FreshDebugText();
    }

    static void OnDatabasePutResponse(RequestException exception, ResponseHelper helper)
    {
        if(exception!= null)
            Debug.Log($"User data for {userId} is saved to database");
    }

    static void OnDatabaseGetResponse(RequestException exception, ResponseHelper helper, User user)
    {
        if (exception == null && user != null)
        {
            CurrentUser.MergeWith(user);
            userState = UserState.Database;

            singleInstance.FreshDebugText();
            singleInstance.warningText.gameObject.SetActive(false);
        }
    }

    void FreshDebugText()
    {
        if (debugInfo == null || !debugInfo.isActiveAndEnabled)
            return;

        debugInfo.text =
            "User: " + userId + "\n" +
            "User State: " + userState + "\n" +
            "-----------------------------\n" +
            currentUser.ToString();
    }
}