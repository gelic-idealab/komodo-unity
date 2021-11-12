using System.Collections;
using System.Collections.Generic;
using Komodo.Utilities;
using UnityEngine;

namespace Komodo.Runtime
{
    public class Theme : SingletonComponent<Theme>
    {
        public static Theme Instance
        {
            get { return ((Theme)_Instance); }
            set { _Instance = value; }
        }

        void Awake ()
        {
            var instance = Instance; // Force-create an instance
        }

        public Color primaryColor = new Color32(0x12, 0x12, 0x12, 0xff);

        public Color secondaryColor = new Color32(0x4A, 0x4A, 0x4A, 0xff);

        public Color tertiaryColor = new Color32(0xED, 0xED, 0xED, 0xff);

        public Color accentColor = new Color32(0x38, 0x33, 0x99, 0xff);

        public Color accentHoverColor = new Color32(0x51, 0x4a, 0xde, 0xff);

        public Color accentActiveColor = new Color32(0x24, 0x21, 0x62, 0xff);

        public Color accentDisabledColor = new Color32(0xba, 0xb9, 0xdb, 0xff);

        public Color accentSelectedColor = new Color32(0x3e, 0x39, 0xaa, 0xff);

        public Color outlineColor = new Color32(0x38, 0x33, 0x99, 0xff);
    }
}
