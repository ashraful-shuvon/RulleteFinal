using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    TMP_Text display;
    float deltaTime = 0.0f;

    readonly Color color = new Color(0.0f, 0.0f, 0.5f, 1.0f);

    private void Start()
    {
        display = GetComponent<TMP_Text>();
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {        
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        display.text = text;
    }
}
