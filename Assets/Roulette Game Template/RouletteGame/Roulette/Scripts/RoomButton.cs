using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }
}
