using Komodo.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class MainUIReferences : MonoBehaviour
    {
        [Header("Player References")]
        [ShowOnly] public GameObject player;
        public Slider playerHeightSlider;
        public Slider playerScaleSlider;

        [Header("Button References")]
        public Button undoButton;
        public Button recenterButton;
        public Button drawButton;
        public Button eraseButton;

        public Text sessionAndBuildText;

        private Canvas mainUICanvas;

        public Transform cursor;

        public void Start()
        {
            mainUICanvas = GetComponent<Canvas>();

            TrySetPlayerSliderConnections();
        }

        //capture the eventsystem cursor since it does not detect it when deactivating and activating the menu
        public void onEnable()
        {
          //  cursor.a

        }
        public void TrySetPlayerSliderConnections()
        {
            //turn off our menu if we don't have a ui manager
            if (!UIManager.IsAlive)
                gameObject.SetActive(false);


            player = GameObject.FindGameObjectWithTag("Player");

            //try get its transform component and set teleportplayer to listen to any changes to the slider and buttons
            if (player)
            {
                if (player.TryGetComponent(out TeleportPlayer telPlayer))
                {
                    playerHeightSlider.onValueChanged.AddListener((val) => telPlayer.UpdatePlayerHeight(val));

                    recenterButton.onClick.AddListener(() => telPlayer.SetPlayerPositionToHome());
                }

                if (player.TryGetComponent(out PlayerReferences playerRefs))
                {
                    //set our references for our draw button
                    Alternate_Button_Function abf = drawButton.GetComponent<Alternate_Button_Function>();

                    if (abf)
                    {
                        abf.onFirstClick.AddListener(() => { playerRefs.drawL.gameObject.SetActive(true); playerRefs.drawR.gameObject.SetActive(true); });
                        abf.onSecondClick.AddListener(() => { playerRefs.drawL.gameObject.SetActive(false); playerRefs.drawR.gameObject.SetActive(false); });
                    }

                    //set our references for our erase button
                    Alternate_Button_Function abfErase = eraseButton.GetComponent<Alternate_Button_Function>();

                    if (abfErase)
                    {
                        abfErase.onFirstClick.AddListener(() => {

                            playerRefs.drawL.Set_DRAW_UPDATE(true); playerRefs.drawR.Set_DRAW_UPDATE(true);
                            playerRefs.eraseL.gameObject.SetActive(true); playerRefs.eraseR.gameObject.SetActive(true);
                            playerRefs.displayEraserL.SetActive(true); playerRefs.displayEraserR.SetActive(true);

                        });
                        abfErase.onSecondClick.AddListener(() => {

                            playerRefs.drawL.Set_DRAW_UPDATE(false); playerRefs.drawR.Set_DRAW_UPDATE(false);
                            playerRefs.eraseL.gameObject.SetActive(false); playerRefs.eraseR.gameObject.SetActive(false);
                            playerRefs.displayEraserL.SetActive(false); playerRefs.displayEraserR.SetActive(false);

                        });
                    }
                }

            }

            //link up our undo funcion if we have a drawing manager
            //setting isalive will be useful per manager to detect if they exist before attaining references
            if (DrawingInstanceManager.IsAlive)
            {
                
                drawButton.gameObject.SetActive(true);

            }
            else
            {
                drawButton.gameObject.SetActive(false);
            }

            if (UndoRedoManager.IsAlive)
                undoButton.onClick.AddListener(() => UndoRedoManager.Instance.Undo());
            else
                undoButton.gameObject.SetActive(false);


            //conect our canvas with the event system manager if it is present
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.canvasesToReceiveEvents.Add(mainUICanvas);
                EventSystemManager.Instance.cursor = this.cursor;
            }

          

        }
    }
}
