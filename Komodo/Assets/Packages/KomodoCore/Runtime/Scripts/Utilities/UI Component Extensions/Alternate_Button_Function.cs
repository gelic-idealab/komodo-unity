using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Komodo.Utilities
{
    /// <summary>
    /// This class allows one button to have two different purposes with clicking on the same button. This is similar to the play and pause button on Spotify; you don't need two seperate buttons for that -- you just need one. This is what this class is for.
    /// </summary>
    public class Alternate_Button_Function : MonoBehaviour
    {
        /// <summary>
        /// A UnityEvent for the first click on a button.
        /// </summary>
        public UnityEvent onFirstClick;

        /// <summary>
        /// A UnityEvent for the second click on the same button.
        /// </summary>
        public UnityEvent onSecondClick;

        /// <summary>
        /// A boolean value to determine if the button is first clicked.
        /// </summary>
        [ShowOnly] public bool isFirstClick;

        /// <summary>
        /// This is not being used.
        /// </summary>
        public void AlternateButtonFunctions()
        {
            if (!isFirstClick)
                onFirstClick.Invoke();

            else
                onSecondClick.Invoke();

            isFirstClick = !isFirstClick;
        }

        /// <summary>
        /// This is not being used either.
        /// </summary>
        public void CallSecondActionIfFirstActionWasMade()
        {
            if (isFirstClick)
            {
                onSecondClick.Invoke();
                isFirstClick = false;
            }
        }

        //public void CallFirstActionWithoutAlternatingFlag()
        //{
        //    onFirstClick.Invoke();
        //}
    }
}