using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Diagnostics.Tracing;
using UnityEngine.UIElements;
using UnityEngine.LowLevelPhysics2D;
using System.Linq;

public class Wordle : MonoBehaviour
{

    public TMP_InputField wordInput;
    public TMP_Text[] wordspaces;
    public GameObject wordle;
    public GameObject minigameManager;


    private string[] wordLibrary = { "kitty", "ready", "stars", "hello", "world" };
    private string selectedWord;
    private string alreadyContains;
    private int lineCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnEnable()
    {
        selectedWord = wordLibrary[UnityEngine.Random.Range(0, wordLibrary.Length)];
        Debug.Log(selectedWord);

        lineCount = 0;
        alreadyContains = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            string input = wordInput.text;
            wordInput.text = "";

            if (input.Length > 5)
            {
                input = input.Substring(0, 5);
            }
            else if (input.Length < 5)
            {
                for (int i = 0; i < 5-input.Length; i++)
                {
                    input += " ";
                }
            }


            char[] letterinput = input.ToCharArray();
            char[] answerinput = selectedWord.ToCharArray();


            for (int i = 0; i < letterinput.Length; i++)
            {
                if (letterinput[i] == answerinput[i])
                {
                    wordspaces[i+lineCount].color = Color.green;
                    alreadyContains += letterinput[i];
                }
                else if (selectedWord.Contains(letterinput[i]) && alreadyContains.Contains(letterinput[i]) != true)
                {
                    wordspaces[i + lineCount].color = Color.yellow;
                    alreadyContains += letterinput[i];
                }
                    wordspaces[i + lineCount].text = letterinput[i].ToString();
            }

            if (input == selectedWord)
            {
                wordle.SetActive(false);
                minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
                boardClear();
            }

            lineCount = lineCount+5;

            if (lineCount >= 25)
            {
                wordle.SetActive(false);
                minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
                boardClear();
            }
        }
    }

    private void boardClear()
    {
        foreach (TMP_Text word in wordspaces)
        {
            word.text = "";
            word.color = new Color(113,113,113,255);
        }
    }
}
