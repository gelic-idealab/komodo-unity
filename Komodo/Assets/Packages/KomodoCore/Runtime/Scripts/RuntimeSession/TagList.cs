namespace Komodo.Runtime {
    /**
     *  Until we can refactor Komodo to move away from the use of tags, we must 
     * use this list. For single-object tags, we expect only one gameObject 
     * with that tag in the scene. If you have more than one, code elsewhere 
     * may register the wrong object. For multi-object tags, we expect one or 
     * more.
     *
     * If you have no objects tagged with a certain tag, projects created with 
     * KomodoCore will not include that tag. Make sure you include an empty 
     * object with that tag in the main scene, or recommend that users create 
     * it themselves.
     */
    public static class TagList {
        private const string prefix = "kmd"; //TODO(Brandon): implement prefixes for tags so module developers don't have to worry about their own tags being conflated with these.

        // Single-object tags
        public const string cameraSet = "CameraSet";
        public const string player = "Player";
        public const string xrCamera = "XRCamera"; //TODO(Brandon): rename this to playspace
        public const string leftEye = "LeftEye";
        public const string rightEye = "RightEye";
        public const string desktopCamera = "DesktopCamera";
        public const string hands = "Hands";
        public const string handL = "hand_L";
        public const string handR = "hand_R";
        public const string drawing = "Drawing";
        public const string menuUI = "MenuUI";

        // Multi-object tags
        public const string interactable = "Interactable"; 
        public const string playerSpawnCenter = "PlayerSpawnCenter";


        //TODO(David): review the below. Do we use these tags at all?
        public const string uiInteractable = "UIInteractable";
        public const string userIgnore = "UserIgnore";
        public const string tool = "Tool";
        public const string vrUIIcon = "VRUIIcon";
        public const string vrUIText = "VRUIText";
        public const string vrUIButtonControlBar = "VRUIButtonControlBar";
        public const string physics = "Physics";
        public const string eventSystemDesktop = "EventSystemDesktop";
        public const string hand = "Hand";
    }
}