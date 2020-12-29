using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public enum TileState { Unmarked, Flagged, Questioned, Opened };

    public int c_position;
    public int r_position;
    public Vector2 pos;
    public bool isVisible;
    public bool isClicked;
    public bool isBomb;
    public bool showAnswer { get; set; }
    public bool showColor { get; set; }

    public TileState tileState;


    public bool isStart;
    public int numberOfBombNeighbors;
    public Color color;
    public string tileText;
    public Transform T;
    public List<Vector2Int> neighbors;

    private static float moveHeight = 0.25f;

    public Tile(int _c, int _r,Transform _transform, int mainBoardCol, int mainBoardRow)
    {
        c_position = _c;
        r_position = _r;
        pos = new Vector2(_c, _r);
        isVisible = false;
        isClicked = false;
        isBomb = false;
        tileState = TileState.Unmarked;
        isStart = false;
        numberOfBombNeighbors = 0;
        color = Color.cyan;
        T = _transform;
        tileText = "";
        neighbors = new List<Vector2Int>();
        showAnswer = false;
        showColor = false;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if ((_c + x) >= 0 &&
                    (_c + x) < mainBoardCol &&
                    (_r + y) >= 0 &&
                    (_r + y) < mainBoardRow)
                {
                    neighbors.Add(new Vector2Int(_c + x, _r + y));
                }
            }
        }
        


    }
    public void UpdateTile()
    {
        UpdateText();
        UpdateColor();
        updateState();
    }
    public void ToggleState()
    {
        switch (tileState)
        {
            case TileState.Unmarked:
                tileState = TileState.Flagged;
                break;
            case TileState.Flagged:
                tileState = TileState.Unmarked;
                break;
            case TileState.Questioned:
                tileState = TileState.Unmarked;
                break;
            case TileState.Opened:
                break;
        }

        UpdateTile();
    }
    public void updateState()
    {
        if (tileState == TileState.Flagged)
        {
            T.gameObject.GetComponent<TileManager>().showSphere(true);
        }
        else
        {
            T.gameObject.GetComponent<TileManager>().showSphere(false);
        }
    }
    public void makeVisible()
    {
        isVisible = true;
        tileState = TileState.Opened;
        showAnswer = true;
        showColor = true;



        UpdateTile();
    }
    public void makeBomb()
    {
        isBomb = true;
        //color = Color.red;
        tileText = "X";
        UpdateTile();
    }
    public void makeStart()
    {
        isStart = true;
        //color = Color.blue;
        UpdateTile();
    }
    /// <summary>
    /// just does the snimation of going down now
    /// </summary>
    public void makeClicked()
    {
        if (!isClicked)
        {
            isClicked = true;
            T.position = T.position + Vector3.down * moveHeight;

            //Vector3 tmp = GetComponent<BoxCollider>().size;
            //GetComponent<BoxCollider>().size = new Vector3(tmp.x, tmp.y, 1f);

            T.gameObject.GetComponent<TileManager>().DisplayedClickedState(true);
            UpdateTile();
        }
    }
    public void makeUnClicked()
    {
        if (isClicked)
        {
            isClicked = false;
            T.position = T.position + Vector3.up * moveHeight;

            //Vector3 tmp = GetComponent<BoxCollider>().size;
            //GetComponent<BoxCollider>().size = new Vector3(tmp.x, tmp.y, 0f);
            T.gameObject.GetComponent<TileManager>().DisplayedClickedState(false);
            UpdateTile();
        }
    }
    public void UpdateColor()
    {
        Material m = T.GetComponent<Renderer>().material;

        color = showColor && isBomb ? Color.red : Color.cyan;

        if (isVisible)
        {
            if (isBomb)
            {
                color = Color.red;
            }
            //else if (isStart)
            //{
            //    color = Color.blue;
            //}
            else
            {
                color = Color.green;
            }
        }

        m.color = color;

    }
    public void UpdateText()
    {
        T.GetComponentInChildren<TextMeshPro>().text = showAnswer ? tileText : "";
    }

    public void countNeighborBombs(Tile[,] board)
    {
        numberOfBombNeighbors = 0;

        for (int i = -1; i <= 1; i++)
            for (int y = -1; y <= 1; y++)
                if (this.c_position + i < board.GetLength(0) && this.c_position + i >= 0 && this.r_position + y < board.GetLength(1) && this.r_position + y >= 0)
                    if (!(i == 0 && y == 0))
                        if (board[this.c_position + i, this.r_position + y].isBomb)
                            numberOfBombNeighbors++;

        tileText = this.isBomb ? "X" : numberOfBombNeighbors.ToString().Replace("0","");

        UpdateTile();
    }

    public int GetVisibleBombNeighbors(Tile[,] board)
    {
        int visibleBombNeighbors = 0;

        foreach (Vector2Int n in neighbors)
        {
            if (board[n.x, n.y].isVisible && 
                board[n.x, n.y].isBomb)
                visibleBombNeighbors++;
        }
        return visibleBombNeighbors;
    }
    public static bool operator ==(Tile c1, Tile c2)
    {
        return c1.c_position == c2.c_position && c1.r_position == c2.r_position;
    }

    public static bool operator !=(Tile c1, Tile c2)
    {
        return !(c1 == c2);
    }

}

