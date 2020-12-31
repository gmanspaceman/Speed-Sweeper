﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BoardGenerator : MonoBehaviour
{
    public int seed = 1;
    public int numberOfMines = 5;
    public LayerMask targetMask;
    public LayerMask menuTintMask;
    public Transform TilePrefab;
    public GameObject gameOver;
    public GameObject gameWin;
    public Text score;

    public GameObject ExplosionPreFab;
    GameObject explosion;

    public GameUI gameUI;

    public ButtonListControl serverListControl;
    GameState g;

    [Range(6, 20)]
    public int Rows = 6;
    [Range(6, 25)]
    public int Columns = 6;

    //Tile[,] gameboard;

    Camera viewCamera;

    bool animationActive;
    float animationProgress;
    float mouse1Time;
    float mouse2Time;


    // Start is called before the first frame update
    void Start()
    {
        gameUI = FindObjectOfType<GameUI>();

        Networking.OnJoinedGame += JoinedGame;
        Networking.OnGridRecieve += GridRecieve;
        Networking.OnTileClicked += TileClicked;
        Networking.OnGameList += GameList;
        Networking.OnGameInfo += GameInfo;
        Networking.OnWaitTurn += WaitTurn;
        Networking.OnYourTurn += YourTurn;

        //if single player just call init
        initalizeGameState();

        //else

    }

    public void MakeServerGame()
    {
        //Networking.OpenServerConnection();

        //Send this to a server
        string msgKey = "MAKE_GAME"; //figure out extra tokens

        Networking.SendToServer(msgKey);
        g.myTurn = true;
    }
    public void JoinedGame(int gameId)
    {
        g.gameId = gameId;
        gameUI.gameInfoManager.gameObject.SetActive(true);
    }
    public void GetGameList()
    {
        string msgKey = "GET_GAMES";
        //joining mid game will need more work
        //need to get copy of grid from host to populate

        Networking.SendToServer(msgKey);
    }
    public void GetGameInfo()
    {
        string msgKey = "GAME_INFO";

        Networking.SendToServer(msgKey);
    }
    public void GameInfo(string s)
    {
        string[] data = s.Split(',');

        string gameId = data[1];
        string NumberOfPlayers = data[2];
        string CurrentPlayerTurn = data[3];

        gameUI.gameInfoManager.UpdateGameInfo(gameId, NumberOfPlayers, CurrentPlayerTurn);
    }
    public void GameList(Dictionary<int, int> gameList)
    {
        serverListControl.RemoveAllServerButton();
        //Display the game list so user can choose what to join
        foreach (KeyValuePair<int, int> k in gameList)
        {
            print("Game " + k.Key + " has " + k.Value + " players.");
            serverListControl.AddServerButton(k.Key.ToString(), k.Value);
        }

    }
    public void JoinGame(int gameId)
    {
        //Send this to a server
        string msgKey = "JOIN_GAME"; 
        //joining mid game will need more work
        //need to get copy of grid from host to populate

        string msg = string.Join(",", msgKey, gameId);

        Networking.SendToServer(msg);

        g.myTurn = false;
    }
    public void StartServerGame()
    {
        string msgKey = "START_GAME";   //build the board and then send this
        Networking.SendToServer(g.SerializeInitBoardForServer(msgKey));

        g.myTurn = false;
        //g.gamePhase = GameState.GamePhase.PreGame; //no longer stuck in netconfig
    }
    public void GridRecieve(string s)
    {

        //other person started the game
        //populate your board too
        PopulateBombs(g, s);

        //I think what is happenis 
        //tje board already has it open so it doesn
        //trip update game barod logic;
        //g.gamePhase = GameState.GamePhase.Playing;
        g.UpdateGameState();
    }
    public void ClickTile(Tile t)
    {
        string msgKey = "TILE_CLICKED";

        string msg = string.Join(",", msgKey, t.c_position, t.r_position);
        Networking.SendToServer(msg);

        g.myTurn = false; // only allow 1 move
    }
    public void TileClicked(int c, int r)
    {
        TileWasClicked(g.board[c,r]);
    }
    public void WaitTurn()
    {
        g.myTurn = false;
    }
    public void YourTurn()
    {
        g.myTurn = true;
    }
    public void EndGame()
    {
        string msgKey = "END_GAME";

        string msg = string.Join(",", msgKey, g.gameId);
        Networking.SendToServer(msg);

        g.gameId = -1;
    }
    public void DropGame()
    {
        string msgKey = "DROP_GAME";

        string msg = string.Join(",", msgKey, g.gameId);
        Networking.SendToServer(msg);

        g.gameId = -1;
    }

    public void initalizeGameState()
    {
        numberOfMines = Mathf.Clamp(numberOfMines, 0, Rows * Columns - 1);
        mouse1Time = 0f;
        mouse2Time = 0f;

        g = new GameState(Columns, Rows, numberOfMines);

        buildGameBoard(g); // builds game with no bombs nothing showing
                           //gamestate has the right size but no bombs
                           //lets start with building the game locally and the TX the whole
                           //thing to the server since we cant guarantee the random see
                           //then, for consistency, we will have the server send us the game back
                           //and then we will load that version of the game which should be identical
                           //Can optimize later and not do this

        //intersting, local player update the game state and sends copy to server
        //server sends it to the other player

        if (!animationActive)
            StartCoroutine(AnimateRippleRight(g));
    }



    // Update is called once per frame
    void Update()
    {
        if (animationActive || explosion != null) //dont allow clicking to move anythign right now
            return;
        
        UpdateUI();

        if (g.gamePhase == GameState.GamePhase.NetworkConfig)
        {
            if(g.gameType == GameState.GameType.Solo)
            {
                g.gamePhase = GameState.GamePhase.PreGame;
                g.myTurn = true; //i think this is alreayd true
            }

            //else do server stuff
            //waiting to see player hosts or joins
 
        }
        if (!g.myTurn)
            return;

        //repond to gamestatechagne here for now
        if (g.gamePhase == GameState.GamePhase.Win || 
            g.gamePhase == GameState.GamePhase.Lose)
        { 
            if (g.gameType == GameState.GameType.Multiplayer &&
                g.gameId != -1)
            {
                EndGame();
            }
        }


            //User Input
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            Vector3 v = Input.mousePosition;
            Tile t = GetTileClicked(v);
            string msgKey;

            if (g.gamePhase == GameState.GamePhase.PreGame)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    g.playTime = 0;

                    //buildGameBoard(g); //building new game
                    PopulateBombs(g, t);
                    StartServerGame();

                }
            }
            else if (g.gamePhase == GameState.GamePhase.Win || g.gamePhase == GameState.GamePhase.Lose)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    gameUI.ShowHideGameEnd(GameState.GamePhase.PreGame);
                    initalizeGameState();
                    return;
                }
            }
            else if (g.gamePhase == GameState.GamePhase.Playing)
            {
                AniateClickDownHover();

                mouse1Time = (Input.GetMouseButtonUp(0)) ? Time.time : mouse1Time;
                mouse2Time = (Input.GetMouseButtonUp(1)) ? Time.time : mouse2Time;

                if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1))
                {
                    mouse1Time = Time.time;
                }
                else if (Input.GetMouseButtonUp(1) && Input.GetMouseButton(0))
                {
                    mouse2Time = Time.time;
                }
                else if (Mathf.Abs(mouse2Time - mouse1Time) < 0.25f)
                {
                    TileWasRightAndLeftClicked(t);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    TileWasClicked(t);
                    ClickTile(t);
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    TileWasRightClicked(t);
                }
            }
        }
    }
    public void UpdateUI()
    {
        gameUI.ShowHideGameEnd(g.gamePhase);

        if (g.gamePhase == GameState.GamePhase.Playing)
        {
            g.playTime += Time.deltaTime;
            gameUI.UpdateScore(g.playTime);
        }
        gameUI.UpdateMines(g.numMines - g.totalFlagged);

        gameUI.WhosTurn(g.myTurn);
    }
    public void AniateClickDownHover()
    {
        if (Input.GetMouseButton(0))
        {
            if (g.gamePhase != GameState.GamePhase.Win &&
            g.gamePhase != GameState.GamePhase.Lose)
            {
                Vector3 v = Input.mousePosition;
                Tile t = GetTileClicked(v);

                //if (t == null)
                //    return;

                //unclick everythign that shouldnt be clicked
                for (int c = 0; c < Columns; c++)
                {
                    for (int r = 0; r < Rows; r++)
                    {
                        if (!g.board[c, r].isVisible)
                        {
                            g.board[c, r].makeUnClicked();
                        }
                    }
                }
                //if its a double click hover press neighbors
                if (Input.GetMouseButton(1))
                {
                    foreach (Vector2 n in t.neighbors)
                    {
                        if (g.board[(int)n.x, (int)n.y].tileState != Tile.TileState.Flagged &&
                            g.board[(int)n.x, (int)n.y].tileState != Tile.TileState.Questioned)
                        {
                            g.board[(int)n.x, (int)n.y].makeClicked();
                        }
                    }
                }
                if (t.tileState != Tile.TileState.Flagged &&
                    t.tileState != Tile.TileState.Questioned)
                {
                    t.makeClicked();
                }
            }
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            //This undoes the mous click hover
            foreach (Vector2Int rc in g.boardCoords)
            {
                if (!g.board[rc.x, rc.y].isVisible)
                {
                    g.board[rc.x, rc.y].makeUnClicked();
                }

            }
        }
    }
    public Tile GetTileClicked(Vector3 v)
    {
        Ray ray = Camera.main.ScreenPointToRay(v);
        RaycastHit hit;

        if (gameUI.menuTint.activeSelf)
        {
            //print("Got here");
            //if (Physics.Raycast(ray, out hit, 100000, menuTintMask, QueryTriggerInteraction.Collide))
            //{
            //    print("Got here2");
            //    gameUI.toggleMenu();
            //    return null;
            //}
            return null;
        }
            

        
        if (Physics.Raycast(ray, out hit, 1000, targetMask, QueryTriggerInteraction.Collide))
        {           
            foreach(Vector2Int coord in g.boardCoords)
            { 
                if (g.board[coord.x, coord.y].T.gameObject == hit.transform.gameObject)
                {
                    return g.board[coord.x, coord.y];
                }
            }
        }

        return null;
    }



    //returns true if game is over
    public bool EvaluateGameState()
    {
        g.UpdateGameState();

        gameUI.ShowHideGameEnd(g.gamePhase);

        switch (g.gamePhase)
        {
            case GameState.GamePhase.Lose:
                StartCoroutine(AnimateBombs());
                return false;
            case GameState.GamePhase.Win:                
                return true;
            default:
                break;
        }

        return false;
    }
    IEnumerator AnimateBombs()
    {
        if (explosion == null)
        {
            foreach (Vector2Int coord in g.boardCoords)
            {
                if (g.board[coord.x, coord.y].isBomb && g.board[coord.x, coord.y].isVisible && g.board[coord.x, coord.y].isClicked)
                {
                    //AudioManager.instance.PlayExplosionAt(0.9f * Camera.main.transform.position + g.board[coord.x, coord.y].T.position * 0.1f);
                    AudioManager.instance.PlayExplosionAt(g.board[coord.x, coord.y].T.position);
                    explosion = Instantiate(ExplosionPreFab, g.board[coord.x, coord.y].T.position, Quaternion.identity, transform);
                    Destroy(explosion, 1.5f);
                    //g.board[coord.x, coord.y].isBomb = false;
                }
            }
            yield return new WaitForSeconds(0.2f);
            foreach (Vector2Int coord in g.boardCoords)
            {
                if (g.board[coord.x, coord.y].isBomb && !g.board[coord.x, coord.y].isVisible)
                {
                    g.board[coord.x, coord.y].makeVisible();
                    //g.board[coord.x, coord.y].isBomb = false;
                }

            }
        }

        while (explosion != null)
            yield return null;

        
        
    }

    public void TileWasClicked(Tile t)
    {
        if (t.tileState == Tile.TileState.Opened)
            return;
        //for each click send it to the server
        //either the server verifies and you wait for response
        //or just clicks in and it sends the click to the other user

        if (!t.isBomb && !t.isClicked)
        {
            AudioManager.instance.PlayClickAt(t.T.position);
        }

        t.makeClicked();
        t.makeVisible();

        //Evaluate Game State

        //check if what was clicked was a bomb, if so no need to flood open
        if (EvaluateGameState())
            return;

        //Flood Open
        //check if visibile bomb neighbors >= neighbombns, 
        //if so, click all non bomb non vis neigh 
        if (t.GetVisibleBombNeighbors(g.board) < t.numberOfBombNeighbors)
            return;

        foreach (Vector2 n in t.neighbors)
        {
            TileWasClicked(g.board[(int)n.x, (int)n.y]);
        }
    }
    public void TileWasRightClicked(Tile t)
    {
        if (t.tileState == Tile.TileState.Opened)
            return;

        t.ToggleState();
        EvaluateGameState();
    }

    public void TileWasRightAndLeftClicked(Tile t)
    {
        //print("Tile State: " + t.tileState.ToString() );
        //check if that tile is opened
        if (t.tileState != Tile.TileState.Opened)
            return;

        //check the number of bomb neighborts on that tile clicked
        //count the nighbers for that number of flags to be met, if so open all unflagged
        int numNeighborFlags = 0;
        foreach(Vector2Int v in t.neighbors)
        {
            if (g.board[v.x, v.y].tileState == Tile.TileState.Flagged)
            {
                numNeighborFlags++;
            }
        }
        if(t.numberOfBombNeighbors == numNeighborFlags)
        {
            foreach (Vector2Int v in t.neighbors)
            {
                //open all the unflagged neihgrs and evaluate 
                if (g.board[v.x, v.y].tileState != Tile.TileState.Flagged &&
                    g.board[v.x, v.y].tileState != Tile.TileState.Opened)
                {
                    TileWasClicked(g.board[v.x, v.y]);
                }
            } 
        }
    }

    public void buildGameBoard(GameState _g)
    {
        _g.board = new Tile[_g.col, _g.row];
        //board = new Transform[Columns, Rows];

        //Create parent for Tiles
        string holderName = "TileHolder";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform tileHolder = new GameObject(holderName).transform;
        tileHolder.parent = transform;

        for (int c = 0; c < _g.col; c++)
        {
            for (int r = 0; r < _g.row; r++)
            {

                Vector3 tilePos = new Vector3(-_g.col / 2f + TilePrefab.localScale.x / 2f + c,
                                                1,
                                                -_g.row / 2f + TilePrefab.localScale.z / 2f + r);

                Transform newTile = Instantiate(TilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * 0.9f;
                newTile.parent = tileHolder;
                newTile.name = "Tile c:" + c.ToString() + " r:" + r.ToString();

                Vector3 tmp = newTile.GetComponent<BoxCollider>().size;
                newTile.GetComponent<BoxCollider>().size = new Vector3(tmp.x / 0.9f, tmp.y / 0.9f, tmp.z / 0.9f);

                //board[c, r] = newTile;
                _g.board[c, r] = new Tile(c, r, newTile, _g.col, _g.row);
            }
        }

        //PopulateNeighbors
        foreach (Tile t in _g.board)
        {
            t.countNeighborBombs(_g.board);
        }

        _g.UpdateGameState();
        
    }
    public void PopulateBombs(GameState _g, string syncString)
    {
        string[] bombs = syncString.Split(',');
        int bomb = 0;
        int co = int.Parse(bombs[bombs.Length - 2]);
        int ro = int.Parse(bombs[bombs.Length - 1]);

        for (int c = 0; c < _g.col; c++)
        {
            for (int r = 0; r < _g.row; r++)
            {
                if (bombs[bomb] == "1")
                    _g.board[c, r].makeBomb();

                bomb++;
            }
        }

        //PopulateNeighbors
        foreach (Tile t in _g.board)
        {
            t.countNeighborBombs(_g.board);
        }

        for (int c = 0; c < _g.col; c++)
        {
            for (int r = 0; r < _g.row; r++)
            {
                if (c == co && r == ro)
                {
                    TileWasClicked(_g.board[c, r]);
                    return;
                }

            }
        }

        
    }
    public void PopulateBombs(GameState _g, Tile b)
    {
        b.makeStart();

        for (int i = 0; i < _g.numMines; i++)
        {
            int c = Random.Range(0, _g.col);
            int r = Random.Range(0, _g.row);

            if (!_g.board[c, r].isBomb && !_g.board[c, r].isStart)
            {
                _g.board[c, r].makeBomb();
            }
            else
            {
                i--;
            }
        }

        //PopulateNeighbors
        foreach (Tile t in _g.board)
        {
            t.countNeighborBombs(_g.board);
        }
        TileWasClicked(b);
    }


    IEnumerator AnimateRippleRight(GameState _g)
    {
        float height = 0.1f;
        float speed = 4f;

        animationActive = true;
        animationProgress = 0;

        for (int c = 0; c < _g.col; c++)
        {
            for (int r = 0; r < _g.row; r++)
            {
                animationProgress++;
                StartCoroutine(AnimateSingleTile(_g.board[c, r], height, speed, Vector3.up));
            }
            yield return null; //not controllering delay between tile new col start
        }

        //animation is the numebr of coroutines called
        while (animationProgress != 0)
            yield return null;

        animationActive = false;
    }
    IEnumerator AnimateSingleTile(Tile tile, float height, float speed, Vector3 dir)
    {
        float percent = 0f;
        Vector3 orig = tile.T.position;
        
        while (percent <= 1)
        {
            percent += Time.deltaTime * speed;
            tile.T.position += (percent < 0.5f) ? dir * height : -dir * height;

            yield return null;
        }

        tile.T.position = orig;  

        animationProgress--;
    }

}