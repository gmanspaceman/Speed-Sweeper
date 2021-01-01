using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Networking : MonoBehaviour
{
    public const string ipAddr = "34.94.134.79";
    public const int port = 11111;
    public const string eom = "<EOM>";

    public Client client;
    public static TcpClient _tcpClient;
    public static NetworkStream _stream;

    public static event Action OnServerConnected;
    public static event Action OnWaitTurn;
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

    // Start is called before the first frame update
    private void Start()
    {
        OpenServerConnection();
    }
    void OpenServerConnection()
    {
        _tcpClient = new TcpClient(ipAddr, port);
        _stream = _tcpClient.GetStream();

        OnServerConnected?.Invoke();

        //strat a thread to listen
        //Thread t = new Thread(ReadFromServerThread);
        //t.Start();  //this will end up rejecting if too many ppl

        StartCoroutine("ReadFromServerThread");
        StartCoroutine("PingServer");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator PingServer()
    {
        while (true)
        {
            Networking.SendToServer("PING");
            yield return new WaitForSeconds(1);
        }
    }
    public static void SendToServer(string msg)
    {
        try
        {
            msg += eom; //append EOM marker

            // Translate the Message into ASCII.
            byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
            // Send the message to the connected TcpServer. 
            _stream.Write(data, 0, data.Length);
            print("Sent: " + msg);

        }
        catch
        {
            Console.WriteLine("Server must have closed the connection!!!!");
        }
    }
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
            serverData += System.Text.Encoding.ASCII.GetString(buffer, 0, inputBuffer);

            Queue<string> validMessages = new Queue<string>();
            bool debugMsgQueueingAndCarry = false;
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
                    default:

                        //clientResponse = "Hey Device! Your Client ID is: " + clientID.ToString() + "\n";

                        break;

                }
            }
        }
    }

    public static bool ReadFromToServer(ref string rsp)
    {
        try
        {
            if (_stream.DataAvailable)
            {

                byte[] data = new byte[1024];
                // Read the Tcp Server Response Bytes.
                int bytes = _stream.Read(data, 0, data.Length);
                rsp = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                print("Received: " + rsp);

                return true;
            }
            else
            {
                return false;
            }    

        }
        catch
        {
            Console.WriteLine("Server must have closed the connection!!!!");
        }
        return false;
    }

    public void StartClient()
    {

        new Thread(() =>
        {
            Client client = new Client();
            Thread.CurrentThread.IsBackground = true;
            client.Connect(ipAddr, port, "COUNT Hello I'm Device 1...");
        }).Start();


    }

    public class Client
    {
        public void Connect(string server, int port, String message)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(server, port);
                NetworkStream stream = tcpClient.GetStream();

                int count = 0;
                while (count++ < 3)
                {
                    // Translate the Message into ASCII.
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                    // Send the message to the connected TcpServer. 
                    stream.Write(data, 0, data.Length);
                    print("Sent: " +  message);

                    // Bytes Array to receive Server Response.
                    data = new byte[256];
                    string response = string.Empty;
                    // Read the Tcp Server Response Bytes.
                    int bytes = stream.Read(data, 0, data.Length);
                    response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    print("Received: " + response);
                    Thread.Sleep(2000);
                    
                    //yield return new WaitForSeconds(2);
                }
                stream.Close();
                tcpClient.Close();
            }
            finally
            //catch (Exception e)
            {
                //Console.WriteLine("Exception: {0}", e);
                Console.WriteLine("Server must have closed the connection!!!!");
            }
            Console.Read();
        }
    }

}

