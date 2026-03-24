using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class SplashManager : MonoBehaviour
{
    public float delay = 1f;

    void Start()
    {
        string savedEmail = PlayerPrefs.GetString("SavedEmail", "");
        string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

        if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
        {
            Debug.Log("Found saved credentials! Silently logging into PlayFab from Splash Screen...");
            var request = new LoginWithEmailAddressRequest
            {
                Email = savedEmail,
                Password = savedPassword,
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
        else
        {
            Invoke("LoadAuthScene", delay);
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"Auto-login from Splash Screen successful! PlayFabId: {result.PlayFabId}");
        SceneManager.LoadScene("EuropeanRoulette_mobile");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Auto-login failed: " + error.GenerateErrorReport());
        
        // Wipe stale credentials so we don't try again next boot
        PlayerPrefs.DeleteKey("SavedEmail");
        PlayerPrefs.DeleteKey("SavedPassword");
        PlayerPrefs.Save();
        
        LoadAuthScene();
    }

    void LoadAuthScene()
    {
        SceneManager.LoadScene("AuthScreen");
    }
}