using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [SerializeField]
    public TMP_InputField EmailInputField;
    public TMP_InputField PasswordInputField;

    [SerializeField]
    public TMP_InputField FullnameInputField;
    public TMP_InputField RegEmailInputField;
    public TMP_InputField RegPasswordInputField;


    void Start()
    {
        //login();
    }

    void login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    // Player registration method
    public void registerNewUser()
    {
        string fullName = FullnameInputField.text;
        string email = RegEmailInputField.text;
        string password = RegPasswordInputField.text;

        if (!IsValidEmailFormat(email))
        {
            Debug.LogError("Registration failed: Invalid email format.");
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            DisplayName = fullName,
            Email = email,
            Password = password,
            Username = fullName,
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnLoginFailure);
    }

    // Player login method
    public void loginWithEmail()
    {
        string email = EmailInputField.text;
        string password = PasswordInputField.text;

        if (!IsValidEmailFormat(email))
        {
            Debug.LogError("Login failed: Invalid email format.");
            return;
        }

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    // Step 1: Local format check
    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    // Step 2: PlayFab database verification — called after registration success.
    // Attempts a login with the registered credentials to confirm the account
    // truly exists in PlayFab before loading the game scene.
    private void VerifyEmailExistsInPlayFab(string email, string password)
    {
        Debug.Log("Verifying account exists in PlayFab database...");

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
        };

        PlayFabClientAPI.LoginWithEmailAddress(
            request,
            // Account confirmed in PlayFab — safe to proceed
            verifyResult =>
            {
                Debug.Log($"Email verified in PlayFab database. PlayFabId: {verifyResult.PlayFabId}");
                SceneManager.LoadScene("EuropeanRoulette_mobile");
            },
            // Account NOT found or credentials invalid in PlayFab
            verifyError =>
            {
                // Error code 1001 = AccountNotFound, 1002 = InvalidEmailOrPassword
                Debug.LogError($"Email verification against PlayFab failed: {verifyError.GenerateErrorReport()}");
                Debug.LogError($"PlayFab Error Code: {verifyError.Error}");

                // Handle specific PlayFab error codes
                switch (verifyError.Error)
                {
                    case PlayFabErrorCode.AccountNotFound:
                        Debug.LogError("No PlayFab account found with this email.");
                        break;
                    case PlayFabErrorCode.InvalidEmailOrPassword:
                        Debug.LogError("Account found but credentials are invalid.");
                        break;
                    default:
                        Debug.LogError("Unexpected error during email verification.");
                        break;
                }
            }
        );
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log($"Registration successful! {result.Username} , {result.PlayFabId}");

        // After registration, verify the email actually exists in PlayFab
        // before allowing entry into the game scene
        string email = RegEmailInputField.text;
        string password = RegPasswordInputField.text;

        if (IsValidEmailFormat(email))
        {
            VerifyEmailExistsInPlayFab(email, password);
        }
        else
        {
            Debug.LogError("Post-registration email format check failed.");
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"Login successful! {result.NewlyCreated} , {result.PlayFabId} , {result.LastLoginTime}");
        SceneManager.LoadScene("EuropeanRoulette_mobile");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }
}