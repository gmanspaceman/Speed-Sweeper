using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor (typeof(BoardGenerator))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        
        BoardGenerator board = target as BoardGenerator;
        if (DrawDefaultInspector())
        {
            board.initalizeGameState();
        }
    }
}
