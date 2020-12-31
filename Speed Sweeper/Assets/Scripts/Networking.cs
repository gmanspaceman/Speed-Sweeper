using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;

public class Networking : MonoBehaviour
{
    public const string ipAddr = "34.94.134.79";
    public const int port = 11111;
    public const string eom = "<EOM>";

    public Client client;
    public static TcpClient _tcpClient;
    public static NetworkStream _stream;

    public static event Action OnWaitTurn;
    public static event Action OnYourTurn;
    public static event Action<int> OnJoinedGame;
    public static event Action<int,int> OnTileClicked;
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
            yield return new WaitForSeconds(5);
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

            serverData = carryData;
            carryData = string.Empty;

            inputBuffer = _stream.Read(buffer, 0, buffer.Length);
            serverData += System.Text.Encoding.ASCII.GetString(buffer, 0, inputBuffer);

            //Carry over
            if (serverData.Contains(eom)) //Find the <EOM> tag
            {
                if (!serverData.EndsWith(eom)) //split and store the rest
                {   string[] splitInput = serverData.Split(new string[] { eom }, StringSplitOptions.None);
                    serverData += splitInput[0];
                    carryData = splitInput[1];
                }
            }
            else //patial packet keep the string and append the next read
            {
                carryData = serverData;
                continue;
            }
            serverData = serverData.Replace(eom, "");

            //Console.WriteLine("{1}: Received: {0}", serverData, Thread.CurrentThread.ManagedThreadId);
            print("Received: " + serverData);

            string[] parseMsg = serverData.Split(',');
            string msgKey = parseMsg[0];

            string clientResponse = "";
            int gameId = -1;

            switch (msgKey)
            {
                //case "MADE_GAME":
                //    gameId = int.Parse(parseMsg[1]);
                //    if (OnMadeGame != null)
                //        OnMadeGame(gameId);

                //    break;
                case "JOINED_GAME":
                    gameId = int.Parse(parseMsg[1]);
                    if (OnJoinedGame != null)
                        OnJoinedGame(gameId);

                    break;
                case "GAME_LIST":
                    Dictionary<int, int> gameList = new Dictionary<int, int>();

                    for (int ii = 1; ii < parseMsg.Length; ii = ii +2 )
                    {
                        int gameNum = int.Parse(parseMsg[ii]);
                        int numberOfPlayers = int.Parse(parseMsg[ii+1]);

                        gameList.Add(gameNum, numberOfPlayers);
                    }

                    if (OnGameList != null)
                        OnGameList(gameList);

                    break;

                case "TILE_CLICKED":
                    int c = int.Parse(parseMsg[1]);
                    int r = int.Parse(parseMsg[2]);
                    if (OnTileClicked != null)
                        OnTileClicked(c,r);

                    break;
                case "START_GAME":
                    if (OnGridRecieve != null)
                        OnGridRecieve(serverData.Replace("START_GAME,",""));

                    break;
                case "WAIT_TURN":
                    if (OnWaitTurn != null)
                        OnWaitTurn();

                    break;
                case "GAME_INFO":
                    if (OnGameInfo != null)
                        OnGameInfo(serverData);

                    break;
                case "YOUR_TURN":
                    if (OnYourTurn != null)
                        OnYourTurn();

                    break;
                default:

                    //clientResponse = "Hey Device! Your Client ID is: " + clientID.ToString() + "\n";

                    break;

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

