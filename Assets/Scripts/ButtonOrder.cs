using JetBrains.Annotations;
using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOrder : MonoBehaviour
{

    public Button[] buttons;
    public GameObject buttonOrder;
    public GameObject minigameManager;
    public TMP_Text instructions;
    public TMP_Text time_display;

    private int counter;
    private string solutiontxt;
    private int[] solution = {1,2,3,4,5,6};
    private float timer = 12;

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
        if (minigameManager.GetComponent<MinigameManager>().minigameStart == true) {
            counter = 0;
            timer = 12;
            solutiontxt = "";

            for (int i = 0; i < solution.Length; i++)
            {
                int temp = solution[i];
                int randIndex = UnityEngine.Random.Range(i, solution.Length);
                solution[i] = solution[randIndex];
                solution[randIndex] = temp;
                solutiontxt += solution[i] + " ";
            }

            instructions.text = "Order: " + solutiontxt;

            minigameManager.GetComponent<MinigameManager>().minigameStart = false;
        }
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        time_display.text = "Time remaining: " + timer;

        if (timer <= 0)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }

        if (counter == 6)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
    }

    public void Button1()
    {
        if (solution[counter] != 1)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button2()
    {
        if (solution[counter] != 2)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button3()
    {
        if (solution[counter] != 3)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button4()
    {
        if (solution[counter] != 4)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button5()
    {
        if (solution[counter] != 5)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
    public void Button6()
    {
        if (solution[counter] != 6)
        {
            buttonOrder.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
        counter++;
    }
}
