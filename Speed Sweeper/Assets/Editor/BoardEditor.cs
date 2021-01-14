using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(BoardGenerator))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        
        BoardGenerator board = target as BoardGenerator;
        if (DrawDefaultInspector())
        {
            PlayerPrefs.DeleteKey("Rows");
            PlayerPrefs.DeleteKey("Cols");
            PlayerPrefs.DeleteKey("Mines");
            board.initalizeGameState();
        }
    }
}
