using UnityEngine;

public class Cloth : MonoBehaviour
{
    public static GameObject chipStackPref;

    private void Awake()
    {
        chipStackPref = Instantiate(Resources.Load<GameObject>("chipstack"));
    }

    public static ChipStack InstanceStack()
    {
        GameObject ob = Instantiate(chipStackPref);

        return ob.GetComponent<ChipStack>();
    }
}
