using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeScreen : MonoBehaviour
{
    public void StartGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene(1);
    }
}
