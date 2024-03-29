﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class Networking : MonoBehaviour
{
    public const string ipAddr = "34.94.134.79";
    public const int port = 11111;
    public Client client;
    public static TcpClient _tcpClient;
    public static NetworkStream _stream;

    // Start is called before the first frame update
    void Start()
    {
        _tcpClient = new TcpClient(ipAddr, port);
        _stream = _tcpClient.GetStream();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SendToServer(string msg)
    {
        try
        {

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

