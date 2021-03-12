using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;
using System;

namespace Komodo.Runtime
{
    public class UndoRedoManager : SingletonComponent<UndoRedoManager>
    {
        public static UndoRedoManager Instance
        {
            get { return ((UndoRedoManager)_Instance); }
            set { _Instance = value; }
        }

        public Stack<Action> savedStrokeActions = new Stack<Action>();

        // Start is called before the first frame update
        void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;
        }

        public void Undo()
        {
            //do not check our stack if we do not have anything in it
            if (savedStrokeActions.Count == 0)
                return;

            //invoke what is on the top stack
            savedStrokeActions.Pop()?.Invoke();

        }


    }
}
