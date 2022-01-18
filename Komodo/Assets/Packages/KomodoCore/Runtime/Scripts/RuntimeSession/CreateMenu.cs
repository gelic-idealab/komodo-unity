using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class CreateMenu : MonoBehaviour
    {
        public MenuPlacement menuPlacement;

        public void Awake()
        {
            ColorPickerManager.Init();
        }

        // Note(Brandon): The start function will not be called on a disabled gameObject.
        public void Start()
        {
            if (!menuPlacement)
            {
                throw new UnassignedReferenceException("menuPlacement");
            }

            ColorPickerManager.AssignMenuPlacement(menuPlacement);

            ColorPickerManager.InitListeners();
        }
    }
}