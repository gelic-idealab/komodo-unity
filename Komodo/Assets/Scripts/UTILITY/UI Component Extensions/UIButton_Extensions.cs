using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIButton_Extensions 
{
    /// <summary>
    /// used to alternate colors when clicking buttons to show a state
    /// </summary>
    /// <param name="button"></param>
    /// <param name="stateColor"></param>
    /// <param name="activeState"></param>
    public static void SetButtonStateColor(this Button button, Color stateColor, bool activeState)
    {
        if (button == null || button.image == null)
            return;

        var colModified = stateColor;

        if (activeState)
        {
            colModified.a = 1;
            button.image.color = colModified;//new Color(0, 0.5f, 0, 1);
            var colors = button.colors;
            colors.normalColor = Color.white;// colModified;
            button.colors = colors;
        }
        else
        {
            colModified.a = 0.0f;
            button.image.color = colModified;///new Color(1, 1, 1 ,1);
            var colors = button.colors;
            colors.normalColor = new Color(1, 1, 1, 0);//Color.white;//colModified;
            button.colors = colors;

        }


        //           

    }
    public static void SetButtonStateColor(this Button button, bool activeState)
    {
        if (button == null || button.image == null)
            return;

        var butColor = button.image.color;

       // var colModified = stateColor;

        if (activeState)
        {
            butColor.a = 1;
            button.image.color = butColor;//new Color(0, 0.5f, 0, 1);
            var colors = button.colors;
            colors.normalColor = Color.white;// colModified;
            button.colors = colors;
        }
        else
        {
            butColor.a = 0.0f;
            button.image.color = butColor;///new Color(1, 1, 1 ,1);
            var colors = button.colors;
            colors.normalColor = new Color(1, 1, 1, 0);//Color.white;//colModified;
            button.colors = colors;

        }


    }

}
