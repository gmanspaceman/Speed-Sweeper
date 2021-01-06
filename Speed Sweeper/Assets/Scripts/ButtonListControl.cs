using UnityEngine;
using UnityEngine.UI;

public class ButtonListControl : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;
    public BoardGenerator boardGen;

    public void AddServerButton(string s, int players)
    {
        GameObject button = Instantiate(buttonTemplate);
        button.SetActive(true);
        button.GetComponent<ButtonListButton>().SetText("Game: " + s + "" + " Players: " + players);
        button.transform.SetParent(buttonTemplate.transform.parent, false);

        button.GetComponent<Button>().onClick.AddListener(() => JoinGameButton(int.Parse(s)));
                   
    }
    public void JoinGameButton(int gameId)
    {
        boardGen.ServerSend_JoinGame(gameId);
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
