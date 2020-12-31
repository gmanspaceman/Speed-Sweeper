using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

[Serializable]
public class GameState : MonoBehaviour
{
    public enum GamePhase { PreGame, Playing, Paused, Lose, Win, NetworkConfig };
    public enum GameType { Solo, Multiplayer };
    public int totalTiles { get; set; }
    public int totalBombs { get; set; }
    public int totalVisible { get; set; }
    public int totalClicked { get; set; }
    public int totalOpened { get; set; }
    public int totalFlagged { get; set; }
    public int totalQuestioned { get; set; }
    public int tilesExplored { get; set; }
    public int bombsRemaining { get; set; }
    public int round { get; set; }
    public float playTime { get; set; }
    public float gameId { get; set; }
    public bool myTurn { get; set; }
    public GamePhase gamePhase { get; set; }
    public GameType gameType { get; set; }
    public Tile[,] board { get; set; }
    public Vector2Int[] boardCoords { get; set; }

    public int bombsclicked;
    public int col;
    public int row;
    public int numMines;
    //public bool newGame { get; set; }

    public void DebugPrint()
    {
        print("Round: " + round.ToString() + "\n" +
            " Game Phase:" + gamePhase.ToString() + "\n" +
            " Total Tiles:" + totalTiles.ToString() + "\n" +
            " Total Bombs:" + totalBombs.ToString() + "\n" +
            " Total Visible:" + totalVisible.ToString() + "\n" +
            " Total Clicked:" + totalClicked.ToString() + "\n" +
            " Tiles Explored:" + tilesExplored.ToString() + "\n" +
            " Total Opened:" + totalOpened.ToString() + "\n" +
            " Total Flagged:" + totalFlagged.ToString() + "\n" +
            " Total Questioned:" + totalQuestioned.ToString() + "\n" +
            " Bombs Remaining:" + bombsRemaining.ToString() + "\n" +
            " Bombs Clicked:" + bombsclicked.ToString()); ;
    }
    public GameState(int _col, int _row, int _numMines)
    {
        totalTiles = 0;
        totalBombs = 0;
        totalVisible = 0;
        totalClicked = 0;
        totalOpened = 0;
        totalFlagged = 0;
        totalQuestioned = 0;
        tilesExplored = 0;
        bombsRemaining = 0;
        bombsclicked = 0;
        round = 0;
        playTime = 0;
        gamePhase = GamePhase.NetworkConfig;
        gameType = GameType.Multiplayer;
        myTurn = (gameType == GameType.Multiplayer) ? false : true;
        gameId = -1;

        numMines = _numMines;
        //newGame = true;

        col = _col;
        row = _row;

        board = new Tile[col, row];

        boardCoords = new Vector2Int[col*row];
        
        
        for (int c = 0, indexCounter = 0; c < col; c++)
        {
            for (int r = 0; r < row; r++)
            {
                boardCoords[indexCounter++] = new Vector2Int(c, r);
            }
        }
    }
    public byte[] SerializeGameState(GameState gameState)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, gameState);
            return ms.ToArray();
        }
    }
    public GameState DeSerializeGameState(byte[] arrBytes)
    {
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            GameState obj = (GameState)binForm.Deserialize(memStream);
            return obj;
        }
    }

    public string SerializeInitBoardForServer(string msgKey)
    {
        string bomb = msgKey;
        string co = "0", ro = "0";
        for (int c = 0; c < board.GetLength(0); c++)
        {
            for (int r = 0; r < board.GetLength(1); r++)
            {
                bomb += board[c, r].isBomb ? ",1" : ",0";

                if (board[c, r].isStart)
                {
                    co = c.ToString();
                    ro = r.ToString();
                }
            }
        }
        bomb += "," + co;
        bomb += "," + ro;

        return bomb;
    }

    public void UpdateGameState()
    {
        totalTiles = 0;
        totalBombs = 0;
        totalVisible = 0;
        totalClicked = 0;
        totalOpened = 0;
        totalFlagged = 0;
        totalQuestioned = 0;
        tilesExplored = 0;
        bombsRemaining = 0;
        bombsclicked = 0;

        round++;

        for (int c = 0; c< board.GetLength(0); c++)
        {
            for (int r = 0; r< board.GetLength(1); r++)
            {
                if (board[c, r].tileState == Tile.TileState.Opened && !board[c, r].isBomb)
                    tilesExplored++;

                totalTiles++;

                if (board[c, r].isBomb)
                    totalBombs++;
                if (board[c, r].isVisible)
                    totalVisible++;
                if (board[c, r].isClicked)
                    totalClicked++;
                if (board[c, r].tileState == Tile.TileState.Opened)
                    totalOpened++;
                if (board[c, r].tileState == Tile.TileState.Flagged)
                    totalFlagged++;
                if (board[c, r].tileState == Tile.TileState.Questioned)
                    totalQuestioned++;

                if (board[c, r].tileState == Tile.TileState.Opened && board[c, r].isBomb)
                    bombsclicked++;

                if (board[c, r].isBomb && board[c, r].tileState != Tile.TileState.Flagged)
                    bombsRemaining++;

            }
        }
 

        if (bombsclicked > 0)
            gamePhase = GamePhase.Lose;
        else if (bombsRemaining == 0 && (totalClicked + totalBombs) == totalTiles)
            gamePhase = GamePhase.Win;
        else if (totalClicked == 0)
            gamePhase = GamePhase.PreGame;
        else
            gamePhase = GamePhase.Playing;

        //DebugPrint();
    }






}

