using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NoahsPuzzle : MonoBehaviour
{
    public GameObject[] buttons;
    private string[] solution;
    private GameObject lastClicked;
    private bool succeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        succeed = false;
        lastClicked = null;
        solution = new string[] {"Empty",   "Empty",    "Empty",    "RD",       "LR",
                                 "Empty",   "Empty",    "RD",       "LU",       "Empty",
                                 "LR",      "LR",       "Cross",    "DL",       "Empty",
                                 "Empty",   "Empty",    "UD",       "UD",       "Empty",
                                 "Empty",   "Empty",    "UR",       "NoDownT",  "LR"};
    }
    
    // Update is called once per frame
    void Update()
    {
       Check();
       if (succeed)
        {
            Debug.Log("Beat Puzzle");
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
}
