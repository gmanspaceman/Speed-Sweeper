using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerConnectionUIManager : MonoBehaviour
{
    public Button leaveGame;
    public Button makeGame;
    public Button connectServer;
    public GameObject gameInfo;

    // Start is called before the first frame update
    private void Awake()
    {
        Networking.OnJoinedGame += JoinedGameView;
        BoardGenerator.OnDroppingGame += DroppedGameView;
        Networking.OnServerConnected += ServerConnected;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    public void ServerConnected()
    {
        connectServer.interactable = false;
    }
    public void JoinedGameView(int gameId)
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
