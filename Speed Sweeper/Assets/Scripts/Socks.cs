using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Assets.Scripts
{
    class Socks
    {
        private static ClientWebSocket ws = new ClientWebSocket();
        private static UTF8Encoding encoder; // For websocket text message encoding.
        private const UInt64 MAXREADSIZE = 1 * 1024 * 1024;

        // Server address
        private static Uri serverUri;
        public static ConcurrentQueue<String> receiveQueue { get; set; }
        public static BlockingCollection<ArraySegment<byte>> sendQueue { get; set; }
        // Threads
        private static Thread receiveThread { get; set; }
        private static Thread sendThread { get; set; }
        public static async Task GAH()
        {

            Console.WriteLine("Hello World!");
            ws = new ClientWebSocket();

            encoder = new UTF8Encoding();
            receiveQueue = new ConcurrentQueue<string>();
            receiveThread = new Thread(RunReceive);
            //receiveThread.Start();
            sendQueue = new BlockingCollection<ArraySegment<byte>>();
            sendThread = new Thread(RunSend);
            sendThread.Start();

            string server = "ws://" + "34.94.134.79" + ":" + "11111";
            //string server = "ws://" + "localhost" + ":" + "11112";

            serverUri = new Uri(server);

            //await ws.ConnectAsync(serverUri, CancellationToken.None);
            await Connect();
            Send("Hello Server<EOM>");
            //while(true)
            //{
            //    //await Send(ws, "data");
            //    //await Receive(ws);
            //    //Console.WriteLine("Sending: Hello Server");
            //    //Send("Hello Server<EOM>");
            //    Thread.Sleep(500);
            //}

        }

        public static async Task Connect()
        {
            Console.WriteLine("Connecting to: " + serverUri);
            ws.ConnectAsync(serverUri, CancellationToken.None);
            while (IsConnecting())
            {
                Console.WriteLine("Waiting to connect...");
                Task.Delay(50).Wait();
            }
            Console.WriteLine("Connect status: " + ws.State);
        }
        public static bool IsConnecting()
        {
            return ws.State == WebSocketState.Connecting;
        }
        public static bool IsConnectionOpen()
        {
            return ws.State == WebSocketState.Open;
        }
        public static void Send(string message)
        {
            byte[] buffer = encoder.GetBytes(message);
            //Debug.Log("Message to queue for send: " + buffer.Length + ", message: " + message);
            var sendBuf = new ArraySegment<byte>(buffer);
            sendQueue.Add(sendBuf);
        }
        private static async void RunSend()
        {
            Console.WriteLine("WebSocket Message Sender looping.");
            ArraySegment<byte> msg;
            while (true)
            {
                while (!sendQueue.IsCompleted)
                {
                    msg = sendQueue.Take();
                    //Debug.Log("Dequeued this message to send: " + msg);
                    await ws.SendAsync(msg, WebSocketMessageType.Text, true /* is last part of message */, CancellationToken.None);
                }
            }
        }
        private static async Task<string> Receive(UInt64 maxSize = MAXREADSIZE)
        {
            // A read buffer, and a memory stream to stuff unknown number of chunks into:
            byte[] buf = new byte[4 * 1024];
            var ms = new MemoryStream();
            ArraySegment<byte> arrayBuf = new ArraySegment<byte>(buf);
            WebSocketReceiveResult chunkResult = null;
            if (IsConnectionOpen())
            {
                do
                {
                    chunkResult = await ws.ReceiveAsync(arrayBuf, CancellationToken.None);
                    ms.Write(arrayBuf.Array, arrayBuf.Offset, chunkResult.Count);
                    //Debug.Log("Size of Chunk message: " + chunkResult.Count);
                    if ((UInt64)(chunkResult.Count) > MAXREADSIZE)
                    {
                        Console.Error.WriteLine("Warning: Message is bigger than expected!");
                    }
                } while (!chunkResult.EndOfMessage);
                ms.Seek(0, SeekOrigin.Begin);
                // Looking for UTF-8 JSON type messages.
                if (chunkResult.MessageType == WebSocketMessageType.Text)
                {
                    return CommunicationUtils2.StreamToString(ms, Encoding.UTF8);
                }
            }
            return "";
        }
        private static async void RunReceive()
        {
            Console.WriteLine("WebSocket Message Receiver looping.");
            string result;
            try
            {
                while (true)
                {
                    //Debug.Log("Awaiting Receive...");
                    result = await Receive();
                    if (result != null && result.Length > 0)
                    {
                        receiveQueue.Enqueue(result);
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Task.Delay(50).Wait();
                    }
                }
            }
            catch (WebSocketException e)
            {
                Console.WriteLine(e);
            }
        }



        //    static async Task Send(ClientWebSocket socket, string data) =>
        //                        await socket.SendAsync(Encoding.UTF8.GetBytes(data), 
        //                                                WebSocketMessageType.Text, 
        //                                                true,
        //                                                CancellationToken.None);


        //static async Task Receive(ClientWebSocket socket)
        //{
        //    var buffer = new ArraySegment<byte>(new byte[2048]);
        //    do
        //    {
        //        WebSocketReceiveResult result;
        //        using (var ms = new MemoryStream())
        //        {
        //            do
        //            {
        //                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        //                ms.Write(buffer.Array, buffer.Offset, result.Count);
        //            } while (!result.EndOfMessage);

        //            if (result.MessageType == WebSocketMessageType.Close)
        //                break;

        //            ms.Seek(0, SeekOrigin.Begin);
        //            using (var reader = new StreamReader(ms, Encoding.UTF8))
        //                Console.WriteLine(await reader.ReadToEndAsync());
        //        }
        //    } while (true);
        //}

    }
    public static class CommunicationUtils2
    {
        /// <summary>
        /// Converts memory stream into string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="ms">Memory Stream.</param>
        /// <param name="encoding">Encoding.</param>
        public static string StreamToString(MemoryStream ms, Encoding encoding)
        {
            string readString = "";
            if (encoding == Encoding.UTF8)
            {
                using (var reader = new StreamReader(ms, encoding))
                {
                    readString = reader.ReadToEnd();
                }
            }
            return readString;
        }
    }
}
