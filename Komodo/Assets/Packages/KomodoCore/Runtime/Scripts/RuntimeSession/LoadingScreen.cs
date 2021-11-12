using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class LoadingScreen : MonoBehaviour
    {
        void Start ()
        {
            var image = GetComponentInChildren<Image>();

            image.color = Theme.Instance.primaryColor;

            var text = GetComponentInChildren<Text>();

            text.color = Theme.Instance.tertiaryColor;
        }
    }
}
