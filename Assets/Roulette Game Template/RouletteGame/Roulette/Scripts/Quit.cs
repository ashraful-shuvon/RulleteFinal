using UnityEngine;

public class Quit : MonoBehaviour
{
    public GameObject toggledObj;
    private static readonly string scapeInput = "escape";

    private void Update()
    {
        if (Input.GetKeyDown(scapeInput) && !toggledObj.activeSelf)
            toggledObj.SetActive(true);
        
        else if(Input.GetKeyDown(scapeInput) &&  toggledObj.activeSelf)
            toggledObj.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
