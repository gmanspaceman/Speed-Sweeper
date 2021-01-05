using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Net.WebSockets;
using System.Text;
using Assets.Scripts;
using System.Net.WebSockets;
using WebSocketSharp;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class Networking : MonoBehaviour
{
    public static event Action OnTCPServerConnected;
    public static event Action OnWebSocketServerConnected;
    public static event Action OnWaitTurn;
    public static event Action<float> OnPingPong;
    public static event Action OnGetMidGame;
    public static event Action<string> OnMidGame;
    public static event Action OnYourTurn;
    public static event Action OnRestart;
    public static event Action<int> OnJoinedGame;
    public static event Action<int,int> OnTileClicked;
    public static event Action<int,int, int> OnTileRightClicked;
    public static event Action<string> OnGridRecieve;
    public static event Action<string> OnGameInfo;
    public static event Action<Dictionary<int, int>> OnGameList;

    public const string ipAddr = "34.94.134.79";
    public const int port = 11111;
    public const string eom = "<EOM>";
    public const string wsString = "ws://34.94.134.79:11111";

    /// <summary>
    /// TCP Implementation
    /// </summary>
    //public Client client;
    public static TcpClient _tcpClient;
    public static NetworkStream _stream;
    public Stopwatch pingPong;

    ///// <summary>
    ///// WebSocket Implementation
    ///// </summary>
    ////WebSock sock;
    static bool websock = false;

    //WebSocketSharp.WebSocket ws;

    //private static ClientWebSocket cws = null;

    ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);

    #region WebSocketJSLib Implement
    [DllImport("__Internal")]
    private static extern void Hello(); //test javascript plugin

    [DllImport("__Internal")]
    private static extern void InitWebSocket(string url); //create websocket connection

    [DllImport("__Internal")]
    private static extern int State(); //check websocket state

    [DllImport("__Internal")]
    private static extern void Send(string message); //send message

    [DllImport("__Internal")]
    private static extern void Close(); //close websocket connection
    Queue<string> recvList = new Queue<string>(); //keep receive messages

    string serverDataWS = string.Empty;
    string carryDataWS = string.Empty;

    #endregion

    void Start()
    {

#if UNITY_WEBGL
        websock = true;
#endif
        
        if (websock)
        {
            OpenWebSocketServerConnection();
        }
        else
        {
            OpenTCPServerConnection();
        }

        StartCoroutine("DispatchQueueFromServer");

        pingPong = new Stopwatch();
        StartCoroutine("PingServer");
    }
    void OpenTCPServerConnection()
    {
        _tcpClient = new TcpClient(ipAddr, port);
        _stream = _tcpClient.GetStream();

        OnTCPServerConnected?.Invoke();

        StartCoroutine("EnqueueTCPFromServerThread");
    }
    void OpenWebSocketServerConnection()
    {
        //cws = new ClientWebSocket();
        InitWebSocket(wsString);

        OnWebSocketServerConnected?.Invoke();
        
        //enqueing happens in jslib
    }
    IEnumerator DispatchQueueFromServer()
    {
        while (true)
        {
            if (recvList.Count > 0)
            {         //When our message queue has string coming.
                Dispatch(recvList.Dequeue());    //We will dequeue message and send to Dispatch function.
            }
            yield return null;
        }
    }
    void Dispatch(string msg)
    {
        print("Processing: " + msg);

        string[] parseMsg = msg.Split(',');
        string msgKey = parseMsg[0];
        int gameId = -1;

        switch (msgKey)
        {
            case "JOINED_GAME":
                gameId = int.Parse(parseMsg[1]);
                OnJoinedGame?.Invoke(gameId);

                break;
            case "GAME_LIST":
                Dictionary<int, int> gameList = new Dictionary<int, int>();

                for (int ii = 1; ii < parseMsg.Length; ii = ii + 2)
                {
                    int gameNum = int.Parse(parseMsg[ii]);
                    int numberOfPlayers = int.Parse(parseMsg[ii + 1]);

                    gameList.Add(gameNum, numberOfPlayers);
                }

                OnGameList?.Invoke(gameList);

                break;

            case "TILE_CLICKED":
                int c1 = int.Parse(parseMsg[1]);
                int r1 = int.Parse(parseMsg[2]);
                OnTileClicked?.Invoke(c1, r1);

                break;
            case "TILE_RIGHTCLICKED":
                int c2 = int.Parse(parseMsg[1]);
                int r2 = int.Parse(parseMsg[2]);
                int st = int.Parse(parseMsg[3]);
                OnTileRightClicked?.Invoke(c2, r2, st);

                break;
            case "START_GAME":
                OnGridRecieve?.Invoke(msg.Replace("START_GAME,", ""));

                break;
            case "RESTART":
                OnRestart?.Invoke();

                break;
            case "WAIT_TURN":
                OnWaitTurn?.Invoke();

                break;
            case "GET_MIDGAME":
                OnGetMidGame?.Invoke();

                break;
            case "MID_GAME":
                OnMidGame?.Invoke(msg);

                break;
            case "GAME_INFO":
                OnGameInfo?.Invoke(msg);

                break;
            case "YOUR_TURN":
                OnYourTurn?.Invoke();

                break;
            case "PONG":
                OnPingPong?.Invoke(pingPong.ElapsedMilliseconds);

                break;
            default:

                //clientResponse = "Hey Device! Your Client ID is: " + clientID.ToString() + "\n";

                break;

        }
    }
    IEnumerator PingServer()
    {
        while (true)
        {
            pingPong.Restart();
            Networking.SendToServer("PING");
            yield return new WaitForSeconds(1);
        }
    }
    public static void SendToServer(string msg)
    {
        msg += eom; //append EOM marker
        print("Sent: " + msg);

        try
        {
            if (websock)
            {
                Send(msg); //use jslib send
            }
            else
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);  // Translate the Message into ASCII.
                _stream.Write(data, 0, data.Length);    // Send the message to the connected TcpServer. 
            }
        }
        catch
        {
            Console.WriteLine("Server must have closed the connection!!!!");
        }
    }
    IEnumerator EnqueueTCPFromServerThread()
    {
        Byte[] buffer = new Byte[1024];
        int inputBuffer;
        while (true)
        {
            if (_stream.DataAvailable)
            {
                inputBuffer = _stream.Read(buffer, 0, buffer.Length);
                //recvList.Enqueue(System.Text.Encoding.UTF8.GetString(buffer, 0, inputBuffer));
                RecvString(System.Text.Encoding.UTF8.GetString(buffer, 0, inputBuffer));
            }
            yield return null;
        }
    }
    /// <summary>
    /// depreciated when websockets was added
    /// shared functions
    /// </summary>
    /// <returns></returns>
    IEnumerator ReadFromServerThread()
    {
        Byte[] buffer = new Byte[1024];
        int inputBuffer;
        string serverData = string.Empty;
        string carryData = string.Empty;

        while (true)
        {
            yield return null;

            if (!_stream.DataAvailable)
            {
                continue;
            }

            #region Carry Data
            //will need to just dump carry data if its getting to obigt
            //this implies more message traffice than the loop can keep up with
            //should only happen in a debug enviorment
            serverData = carryData;
            carryData = string.Empty;

            inputBuffer = _stream.Read(buffer, 0, buffer.Length);
            serverData += System.Text.Encoding.UTF8.GetString(buffer, 0, inputBuffer);

            Queue<string> validMessages = new Queue<string>();
            bool debugMsgQueueingAndCarry = true;
            //Carry over
            if (serverData.Contains(eom)) //Find the <EOM> tag
            {
                //lets find a way to store all full messages right now
                //just carry over partial message

                string[] splitInput = serverData.Split(new string[] { eom }, StringSplitOptions.RemoveEmptyEntries);

                if (serverData.EndsWith(eom))
                {
                    //all messages are full
                    foreach (string msg in splitInput)
                    {
                        validMessages.Enqueue(msg.Replace(eom, ""));
                        if(debugMsgQueueingAndCarry)
                            print("FullMsgQueued: " + msg);
                    }
                }
                else
                {
                    //last message in is partial
                    for(int ii = 0; ii < splitInput.Length - 1; ii++)
                    {
                        validMessages.Enqueue(splitInput[ii].Replace(eom, ""));
                        if (debugMsgQueueingAndCarry) 
                            print("FullMsgQueued: " + splitInput[ii]);
                    }
                    carryData = splitInput[splitInput.Length - 1];
                    if (debugMsgQueueingAndCarry) 
                        print("CarryData: " + carryData);
                }
            }
            else //patial packet keep the string and append the next read
            {
                carryData = serverData;

                if (carryData != string.Empty)
                    if (debugMsgQueueingAndCarry) 
                        print("carryData: " + carryData);

                continue;
            }

            if (validMessages.Count == 0)
                continue; // nothing on the valid queue, i dont think it can get here


            ///flush some of the queue if its gettign big?
            ///if there 3 or more eom in the carry data dump gameinfo and gamelist
            ///
            //if(Regex.Matches(carryData, eom).Count > 3)
            //{
            //    Regex.Replace(carryData, "GAME_LIST.+" + eom, string.Empty);
            //}

            #endregion

            //loops through the queue
            while (validMessages.Count != 0)
            {
                serverData = validMessages.Dequeue();

                //Console.WriteLine("{1}: Received: {0}", serverData, Thread.CurrentThread.ManagedThreadId);
                print("Processing: " + serverData);

                string[] parseMsg = serverData.Split(',');
                string msgKey = parseMsg[0];
                int gameId = -1;

                switch (msgKey)
                {
                    case "JOINED_GAME":
                        gameId = int.Parse(parseMsg[1]);
                        OnJoinedGame?.Invoke(gameId);

                        break;
                    case "GAME_LIST":
                        Dictionary<int, int> gameList = new Dictionary<int, int>();

                        for (int ii = 1; ii < parseMsg.Length; ii = ii + 2)
                        {
                            int gameNum = int.Parse(parseMsg[ii]);
                            int numberOfPlayers = int.Parse(parseMsg[ii + 1]);

                            gameList.Add(gameNum, numberOfPlayers);
                        }

                        OnGameList?.Invoke(gameList);

                        break;

                    case "TILE_CLICKED":
                        int c1 = int.Parse(parseMsg[1]);
                        int r1 = int.Parse(parseMsg[2]);
                        OnTileClicked?.Invoke(c1, r1);

                        break;
                    case "TILE_RIGHTCLICKED":
                        int c2 = int.Parse(parseMsg[1]);
                        int r2 = int.Parse(parseMsg[2]);
                        int st = int.Parse(parseMsg[3]);
                        OnTileRightClicked?.Invoke(c2, r2, st);

                        break;
                    case "START_GAME":
                        OnGridRecieve?.Invoke(serverData.Replace("START_GAME,", ""));
                        
                        break;
                    case "RESTART":
                        OnRestart?.Invoke();
                        
                        break;
                    case "WAIT_TURN":
                        OnWaitTurn?.Invoke();

                        break;
                    case "GET_MIDGAME":
                        OnGetMidGame?.Invoke();

                        break;
                    case "MID_GAME":
                        OnMidGame?.Invoke(serverData);

                        break;
                    case "GAME_INFO":
                        OnGameInfo?.Invoke(serverData);

                        break;
                    case "YOUR_TURN":
                        OnYourTurn?.Invoke();

                        break;
                    case "PONG":
                        OnPingPong?.Invoke(pingPong.ElapsedMilliseconds);

                        break;
                    default:
                        break;
                }
            }
        }
    }
    
    void RecvString(string message)
    {
        #region Carry Data
        //will need to just dump carry data if its getting to obigt
        //this implies more message traffice than the loop can keep up with
        //should only happen in a debug enviorment
        serverDataWS = carryDataWS;
        carryDataWS = string.Empty;

        serverDataWS += message;

        bool debugMsgQueueingAndCarry = false;
        //Carry over
        if (serverDataWS.Contains(eom)) //Find the <EOM> tag
        {
            //lets find a way to store all full messages right now
            //just carry over partial message

            string[] splitInput = serverDataWS.Split(new string[] { eom }, StringSplitOptions.RemoveEmptyEntries);

            if (serverDataWS.EndsWith(eom))
            {
                //all messages are full
                foreach (string msg in splitInput)
                {
                    recvList.Enqueue(msg.Replace(eom, ""));
                    if (debugMsgQueueingAndCarry)
                        print("FullMsgQueued: " + msg);
                }
            }
            else
            {
                //last message in is partial
                for (int ii = 0; ii < splitInput.Length - 1; ii++)
                {
                    recvList.Enqueue(splitInput[ii].Replace(eom, ""));
                    if (debugMsgQueueingAndCarry)
                        print("FullMsgQueued: " + splitInput[ii]);
                }
                carryDataWS = splitInput[splitInput.Length - 1];
                if (debugMsgQueueingAndCarry)
                    print("CarryData: " + carryDataWS);
            }
        }
        else //patial packet keep the string and append the next read
        {
            carryDataWS = serverDataWS;

            if (carryDataWS != string.Empty)
                if (debugMsgQueueingAndCarry)
                    print("carryData: " + carryDataWS);
        }
        #endregion
    }

    //For Receive Message, this function was call by plugin, we need to keep this name.
    void ErrorString(string message)
    {
        //We can do the same as RecvString here.
        print("JJSLIB Error: " + message);
    }
}

