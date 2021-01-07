using UnityEngine;
using UnityEngine.UI;

public class GameInfoManager : MonoBehaviour
{
    public Text gameId;
    public Text numPlayer;
    public Text currPlayerTurn;

    void Start()
    {
        //Networking.OnJoinedGame += ShowGameInfo;
        //BoardGenerator.OnDroppingGame += HideGameInfo;
        Networking.OnGameInfo += UpdateGameInfo;
    }
    public void UpdateGameInfo(string s)
    {
        string[] data = s.Split(',');

        string _gameId = data[1];
        string _NumberOfPlayers = data[2];
        string _CurrentPlayerTurn = data[3];
        string _CurrentPlayerTurnName = data[4];

        gameId.text = "Game Id: " + _gameId;
        numPlayer.text = "Number of Players: " + _NumberOfPlayers;
        currPlayerTurn.text = "Current Player Turn: " + _CurrentPlayerTurnName;
    }
}