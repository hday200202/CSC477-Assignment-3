using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Nodes : MonoBehaviour
{

    public Image[] nodes;
    public GameObject mapScreen;
    public GameObject winScreen;
    public GameObject[] puzzles;
    public GameObject minigameManager;
    public GameObject gameManager;

    private int i = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nodes[0].color = Color.white;
        GameObject puzzle = puzzleSelector();
        puzzle.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (minigameManager.GetComponent<MinigameManager>().minigameSuccess == true)
        {
            i++;
            if (i >= nodes.Length)
            {
                mapScreen.SetActive(false);
                winScreen.SetActive(true);
            }
            else
            {
                nodes[i].color = Color.white;
                GameObject puzzle = puzzleSelector();
                puzzle.SetActive(true);
            }
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = false;
        }
        if (minigameManager.GetComponent<MinigameManager>().minigameFailure == true)
        {
            GameObject puzzle = puzzleSelector();
            puzzle.SetActive(true);
            gameManager.GetComponent<GameManager>().suspicion += 1;
            minigameManager.GetComponent<MinigameManager>().minigameFailure = false;
        }
    }

    public GameObject puzzleSelector()
    {
        int randIndex = UnityEngine.Random.Range(0, puzzles.Length);
        return puzzles[randIndex];
    }
}
