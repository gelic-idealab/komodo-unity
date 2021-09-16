using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class FontSizeAdjuster : MonoBehaviour
    {
        private readonly int[] _sizes = new int[] {
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            14,
            16, // default
            18,
            24,
            28,
            36,
            72,
            80
        };

        private int _currentIndex = 8; // size 16

        public void Increase ()
        {
            if (_currentIndex < _sizes.Length - 1)
            {
                _currentIndex += 1;
            }

            _Apply();
        }

        public void Decrease ()
        {
            if (_currentIndex > 0)
            {
                _currentIndex -= 1;
            }

            _Apply();
        }

        private void _Apply ()
        {
            Debug.Log($"Font size is now {_sizes[_currentIndex]}");

            // TODO(Brandon) - implement this so font size actually changes. 
        }
    }
}