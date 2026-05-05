using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Diagnostics.Tracing;
using UnityEngine.UIElements;
using UnityEngine.LowLevelPhysics2D;

public class Wordle : MonoBehaviour
{

    public TMP_InputField wordInput;
    public TMP_Text[] wordspaces;
    public GameObject wordle;
    public GameObject minigameManager;


    private string[] wordLibrary = { "kitty", "ready", "stars", "hello", "world" };
    private string selectedWord;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            string input = wordInput.text;
            wordInput.text = "";

            Debug.Log(input);

            char[] letterinput = input.ToCharArray();
            char[] answerinput = selectedWord.ToCharArray();


            for (int i = 0; i < letterinput.Length; i++)
            {
                if (letterinput[i] == answerinput[i])
                {
                    wordspaces[i+lineCount].color = Color.green;
                }
                else if (selectedWord.Contains(letterinput[i]))
                {
                    wordspaces[i + lineCount].color = Color.yellow;
                }
                    wordspaces[i + lineCount].text = letterinput[i].ToString();
            }

            if (input == selectedWord)
            {
                wordle.SetActive(false);
                minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
            }

            lineCount = lineCount+5;
        }
    }
}
