using UnityEngine;
using UnityEngine.UI;

public class ButtonListControl : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;

    public void AddServerButton(string s, int players)
    {
        GameObject button = Instantiate(buttonTemplate);
        button.SetActive(true);
        //button.GetComponent<ButtonListButton>().SetText("Game: " + s + "" + " Players: " + players);
        button.GetComponent<ButtonListButton>().SetComplexText(int.Parse(s),"GMAN", players);
        button.transform.SetParent(buttonTemplate.transform.parent, false);

        button.GetComponent<Button>().onClick.AddListener(() => JoinGameButton(int.Parse(s)));
                   
    }
    public void JoinGameButton(int gameId)
    {
        PlayerPrefs.SetInt("JoinGame", gameId);
        PlayerPrefs.Save();
        //boardGen.ServerSend_JoinGame(gameId);
    }
    public void RemoveAllServerButton()
    {
        ButtonListButton[] bList = buttonTemplate.transform.parent.GetComponentsInChildren<ButtonListButton>();
        foreach (ButtonListButton b in bList)
        {
            Destroy(b.gameObject);
        }
    }
}
