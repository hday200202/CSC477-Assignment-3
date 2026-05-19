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
    public AudioSource audioSrc;
    public AudioClip success;
    public AudioClip failure;


    private string[] wordLibrary = { "kitty", "ready", "stars", "hello", "world", "tacos", "puppy", "fleet", "smart", "trike", "berry", "straw", "river" };
    private string selectedWord;
    private string alreadyContains;
    private int lineCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wordInput.characterLimit = 5;
    }

    private void OnEnable()
    {
        if (minigameManager.GetComponent<MinigameManager>().minigameStart == true)
        {
            selectedWord = wordLibrary[UnityEngine.Random.Range(0, wordLibrary.Length)];
            Debug.Log(selectedWord);

            lineCount = 0;
            alreadyContains = "";
            minigameManager.GetComponent<MinigameManager>().minigameStart = false;

            wordInput.ActivateInputField();
        }
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
                else if (selectedWord.Contains(letterinput[i]))
                {
                    wordspaces[i + lineCount].color = Color.yellow;
                    if (alreadyContains.Contains(letterinput[i]) != true) {
                        alreadyContains += letterinput[i];
                    }
                }
                    wordspaces[i + lineCount].text = letterinput[i].ToString();
            }

            if (input == selectedWord)
            {
                wordle.SetActive(false);
                minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
                audioSrc.PlayOneShot(success);
                boardClear();
            }

            lineCount = lineCount+5;

            if (lineCount >= 25)
            {
                wordle.SetActive(false);
                minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
                audioSrc.PlayOneShot(failure);
                boardClear();
            }

            wordInput.ActivateInputField();
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
