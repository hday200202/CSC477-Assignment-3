using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;
using System;

public class Captcha : MonoBehaviour
{
    public Button[] buttons;
    public GameObject captcha;
    public GameObject minigameManager;
    public TMP_Text instructions;

    private static string[] color = { "orange one", "blue one", "green one", "pink one" };
    private static string[] number = {"number 1", "number 2", "number 3", "number 4"};
    private static string[] shape = {"square", "triangle", "hexagon", "circle"};
    private string[][] options = { color, number, shape };
    private string selectedTrait;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttons[0].onClick.AddListener(Button1);
        buttons[1].onClick.AddListener(Button2);
        buttons[2].onClick.AddListener(Button3);
        buttons[3].onClick.AddListener(Button4);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnable()
    {
        string[] selectedList = options[UnityEngine.Random.Range(0, options.Length)];
        selectedTrait = selectedList[UnityEngine.Random.Range(0, selectedList.Length)];
        Debug.Log(selectedTrait);

        instructions.text = "Click the " + selectedTrait + ".";
    }

    private void Button1()
    {
        if (selectedTrait == "orange one")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "number 1")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "square")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
    }
    private void Button2()
    {
        if (selectedTrait == "blue one")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "number 2")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "triangle")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
    }
    private void Button3()
    {
        if (selectedTrait == "green one")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "number 3")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "hexagon")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
    }
    private void Button4()
    {
        if (selectedTrait == "pink one")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "number 4")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else if (selectedTrait == "circle")
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
        else
        {
            captcha.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }
    }
}
