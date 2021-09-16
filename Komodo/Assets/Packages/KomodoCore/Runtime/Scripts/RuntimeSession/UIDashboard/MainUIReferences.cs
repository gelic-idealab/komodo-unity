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

        public Button drawButton;

        public Button eraseButton;

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

            if (hoverCursor == null) {
                throw new System.Exception("You must have a HoverCursor component");
            }

            if (hoverCursor.cursorGraphic == null) { 
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

        public void OnDrawButtonFirstClick (PlayerReferences playerRefs)
        {
            playerRefs.drawL.gameObject.SetActive(true); 
            playerRefs.drawR.gameObject.SetActive(true); 
        }

        public void OnDrawButtonSecondClick (PlayerReferences playerRefs) 
        {
            playerRefs.drawL.gameObject.SetActive(false); 
            playerRefs.drawR.gameObject.SetActive(false); 
        }

        public void OnEraseButtonFirstClick (PlayerReferences playerRefs) 
        {
            playerRefs.drawL.Set_DRAW_UPDATE(true); 
            playerRefs.drawR.Set_DRAW_UPDATE(true);
            playerRefs.eraseL.gameObject.SetActive(true); playerRefs.eraseR.gameObject.SetActive(true);
            playerRefs.displayEraserL.SetActive(true); playerRefs.displayEraserR.SetActive(true);
        }

        public void OnEraseButtonSecondClick (PlayerReferences playerRefs)
        {
            playerRefs.drawL.Set_DRAW_UPDATE(false); 
            playerRefs.drawR.Set_DRAW_UPDATE(false);
            playerRefs.eraseL.gameObject.SetActive(false); playerRefs.eraseR.gameObject.SetActive(false);
            playerRefs.displayEraserL.SetActive(false); playerRefs.displayEraserR.SetActive(false);
        }

        public void OnUndoButtonClick ()
        {
            UndoRedoManager.Instance.Undo();
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

                if (player.TryGetComponent(out PlayerReferences playerRefs))
                {
                    //set our references for our draw button
                    Alternate_Button_Function abf = drawButton.GetComponent<Alternate_Button_Function>();

                    if (abf)
                    {
                        abf.onFirstClick.AddListener(() => { 
                            OnDrawButtonFirstClick(playerRefs);
                        });
                        abf.onSecondClick.AddListener(() => { 
                            OnDrawButtonSecondClick(playerRefs);
                        });
                    }

                    //set our references for our erase button
                    Alternate_Button_Function abfErase = eraseButton.GetComponent<Alternate_Button_Function>();

                    if (abfErase)
                    {
                        abfErase.onFirstClick.AddListener(() => {
                            OnEraseButtonFirstClick(playerRefs);
                        });
                        abfErase.onSecondClick.AddListener(() => {
                            OnEraseButtonSecondClick(playerRefs);
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
            {
                undoButton.onClick.AddListener(() => OnUndoButtonClick());
            }
            else
            {
                undoButton.gameObject.SetActive(false);
            }

            //conect our canvas with the event system manager if it is present
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.canvasesToReceiveEvents.Add(mainUICanvas);
                EventSystemManager.Instance.cursor = this.cursor;
            }
        }
    }
}
