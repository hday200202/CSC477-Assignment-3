using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NoahsPuzzle : MonoBehaviour
{
    public GameObject[] buttons;
    public GameObject minigameManager;
    public GameObject slider;
    private string[] solution;
    private GameObject lastClicked;
    private bool succeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        succeed = false;
        lastClicked = null;
        solution = new string[] {"Empty",   "Empty",    "Empty",    "RD",       "LRRed",
                                 "Empty",   "Empty",    "RD",       "LU",       "Empty",
                                 "LR",      "LR",       "Cross",    "DL",       "Empty",
                                 "Empty",   "Empty",    "UD",       "UDGreen",       "Empty",
                                 "Empty",   "Empty",    "UR",       "NoDownT",  "LRGB"};
    }

    // Update is called once per frame
    void Update()
    {
       Check();
       if (succeed)
        {
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
            succeed = false;
            slider.SetActive(false);
            Reset();
            Debug.Log("Completed");

        }
        else
        {
            Debug.Log("Not Yet");
        }
    }

    void Check() {
        succeed = true;
        for (int i = 0; i < buttons.Length; i++) { 
            bool check = buttons[i].CompareTag(solution[i]);
            succeed &= check;
        }
        
    }

    public void Swap(GameObject clicked) {
        if (lastClicked == null)
        {
            lastClicked = clicked;
        }
        else 
        {
            //swap clicked and last clicked
            int lastClickedIndex = Array.IndexOf(buttons, lastClicked);
            int clickedIndex = Array.IndexOf(buttons, clicked);
            Vector3 tempPos = clicked.transform.position;
            clicked.transform.position = lastClicked.transform.position;
            lastClicked.transform.position = tempPos;
            buttons[lastClickedIndex] = clicked;
            buttons[clickedIndex] = lastClicked;
            lastClicked = null;
        }

    }

    void Reset()
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < 21; i++) { 
            int j = rand.Next(25);
            int k = rand.Next(25);
            GameObject tileOne = buttons[j];
            GameObject tileTwo = buttons[k];
            //swap clicked and last clicked
            Vector3 tempPos = tileOne.transform.position;
            tileOne.transform.position = tileTwo.transform.position;
            tileTwo.transform.position = tempPos;
            buttons[k] = tileOne;
            buttons[j] = tileTwo;
        }
    }

}
