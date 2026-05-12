using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
    private Animator animator;
    private Animator prevNode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nodes[0].color = Color.white;
        //GameObject puzzle = puzzleSelector();
        GameObject puzzle = puzzles[0];
        minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzle);
        animator = nodes[0].GetComponent<Animator>();
        prevNode = animator;
        animator.enabled = true;
        Debug.Log(puzzle.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (minigameManager.GetComponent<MinigameManager>().minigameSuccess == true)
        {
            gameManager.GetComponent<GameManager>().PuzzleCompleted();
            i++;
            if (i >= nodes.Length)
            {
                mapScreen.SetActive(false);
                winScreen.SetActive(true);
                gameManager.GetComponent<GameManager>().CompleteGame();
            }
            else
            {
                prevNode.enabled = false;
                nodes[i].color = Color.white;
                animator = nodes[i].GetComponent<Animator>();
                animator.enabled = true;
                prevNode = animator;
                GameObject puzzle = puzzleSelector();
                minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzle);
                puzzle.SetActive(true);
            }
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = false;
        }
        if (minigameManager.GetComponent<MinigameManager>().minigameFailure == true)
        {
            gameManager.GetComponent<GameManager>().PuzzleFailed();
            GameObject puzzle = puzzleSelector();
            minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzle);
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
