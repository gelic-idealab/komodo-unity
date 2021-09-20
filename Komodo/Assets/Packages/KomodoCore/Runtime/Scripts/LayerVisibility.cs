using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    // A candidate upgrade for KomodoCore. This is more general than BlinderBox.
    public class LayerVisibility : MonoBehaviour
    {
        // The cameras for which the layers should be enabled or disabled.
        public List<Camera> cameras;

        // A list of layer names. Objects in these layers will become invisible 
        // or visible as the layers are enabled and disabled.
        public List<string> layersToToggle;

        public void Start()
        {
            if (layersToToggle.Count == 0)
            {
                Debug.LogWarning("No layersToToggle were specified.");
            }

            if (cameras.Count == 0)
            {
                Debug.LogWarning("No cameras were specified.");
            }
        }

        public void HideLayers()
        {
            for (int cam = 0; cam < cameras.Count; cam += 1)
            {
                for (int layer = 0; layer < layersToToggle.Count; layer += 1)
                {
                    DisableLayer(cameras[cam], layersToToggle[layer]);
                }
            }
        }

        public void ShowLayers()
        {
            for (int cam = 0; cam < cameras.Count; cam += 1)
            {
                for (int layer = 0; layer < layersToToggle.Count; layer += 1)
                {
                    EnableLayer(cameras[cam], layersToToggle[layer]);
                }
            }
        }

        private void EnableLayer(Camera camera, string name)
        {
            camera.cullingMask |= 1 << LayerMask.NameToLayer(name);
        }

        private void DisableLayer(Camera camera, string name)
        {
            camera.cullingMask &= ~(1 << LayerMask.NameToLayer(name));
        }
    }
}