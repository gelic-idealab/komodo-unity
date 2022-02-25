using Komodo.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class MainUIReferences : MonoBehaviour
    {

        //TODO -- rename to KomodoMenu or something more clear? 

        [Header("Player References")]

        [ShowOnly] public GameObject player;

        public Slider playerScaleSlider;

        [Header("Button References")]

        public Button undoButton;

        public Button recenterButton;

        public Text sessionAndBuildText;

        private Canvas mainUICanvas;

        public HoverCursor hoverCursor;

        public Transform cursor;

        public GameObject cursorGraphic;

        public void Start()
        {
            mainUICanvas = GetComponent<Canvas>();

            TrySetPlayerSliderConnections();

            hoverCursor = gameObject.GetComponent<HoverCursor>();

            if (hoverCursor == null)
            {
                throw new System.Exception("You must have a HoverCursor component");
            }

            if (hoverCursor.cursorGraphic == null)
            {
                throw new System.Exception("HoverCursor component does not have a cursorGraphic property");
            }

            cursor = hoverCursor.cursorGraphic.transform; //TODO -- is there a shorter way to say this?

            cursorGraphic = hoverCursor.cursorGraphic.gameObject;
        }

        public void OnBeginHeightCalibrationButtonClicked (HeightCalibration heightCalibration)
        {
            heightCalibration.StartCalibration();
        }

        public void OnEndHeightCalibrationButtonClicked (HeightCalibration heightCalibration)
        {
            heightCalibration.EndCalibration();
        }

        public void OnRecenterButtonClicked (TeleportPlayer telPlayer) 
        {
            telPlayer.SetPlayerPositionToHome2();
        }

        public void TrySetPlayerSliderConnections()
        {
            //turn off our menu if we don't have a ui manager
            if (!UIManager.IsAlive)
            {
                gameObject.SetActive(false);
            }

            player = GameObject.FindWithTag(TagList.player);

            //try get its transform component and set teleportplayer to listen to any changes to the slider and buttons
            if (player)
            {
                if (player.TryGetComponent(out TeleportPlayer telPlayer))
                {
                    recenterButton.onClick.AddListener(() => OnRecenterButtonClicked(telPlayer));
                }
            }

            //conect our canvas with the event system manager if it is present
            if (!EventSystemManager.IsAlive)
            {
                Debug.LogWarning("EventSystemManager is not alive. Not proceeding.");

                return;
            }

            if (!this.cursor)
            {
                Debug.LogWarning("You must assign cursor in MainUIReferences", this);
            }

            EventSystemManager.Instance.canvasesToReceiveEvents.Add(mainUICanvas);

            EventSystemManager.Instance.cursor = this.cursor;
        }
    }
}
