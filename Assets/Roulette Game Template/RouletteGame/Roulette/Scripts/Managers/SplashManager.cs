using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public float delay = 1f;

    void Start()
    {
        Invoke("LoadAuthScene", delay);
    }

    void LoadAuthScene()
    {
        SceneManager.LoadScene("AuthScreen");
    }
}