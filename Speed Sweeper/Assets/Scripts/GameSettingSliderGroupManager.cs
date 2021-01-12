using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingSliderGroupManager : MonoBehaviour
{
    public Slider rows;
    public Slider cols;
    public Slider mines;
    public Transform rowsLabel;
    public Transform colsLabel;
    public Transform minesLabel;


    public void SetSLiders (int col, int row, int mine)
    {
        cols.value = col; 
        rows.value = row;
        mines.value = mine;

        rowsLabel.GetComponent<TextMeshProUGUI>().text = "ROWS: " + row;
        colsLabel.GetComponent<TextMeshProUGUI>().text = "COLUMNS: " + col;
        minesLabel.GetComponent<TextMeshProUGUI>().text = "MINES: " + mine;
    }
    public void SetSlidersInteractible(bool val)
    {
        cols.interactable = val;
        rows.interactable = val;
        mines.interactable = val;
    }
}
