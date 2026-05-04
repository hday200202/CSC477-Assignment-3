using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NoahsPuzzlePiece : MonoBehaviour
{
    public Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //button = GetComponent<Button>();
        ////spriteDeselect = button.GetComponent<Sprite>();
        ////spriteSelect = Resources.Load<Sprite>("PuzzleTempTextures/TPiece/dBlankTSelected");
        ////button.GetComponent<Image>().sprite = spriteSelect;
        //if (button != null)
        //{
        //    button.onClick.AddListener(OnButtonClickAction);
        //}

     

}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClick()
    {
        Debug.Log("The button was clicked!");
    }
}
