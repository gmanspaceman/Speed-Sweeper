using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    int clientId;
    int myTurnCount;

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
    bool doubleClick;
    Stopwatch mouse1DownTime;
    Stopwatch mouse1TimeSinceLast;

    public static event Action OnEndGame;
    public static event Action OnDroppingGame;

    // Start is called before the first frame update
    private void Awake()
    {
        Networking.OnJoinedGame += JoinedGame;
        //Networking.OnGridRecieve += GridRecieve;
        //Networking.OnTileClicked += TileClicked;
        //Networking.OnTileRightClicked += TileRightClicked;
        //Networking.OnTileLeftAndRightClicked += TileLeftAndRightClicked;
        Networking.OnGameList += GameList;
       // Networking.OnWaitTurn += WaitTurn;
        Networking.OnGameUpdate += GameUpdate;
        Networking.OnHello += Hello;
        //Networking.OnYourTurn += YourTurn;
        Networking.OnRestart += Restart;
        //Networking.OnGetMidGame += ServerSend_GetMidGame;
        //Networking.OnMidGame += MidGame;
        //Networking.OnTCPServerConnected += ServerSend_GetGameList;
        //Networking.OnWebSocketServerConnected += ServerSend_GetGameList;
        Networking.OnServerConnected += ServerSend_GetGameList;
        Networking.OnServerConnected += ServerSend_WHOAMI;
        Networking.OnGameInfo += UpdateGameInfo;

        OnEndGame += ServerSend_EndGame;
    }
    void Start()
    {
        gameUI = FindObjectOfType<GameUI>();
        mouse1DownTime = new Stopwatch();
        mouse1TimeSinceLast = new Stopwatch();
        mouse1TimeSinceLast.Start();

        clientId = -1;
        myTurnCount = 0;

        //if single player just call init

        initalizeGameState();
        if (PlayerPrefs.GetString("GameMode") == "Solo")
        {
            if (PlayerPrefs.HasKey("LocalSave"))
            {
                g.UnPackMidGameBoardStateForServer(PlayerPrefs.GetString("LocalSave"));
            }
        }
        else if (PlayerPrefs.GetString("GameMode") == "Multi")
        {
            //ServerSend_WHOAMI(true);
            //ServerSend_IAM(PlayerPrefs.GetString("Username","NaN"));

            if (PlayerPrefs.GetString("GameMode_Multi") == "Make")
            {
                ServerSend_MakeServerGame();
            }
            else if (PlayerPrefs.GetString("GameMode_Multi") == "Join")
            {
                ServerSend_JoinGame(PlayerPrefs.GetInt("JoinGame"));
            }
        }
        //if (!animationActive)
        //    StartCoroutine(AnimateRippleRight(g));
    }
    public void initalizeGameState(int _gameId = -1)
    {
        gameUI.ShowHideGameEnd(GameState.GamePhase.PreGame);

        //get saved values from menu screens or whenever
        Rows = (int)PlayerPrefs.GetFloat("Rows", Rows);
        Columns = (int)PlayerPrefs.GetFloat("Cols", Columns);
        numberOfMines = (int)PlayerPrefs.GetFloat("Mines", numberOfMines);

        numberOfMines = Mathf.Clamp(numberOfMines, 0, Rows * Columns - 1);
        mouse1Time = 0f;
        mouse2Time = 1f;

        g = new GameState(Columns, Rows, numberOfMines);

        g.gameId = _gameId;

        buildGameBoard(g); // builds game with no bombs nothing showing
                           //gamestate has the right size but no bombs
                           //lets start with building the game locally and the TX the whole
                           //thing to the server since we cant guarantee the random see
                           //then, for consistency, we will have the server send us the game back
                           //and then we will load that version of the game which should be identical
                           //Can optimize later and not do this

        //intersting, local player update the game state and sends copy to server
        //server sends it to the other player
        
    }
    public void ServerSend_GetGameInfo()
    {
        string msgKey = "GAME_INFO";

        Networking.SendToServer(msgKey);
    }
    public void ServerSend_WHOAMI(bool isConnected)
    {
        string msgKey = "WHOAMI";

        if(isConnected)
            Networking.SendToServer(msgKey);
    }
    public void ServerSend_IAM(string s)
    {
        string msgKey = "I_AM";
        string message = string.Join(",", msgKey, 
                                            s.Trim());

        Networking.SendToServer(message);
    }
    public void Hello(int id)
    {
        clientId = id;
    }
    public void UpdateGameInfo(string s)
    {
        string[] data = s.Split(',');
        string _gameId = data[1];
        string _NumberOfPlayers = data[2];
        string _CurrentPlayerTurn = data[3];
        string _CurrentPlayerTurnName = data[4];

        ////literalyl just here to fix turn desyn issue
        ////maybe im not getting yourturn packet
        //if (clientId == int.Parse(_CurrentPlayerTurn))
        //    myTurnCount++;
        //else
        //    myTurnCount = 0;

        //myTurnCount = myTurnCount > 2 ? 2 : myTurnCount;

        //if (myTurnCount == 2)
        //    g.myTurn = true;
        ////else
        ////    g.myTurn = false;

    }

    public void ServerSend_MakeServerGame()
    {
        //Networking.OpenServerConnection();

        //Send this to a server
        string msgKey = "MAKE_GAME"; //figure out extra tokens

        string makeGame = String.Join(",", msgKey,
                                            Columns,
                                            Rows,
                                            numberOfMines);

        Networking.SendToServer(makeGame);
        myTurnCount = 2;
        g.myTurn = true;
        gameUI.WhosTurn(g.myTurn);
        gameUI.SetTurnBanner(PlayerPrefs.GetString("Username"));
    }
    public void ServerSend_JoinGame(int gameId)
    {
        //Send this to a server
        string msgKey = "JOIN_GAME";
        //joining mid game will need more work
        //need to get copy of grid from host to populate

        string msg = string.Join(",", msgKey, gameId);

        Networking.SendToServer(msg);

        myTurnCount = 0;
        g.myTurn = false;
        gameUI.WhosTurn(g.myTurn);
        gameUI.SetTurnBanner();
    }
    public void JoinedGame(int _gameState, int _CurrentTurnId, string _CurrentTurnName, int _gameId, string[] _raw)
    {
        g.gameId = _gameId;
        //g.gameId = gameId;
        if (_gameState == 1)
        {
            string[] gameUpdate = _raw.Skip(5).ToArray();
            string currentGameState = String.Join(",", gameUpdate);
            //currentGameState = "GAME_UPDATE," + currentGameState;
            //this should not be just the GAME_UPDATE message
            g.UnPackMidGameBoardStateForServer(currentGameState); 
            
        }
        else
        {
            Restart(_gameId, _CurrentTurnId, _CurrentTurnName); //i guess restart, maybe i want to actualyl show a finished game
                                                //or not do anything on a new game(prob nbot)
        }
        //clientId = _clientId; //THIS WSANT CHANGE ON SERVER TO BE GAMESTATE
        //gameUI.gameInfoManager.gameObject.SetActive(true);
    }
    public void GameUpdate(string gameUpdate)
    {
        string[] update = gameUpdate.Split(',');
        string currentPlayerTurnId = update[1];
        string currentPlayerTurnName = update[2];

        //print(gameUpdate);

        string moveUpdate = String.Join(",", update.Skip(3));

        //print(moveUpdate);

        g.UnPackMidGameBoardStateForServer(moveUpdate);


        if (int.Parse(currentPlayerTurnId) == clientId)
            g.myTurn = true;
        else
            g.myTurn = false;

        gameUI.WhosTurn(g.myTurn);
        gameUI.SetTurnBanner(currentPlayerTurnName);
    }
    public void ServerSend_Move()
    {
        g.myTurn = false;
        gameUI.WhosTurn(g.myTurn);
        Networking.SendToServer(g.PackMidGameBoardStateForServer("MOVE"));
        gameUI.SetTurnBanner();
    }
    public void ServerSend_GetGameList(bool isConnected)
    {
        string msgKey = "GET_GAMES";
        //joining mid game will need more work
        //need to get copy of grid from host to populate
        if(isConnected)
            Networking.SendToServer(msgKey);
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
    public void ServerSend_EndGame()
    {
        PlayerPrefs.DeleteKey("LocalSave");
        PlayerPrefs.DeleteKey("LocalSave_Rows");
        PlayerPrefs.DeleteKey("LocalSave_Cols");
        PlayerPrefs.DeleteKey("LocalSave_Mines");
        PlayerPrefs.Save();

        if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
        {
            string msgKey = "END_GAME";

            string msg = string.Join(",", msgKey, g.gameId);
            Networking.SendToServer(msg);

            g.myTurn = true;
            gameUI.WhosTurn(g.myTurn);
            gameUI.SetTurnBanner();
            //g.gameId = -1; //okay so comenting this out then doesnt lock out game in udpate
        }
    }
    public void Restart(int gameid, int clientTurnId, string _CurrentTurnName)
    {
       //gameUI.ShowHideGameEnd(GameState.GamePhase.PreGame);
        initalizeGameState(gameid);

        if (g.gameType == GameState.GameType.Multiplayer && gameid != -1)
        {
            //i think i can do pregame here
            g.gamePhase = GameState.GamePhase.PreGame;

            if (clientTurnId == clientId)
                g.myTurn = true;
            else
                g.myTurn = false;
        }
        else
        {
            g.myTurn = true;
        }
        gameUI.WhosTurn(g.myTurn);
        gameUI.SetTurnBanner(_CurrentTurnName);
        //if (!animationActive)
        //    StartCoroutine(AnimateRippleRight(g));

    }
    public void ServerSend_RestartGame()
    {
        string msgKey = "RESTART";
        Networking.SendToServer(msgKey);
    }
    public void ServerSend_DropGame()
    {
        string msgKey = "DROP_GAME";

        string msg = string.Join(",", msgKey, g.gameId);
        Networking.SendToServer(msg);

        OnDroppingGame?.Invoke();

        //gameUI.gameInfoManager.gameObject.SetActive(false);

        g.gameId = -1;
    }

    



    // Update is called once per frame
    void Update()
    {
        if (animationActive || explosion != null) //dont allow clicking to move anythign right now
            return;
        
        UpdateUI();


        
        if (Input.GetMouseButtonDown(0))
        {
            mouse1DownTime.Restart();

            if (doubleClick || mouse1TimeSinceLast.ElapsedMilliseconds < 250f)
            {
                doubleClick = true;
            }

            mouse1TimeSinceLast.Restart();
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouse1DownTime.Stop();
            doubleClick = false;
        }


        AniateClickDownHover(doubleClick);
        
        
        
        if (g.gamePhase == GameState.GamePhase.NetworkConfig)
        {
            if(g.gameType == GameState.GameType.Solo)
            {
                g.gamePhase = GameState.GamePhase.PreGame;
                g.myTurn = true; //i think this is alreayd true
                gameUI.WhosTurn(g.myTurn);
                gameUI.SetTurnBanner();
            }

            //else do server stuff
            //waiting to see player hosts or joins
            //dont need this now but maybe future
 
        }
        if (!g.myTurn && g.gameId != -1 && (g.gamePhase == GameState.GamePhase.Playing ||
                                            g.gamePhase == GameState.GamePhase.PreGame))
            return;


        
        //print(mouse1DownTime.ElapsedMilliseconds);

        //User Input
        if (Input.GetMouseButtonUp(0) || 
            Input.GetMouseButtonUp(1) || 
            (mouse1DownTime.ElapsedMilliseconds >= 500 && mouse1DownTime.IsRunning) ||
            doubleClick)
        {
            Vector3 v = Input.mousePosition;
            bool found;
            Tile t = GetTileClicked(v, out found);

            if (!found)
                return;

            string msgKey;
            

            if (g.gamePhase == GameState.GamePhase.PreGame)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    g.playTime = 0;

                    //buildGameBoard(g); //building new game
                    PopulateBombs(g, t);

                    //if(g.gameType == GameState.GameType.Multiplayer &&
                    //    g.gameId != -1)
                    //    ServerSend_StartServerGame();

                    if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                        ServerSend_Move();

                }
            }
            else if (g.gamePhase == GameState.GamePhase.Win || 
                     g.gamePhase == GameState.GamePhase.Lose)
            {
                //if game is over clear the local save
                

                if (Input.GetMouseButtonUp(0))
                {
                    //Restart(); //commented out to test havint the server dictact all gamestates

                    if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                        ServerSend_RestartGame();
                    else
                        Restart((int)g.gameId, clientId, "Yours!");
                    
                    return;
                }
            }
            else if (g.gamePhase == GameState.GamePhase.Playing)
            {
                mouse1Time = (Input.GetMouseButtonUp(0)) ? Time.time : mouse1Time;
                mouse2Time = (Input.GetMouseButtonUp(1)) ? Time.time : mouse2Time;

                if (!doubleClick && mouse1DownTime.ElapsedMilliseconds == 0 && !Input.GetMouseButton(1) && !Input.GetMouseButtonUp(1))
                {
                    //print("here2");
                }
                else if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1))
                {
                    mouse1Time = Time.time;
                }
                else if (Input.GetMouseButtonUp(1) && Input.GetMouseButton(0))
                {
                    mouse2Time = Time.time;
                }
                else if ((doubleClick || Mathf.Abs(mouse2Time - mouse1Time) < 0.25f) && mouse1DownTime.ElapsedMilliseconds < 500)
                {
                    //print("here5");
                    bool leftandrigthwilldosomething = false;



                    //add the following here temporarily 
                    //put it somewhere better
                    int numNeighborFlags = 0;
                    foreach (Vector2Int vt in t.neighbors)
                    {
                        if (g.board[vt.x, vt.y].tileState == Tile.TileState.Flagged)
                        {
                            numNeighborFlags++;
                        }
                    }
                    if (t.numberOfBombNeighbors == numNeighborFlags)
                    {
                        foreach (Vector2Int vtt in t.neighbors)
                        {
                            //open all the unflagged neihgrs and evaluate 
                            if (g.board[vtt.x, vtt.y].tileState != Tile.TileState.Flagged &&
                                g.board[vtt.x, vtt.y].tileState != Tile.TileState.Opened)
                            {
                                leftandrigthwilldosomething = true;
                            }
                        }
                    }

                    if (leftandrigthwilldosomething)
                    {
                        //if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                        //    ServerSend_LeftAndRightClickTile(t);

                        TileWasRightAndLeftClicked(t);

                        if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                            ServerSend_Move();
                    }
                }
                else if (Input.GetMouseButtonUp(0) && mouse1DownTime.ElapsedMilliseconds < 500)
                {
                    if (t.tileState != Tile.TileState.Opened)
                    {
                        //if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                        //    ServerSend_ClickTile(t);

                        TileWasClicked(t);

                        if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                            ServerSend_Move();

                    }
                }
                else if (Input.GetMouseButtonUp(1) || (mouse1DownTime.ElapsedMilliseconds >= 500))
                {
                    mouse1DownTime.Reset();

                    if (t.tileState != Tile.TileState.Opened)
                    {
                        //if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                        //    ServerSend_RightClickTile(t, t.tileState);

                        TileWasRightClicked(t);

                        if (g.gameType == GameState.GameType.Multiplayer && g.gameId != -1)
                            ServerSend_Move();
                    }
                }
                //save game locallys
                if (g.gamePhase == GameState.GamePhase.Playing)
                {
                    PlayerPrefs.SetString("LocalSave", g.PackMidGameBoardStateForServer("MOVE"));
                    PlayerPrefs.SetFloat("LocalSave_Rows", g.row);
                    PlayerPrefs.SetFloat("LocalSave_Cols", g.col);
                    PlayerPrefs.SetFloat("LocalSave_Mines", g.numMines);
                    PlayerPrefs.Save();
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
    public void AniateClickDownHover(bool doubleClick)
    {
        if (Input.GetMouseButton(0) || doubleClick)
        {
            if (g.gamePhase != GameState.GamePhase.Win && g.gamePhase != GameState.GamePhase.Lose)
            {
                Vector3 v = Input.mousePosition;
                bool found;
                Tile t = GetTileClicked(v, out found);

                if (!found)
                    return;

                //unclick everythign that shouldnt be clicked
                for (int c = 0; c < Columns; c++)
                {
                    for (int r = 0; r < Rows; r++)
                    {
                        Vector2Int tmp = new Vector2Int(c, r);

                        if((Input.GetMouseButton(1) || doubleClick) && t.neighbors.Contains(tmp))
                        {
                            if (g.board[c,r].tileState != Tile.TileState.Flagged && g.board[c, r].tileState != Tile.TileState.Questioned)
                            {
                                g.board[c, r].makeClicked();
                            }
                        }
                        else if (t.c_position == c && t.r_position == r)
                        {
                            if (t.tileState != Tile.TileState.Flagged && t.tileState != Tile.TileState.Questioned)
                            {
                                t.makeClicked();
                            }
                        }
                        else if (!g.board[c, r].isVisible)
                        {
                            g.board[c, r].makeUnClicked();
                        }
                    }
                }
            }
        }
        else 
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
    public Tile GetTileClicked(Vector3 v, out bool found)
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
            found = false;
            return null;
        }
            

        
        if (Physics.Raycast(ray, out hit, 1000, targetMask, QueryTriggerInteraction.Collide))
        {           
            foreach(Vector2Int coord in g.boardCoords)
            { 
                if (g.board[coord.x, coord.y].T.gameObject == hit.transform.gameObject)
                {
                    found = true;
                    return g.board[coord.x, coord.y];
                }
            }
        }
        found = false;
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
                if (OnEndGame != null)
                    OnEndGame();
                return true;
            case GameState.GamePhase.Win:
                if (OnEndGame != null)
                    OnEndGame();
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

        float width = ((float)Camera.main.orthographicSize * 2f * (float)Camera.main.aspect);
        float height = (float)Camera.main.orthographicSize * 2f;

        print("Board width: " + width);
        print("Tile Width needs to be: " + (float)width / (float)(_g.col + 0));
        float tileScaleW = (float) (width / (float)(_g.col + 2));
        float tileScaleH = (float) (height / (float)(_g.row + 3));

        float tileScale = Mathf.Min(tileScaleW, tileScaleH);

        //Transform scaled = TilePrefab;
        //scaled.localScale = scaled.localScale * tileScale;

        for (int c = 0; c < _g.col; c++)
        {
            for (int r = 0; r < _g.row; r++)
            {
                Vector3 tilePos = (
                                    new Vector3(-(_g.col* tileScale) / 2f + tileScale / 2f + tileScale * (c),
                                                1,
                                                -(_g.row * tileScale) / 2f + tileScale / 2f + tileScale * (r)));

                Transform newTile = Instantiate(TilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90));


                

                //newTile.localScale = Vector3.one * tileScale * 0.9f;

                newTile.localScale = new Vector3(tileScale * 0.9f,
                                                 tileScale * 0.9f,
                                                 tileScale * 0.9f);

                newTile.parent = tileHolder;
                newTile.name = "Tile c:" + c.ToString() + " r:" + r.ToString();

                Vector3 tmp = newTile.GetComponent<BoxCollider>().size;
                newTile.GetComponent<BoxCollider>().size = new Vector3(tmp.x / 0.9f, tmp.y / 0.9f, tmp.z / 0.9f);


                //Vector3 tilePos = new Vector3(-_g.col / 2f + TilePrefab.localScale.x / 2f + c,
                //                                1,
                //                                -_g.row / 2f + TilePrefab.localScale.z / 2f + r);

                //Transform newTile = Instantiate(TilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90));

                //newTile.localScale = Vector3.one *  0.9f;
                //newTile.parent = tileHolder;
                //newTile.name = "Tile c:" + c.ToString() + " r:" + r.ToString();

                //Vector3 tmp = newTile.GetComponent<BoxCollider>().size;
                //newTile.GetComponent<BoxCollider>().size = new Vector3(tmp.x / 0.9f, tmp.y / 0.9f, tmp.z / 0.9f);


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
            int c = UnityEngine.Random.Range(0, _g.col);
            int r = UnityEngine.Random.Range(0, _g.row);

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