using UnityEngine;
using UnityEngine.UI;

public class GameInfoManager : MonoBehaviour
{
    public Text gameId;
    public Text numPlayer;
    public Text currPlayerTurn;

    public void UpdateGameInfo(string _gameId, string _numPlayer, string _currPlayerTurn)
    {
        gameId.text = "Game Id: " + _gameId;
        numPlayer.text = "Number of Players: " + _numPlayer;
        currPlayerTurn.text = "Current Player Turn: " + _currPlayerTurn;
    }
}
