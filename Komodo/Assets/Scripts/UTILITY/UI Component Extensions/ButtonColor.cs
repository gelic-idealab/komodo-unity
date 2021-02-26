using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ButtonColor 
{
    /// <summary>
    /// used to alternate colors when clicking buttons to show a state
    /// </summary>
    /// <param name="button"></param>
    /// <param name="stateColor"></param>
    /// <param name="activeState"></param>
    public static void SetButtonColor(this Button button, Color stateColor, bool activeState)
    {
        Color buttonIsActiveColor = new Color(0, 0.5f, 0, 1);
        Color buttonIsInactiveColor = new Color(1, 1, 1 ,1);

        SetButtonColor(button, activeState, buttonIsActiveColor, buttonIsInactiveColor);
    }

    public static void SetButtonColor(this Button button, bool activeState, Color buttonIsActiveColor, Color buttonIsInactiveColor)
    {

        if (button == null || button.image == null) {
            return;
        }

        if (activeState)
        {
           button.image.color = buttonIsActiveColor;
        }
        else
        {
            button.image.color = buttonIsInactiveColor;
        }
    }
}
