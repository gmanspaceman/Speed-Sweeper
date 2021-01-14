using UnityEngine;
using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Networking : MonoBehaviour
{
    public static event Action<bool> OnServerConnected;
    public static event Action<float> OnPingPong;
    public static event Action<int> OnHello;
    public static event Action<string> OnGameUpdate;
    public static event Action<int, int, string> OnRestart;
    public static event Action<int, int, string, int, string[]> OnJoinedGame;
    public static event Action<string> OnGameInfo;
    public static event Action<Dictionary<int, int>> OnGameList;

    public const string ipAddr = "34.94.134.79";
    public const int port = 11111;
    public const string eom = "<EOM>";
    public const string wsString = "ws://34.94.134.79:11111";

    public static TcpClient _tcpClient;
    public static NetworkStream _stream;
    public Stopwatch pingPong;
    public float pingTime { get; set; }
    public bool isConnected = false;

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

    //ServerConnectionUIManager.OnConnectClicked += Reconnect;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void MultiStart()
    {

        //OnTCPServerConnected?.Invoke(false);
        //OnWebSocketServerConnected?.Invoke(false);
        OnServerConnected?.Invoke(false);
        
        //if(PlayerPrefs.GetString("GameMode") == "Multi")
            Connect();
    }
    public void Reconnect()
    {
        Disconnect();
        Connect();
    }
    public void Connect()
    {
#if UNITY_WEBGL
        StartCoroutine("OpenWebSocketServerConnection");
#else
        StartCoroutine("OpenTCPServerConnection");
#endif

        pingPong = new Stopwatch();
        StartCoroutine("PingServer");
        StartCoroutine("DispatchQueueFromServer");
    }
    public void Disconnect()
    {
        StopAllCoroutines();
        pingPong.Reset();

#if UNITY_WEBGL
        Close();
#else
        _stream.Close();
        _tcpClient.Close();
#endif
        //OnTCPServerConnected?.Invoke(false);
        //OnWebSocketServerConnected?.Invoke(false);
        OnServerConnected?.Invoke(false);
        
    }
    IEnumerator OpenTCPServerConnection()
    {
        _tcpClient = new TcpClient(ipAddr, port);
        _stream = _tcpClient.GetStream();
        if(!_tcpClient.Connected)
        {
            //OnTCPServerConnected?.Invoke(false);
            yield return new WaitForSeconds(1);
        }
        StartCoroutine("EnqueueTCPFromServerThread");
        //OnTCPServerConnected?.Invoke(true);
        OnServerConnected?.Invoke(true);
        isConnected = true;
    }
    IEnumerator OpenWebSocketServerConnection()
    {
        InitWebSocket(wsString);
        while (State() != 1)
        {
            //OnWebSocketServerConnected?.Invoke(false);
            yield return new WaitForSeconds(1); //wait till it connects before throwing the event
        }
        //OnWebSocketServerConnected?.Invoke(true);
        OnServerConnected?.Invoke(true);
        isConnected = true;
        //enqueing happens in jslib
    }
    IEnumerator DispatchQueueFromServer()
    {
        while (true)
        {
            if (recvList.Count != 0)
            {         //When our message queue has string coming.
                Dispatch(recvList.Dequeue());
            }
            yield return null;
        }
    }
    void Dispatch(string msg)
    {
        if (!msg.Contains("PONG"))
            print("Recv: " + msg);

        string[] parseMsg = msg.Split(',');
        string msgKey = parseMsg[0];
        int gameId = -1;
        switch (msgKey)
        {
            case "GAME_UPDATE":

                OnGameUpdate?.Invoke(msg);
                break;
            case "HELLO":
                int id = int.Parse(parseMsg[1]);
                OnHello?.Invoke(id);
                break;
            case "JOINED_GAME":
                int gameState = int.Parse(parseMsg[1]);
                int currentTurnId = int.Parse(parseMsg[2]);
                string currentTurnName = parseMsg[3];
                gameId = int.Parse(parseMsg[4]);

                OnJoinedGame?.Invoke(gameState, currentTurnId, currentTurnName, gameId, parseMsg);
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
            //case "TILE_CLICKED":
            //    int c1 = int.Parse(parseMsg[1]);
            //    int r1 = int.Parse(parseMsg[2]);
                
            //    OnTileClicked?.Invoke(c1, r1);
            //    break;
            //case "TILE_LEFTANDRIGHTCLICKED":
            //    int c3 = int.Parse(parseMsg[1]);
            //    int r3 = int.Parse(parseMsg[2]);
                
            //    OnTileLeftAndRightClicked?.Invoke(c3, r3);
            //    break;
            //case "TILE_RIGHTCLICKED":
            //    int c2 = int.Parse(parseMsg[1]);
            //    int r2 = int.Parse(parseMsg[2]);
            //    int st = int.Parse(parseMsg[3]);
                
            //    OnTileRightClicked?.Invoke(c2, r2, st);
            //    break;
            //case "START_GAME":
                
            //    OnGridRecieve?.Invoke(msg.Replace("START_GAME,", ""));
            //    break;
            case "RESTART":
                gameId = int.Parse(parseMsg[1]);
                int clientId = int.Parse(parseMsg[2]);
                currentTurnName = parseMsg[3];

                OnRestart?.Invoke(gameId, clientId, currentTurnName);
                break;
            //case "WAIT_TURN":
                
            //    OnWaitTurn?.Invoke();
            //    break;
            //case "GET_MIDGAME":
                
            //    OnGetMidGame?.Invoke();
            //    break;
            //case "MID_GAME":
                
            //    OnMidGame?.Invoke(msg);
            //    break;
            case "GAME_INFO":
                
                OnGameInfo?.Invoke(msg);
                break;
            //case "YOUR_TURN":
                
            //    OnYourTurn?.Invoke();
            //    break;
            case "PONG":
                pingTime = pingPong.ElapsedMilliseconds;
                OnPingPong?.Invoke(pingPong.ElapsedMilliseconds);
                pingPong.Reset();
                break;
            default:

                break;
        }
    }
    IEnumerator PingServer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            pingPong.Restart();
            Networking.SendToServer("PING");
            
            if (pingPong.ElapsedMilliseconds < 5000 && !isConnected)
            {
                isConnected = true;
                //OnTCPServerConnected?.Invoke(true);
                //OnWebSocketServerConnected?.Invoke(true);
                OnServerConnected?.Invoke(true);
            }
            if (pingPong.ElapsedMilliseconds > 5000 && isConnected)
            {
                isConnected = false;
                //OnTCPServerConnected?.Invoke(false);
                //OnWebSocketServerConnected?.Invoke(false);
                OnServerConnected?.Invoke(false);
            }
        }
    }
    public static void SendToServer(string msg)
    {
        msg += eom; //append EOM marker
        if(!msg.Contains("PING"))
            print("Sent: " + msg);

        try
        {

#if UNITY_WEBGL
            if (State() == 1)
                Send(msg); //use jslib send
#else
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);  // Translate the Message into ASCII.
            _stream.Write(data, 0, data.Length);    // Send the message to the connected TcpServer. 
#endif
        }
        catch
        {
            Console.WriteLine("Server must have closed the connection!!!!");
        }
    }
    IEnumerator EnqueueTCPFromServerThread()
    {
        Byte[] buffer = new Byte[4 * 1024];
        int inputBuffer;
        while (true)
        {
            if (_stream.DataAvailable)
            {
                inputBuffer = _stream.Read(buffer, 0, buffer.Length);
                RecvString(System.Text.Encoding.UTF8.GetString(buffer, 0, inputBuffer));
            }
            yield return null;
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
    public void ServerSend_WHOAMI()
    {
        string msgKey = "WHOAMI";

        if (isConnected)
            SendToServer(msgKey);
    }
    public void ServerSend_IAM(string s)
    {
        string msgKey = "I_AM";
        string message = string.Join(",", msgKey,
                                            s.Trim());

        SendToServer(message);
    }
    public void ServerSend_GetGameList()
    {
        string msgKey = "GET_GAMES";
        //joining mid game will need more work
        //need to get copy of grid from host to populate
        if (isConnected)
            SendToServer(msgKey);
    }
}

