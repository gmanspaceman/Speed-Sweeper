using UnityEngine;
using UnityEngine.UI;

public class ButtonListControl : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;
    public BoardGenerator boardGen;

    void Start()
    {    
        for (int i = 0; i < 20; i++)
        {
            //GameObject button = Instantiate(buttonTemplate);
            //button.SetActive(true);

            //button.GetComponent<ButtonListButton>().SetText("Button #" + i);

            //button.transform.SetParent(buttonTemplate.transform.parent, false);
        }
    }

    public void AddServerButton(string s, int players)
    {
        GameObject button = Instantiate(buttonTemplate);
        button.SetActive(true);
        button.GetComponent<ButtonListButton>().SetText("Join Game: " + s + ": " + " Players: " + players);
        button.transform.SetParent(buttonTemplate.transform.parent, false);

        button.GetComponent<Button>().onClick.AddListener(() => JoinGameButton(int.Parse(s)));
                   
    }
    public void JoinGameButton(int gameId)
    {
        boardGen.JoinGame(gameId);
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
