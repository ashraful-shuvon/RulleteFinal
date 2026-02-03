using UnityEngine;
using System.Collections.Generic;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Color tabIdle;
    public Color tabHover;
    public Color tabActive;
    public Color tabDeactive;

    public TabButton tab;

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
            tabButtons = new List<TabButton>();

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        if(tab == null || button != tab)
            button.ChangeColor(tabHover);
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        tab?.Deselect();

        tab = button;
        tab.Select();
        ResetTabs();
        button.ChangeColor(tabActive);
    }


    public void ResetTabs() {

        foreach (TabButton button in tabButtons)
        {
            if (tab == button)
                continue;
            button.ChangeColor(tabIdle);
        }
    }

    public void DeselectTabs()
    {
        tab = null;
        foreach (TabButton button in tabButtons)
        {
            button.ChangeColor(tabIdle);
            button.Deselect();
        }
    }
}
