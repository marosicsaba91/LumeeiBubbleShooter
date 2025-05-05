using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseAuthentication : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] Button signInButton;
    [SerializeField] TMP_Text resultInfoText;


    private void Awake()
    {
        signInButton.onClick.AddListener(OnSignInButtonClicked);
    }

    void OnSignInButtonClicked()
    {
        // string url = Application.absoluteURL;

        string email = emailInput.text;
        string password = passwordInput.text;

        // FirebaseAuth.SignInWithEmailAndPassword(email, password, name, nameof(FirebaseAuthSuccessCallback), nameof(FirebaseAuthFailedFallback));
         
    }

    void FirebaseAuthSuccessCallback(string output)
    {
        resultInfoText.color = Color.green;
        resultInfoText.text = $"Success: {output}";
    }

    void FirebaseAuthFailedFallback(string output)
    {
        resultInfoText.color = Color.red;
        resultInfoText.text = $"Fail: {output}";

        // FirebaseError firebaseError = JsonUtility.FromJson<FirebaseError>(output);
    }
}
