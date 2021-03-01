using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class MainUIReferences : MonoBehaviour
    {
        [Header("Player References")]
        [ShowOnly]public GameObject player;
        public Slider playerHeightSlider;
        public Slider playerScaleSlider;
        public Button undoButton;
        public Button recenterButton;


        public Text sessionAndBuildText;

        private Canvas mainUICanvas;

        public void Start()
        {
            mainUICanvas = GetComponent<Canvas>();
            TrySetPlayerSliderConnections();
        }
        public void TrySetPlayerSliderConnections()
        {
            player = GameObject.FindGameObjectWithTag("Player");

            //try get its transform component and set teleportplayer to listen to any changes to the slider and buttons
            if (player)
            {
                if (player.TryGetComponent(out TeleportPlayer telPlayer))
                {
                    playerHeightSlider.onValueChanged.AddListener((val) => telPlayer.UpdatePlayerHeight(val));

                    recenterButton.onClick.AddListener(() => telPlayer.SetPlayerPositionToHome());
                }

            }

            //link up our undo funcion if we have a drawing manager
            //setting isalive will be useful per manager to detect if they exist before attaining references
            if (DrawingInstanceManager.IsAlive)
            {
                undoButton.onClick.AddListener(() => DrawingInstanceManager.Instance.Undo());
            }


            //conect our canvas with the event system manager if it is present
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.canvasesToReceiveEvents.Add(mainUICanvas);
            }

        }
    }
}
