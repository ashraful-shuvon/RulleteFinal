using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public TabGroup tabGroup;
    [Space]
    public GameObject window;

    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    private Image bg;

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    private void Awake()
    {
        bg = GetComponent<Image>();
        tabGroup.Subscribe(this);
    }


    public void ChangeColor(Color newColor)
    {
        bg.color = newColor;
    }

    public void Select()
    {
        onTabSelected?.Invoke();
        if(window != null)
            window.SetActive(true);
    }

    public void Deselect()
    {
        onTabDeselected?.Invoke();
        if(window != null)
            window.SetActive(false);
    }
}
