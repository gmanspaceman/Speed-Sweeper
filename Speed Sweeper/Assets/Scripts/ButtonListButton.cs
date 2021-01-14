using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonListButton : MonoBehaviour
{
    public GameObject buttonText;
    public GameObject gameid;
    public GameObject host;
    public GameObject numPlayers;

    public void SetText(string s)
    {
        buttonText.GetComponent<TextMeshProUGUI>().text = s;
    }
    public void SetComplexText(int _gameId, string _host, int _numPlayers)
    {
        gameid.GetComponent<TextMeshProUGUI>().text = "GAME ID: " + _gameId.ToString();
        host.GetComponent<TextMeshProUGUI>().text = "HOST: " + _host.ToUpper();
        numPlayers.GetComponent<TextMeshProUGUI>().text = "PLAYERS: " + _numPlayers.ToString();
    }
}