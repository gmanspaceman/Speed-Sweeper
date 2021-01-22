﻿using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ServerConnectionUIManager : MonoBehaviour
{
    public Button leaveGame;
    public Button makeGame;
    public Button connectServer;
    public GameObject gameInfo;
    public Text ping;
    public Text isConnected;
    public Text nameInput;

   // Stopwatch timer;
    // Start is called before the first frame update
    private void Awake()
    {
        Networking.OnJoinedGame += JoinedGameView;
        BoardGenerator.OnDroppingGame += DroppedGameView;
        //Networking.OnTCPServerConnected += ServerConnected;
        Networking.OnServerConnected += ServerConnected;
        Networking.OnPingPong += UpdatePing;
    }
    public void Start()
    {
       // timer = new Stopwatch();
        //timer.Start();
    }
    public void Update()
    {
        
    }
    public void UpdatePing(float f)
    {
        ping.text = "Ping: " + f + " ms";
    }
    public void UpdateIAm()
    {
        if (nameInput.text.Trim().Length > 0)
        {
            string msgKey = "I_AM";
            string message = string.Join(",", msgKey, nameInput.text.Trim());

            Networking.SendToServer(message);
        }

    }
    // Update is called once per frame
    public void ServerConnected(bool _isConnected)
    {
        connectServer.interactable = !_isConnected;
        isConnected.text = _isConnected ? "Connected! :)" : "Disconnected! :(";
    }
    public void JoinedGameView(int gameState, int clientTurnId, string clientTurnName, int gameId, int _col, int _row, int _mines, string[] raw)
    {
        leaveGame.interactable = true;
        makeGame.interactable = false;
        gameInfo.SetActive(true);
    }
    public void DroppedGameView()
    {
        leaveGame.interactable = false;
        makeGame.interactable = true;
        gameInfo.SetActive(false);
    }
}
