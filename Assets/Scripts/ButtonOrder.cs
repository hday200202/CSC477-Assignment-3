using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOrder : MonoBehaviour
{

    public Button[] buttons;
    public GameObject buttonOrder;
    public GameObject minigameManager;

    private int counter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttons[0].onClick.AddListener(Button1);
        buttons[1].onClick.AddListener(Button2);
        buttons[2].onClick.AddListener(Button3);
        buttons[3].onClick.AddListener(Button4);
        buttons[4].onClick.AddListener(Button5);
        buttons[5].onClick.AddListener(Button6);
    }

    // Update is called once per frame
    public void OnEnable()
    {
        counter = 1;
    }

    public void Update()
    {
        if (counter == 7)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
    }

    public void Button1()
    {
        if (counter != 1)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button2()
    {
        if (counter != 2)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button3()
    {
        if (counter != 3)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button4()
    {
        if (counter != 4)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button5()
    {
        if (counter != 5)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button6()
    {
        if (counter != 6)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
}
