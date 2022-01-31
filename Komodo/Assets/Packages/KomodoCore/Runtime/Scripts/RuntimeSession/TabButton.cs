using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public TabManager tabManager;

        public Image background;

        public UnityEvent onTabSelected;

        public UnityEvent onTabDeselected;

        public List<GameObject> objects;

        public void OnPointerClick(PointerEventData eventData)
        {
            tabManager.OnTabToggled(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabManager.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabManager.OnTabExit(this);
        }

        void Start ()
        {
            background = GetComponent<Image>();

            if (tabManager == null) 
            {
                throw new System.NullReferenceException("tabManager");
            }

            tabManager.Subscribe(this);
        }

        public void Select ()
        {
            onTabSelected.Invoke();

            if (gameObject.name == "Settings") 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.SETTING_TAB);
            }
            if (gameObject.name == "People") 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.PEOPLE_TAB);
            }
            if (gameObject.name == "Interact") 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.INTERACTION_TAB);
            }
            if (gameObject.name == "Create") 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.CREATE_TAB);
            }
        }

        public void Deselect ()
        {
            onTabDeselected.Invoke();
        }
    }
}
