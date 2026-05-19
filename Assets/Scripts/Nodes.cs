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
    private int pendingAdvances = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nodes[0].color = Color.white;
        GameObject puzzle = puzzleSelector();
        //GameObject puzzle = puzzles[0];
        minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzle);
        animator = nodes[0].GetComponent<Animator>();
        prevNode = animator;
        animator.enabled = true;
        Debug.Log(puzzle.name);
        if (pendingAdvances > 0) {
            int pending = pendingAdvances;
            pendingAdvances = 0;
            AdvanceNodes(pending);
        }
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
                ShowWinScreen();
                gameManager.GetComponent<GameManager>().CompleteGame();
            }
            else
            {
                foreach (var p in puzzles) p.SetActive(false);
                nodes[i].color = Color.white;
                prevNode.enabled = false;
                animator = nodes[i].GetComponent<Animator>();
                animator.enabled = true;
                prevNode = animator;
                GameObject puzzle = puzzleSelector();
                minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzle);
            }
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = false;
        }
        if (minigameManager.GetComponent<MinigameManager>().minigameFailure == true)
        {
            gameManager.GetComponent<GameManager>().PuzzleFailed();
            foreach (var p in puzzles) p.SetActive(false);
            GameObject puzzle = puzzleSelector();
            minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzle);
            gameManager.GetComponent<GameManager>().suspicion += 1;
            minigameManager.GetComponent<MinigameManager>().minigameFailure = false;
        }
    }

    void ShowWinScreen() {
        // If winScreen is a child of mapScreen, reparent it first so
        // deactivating mapScreen doesn't kill winScreen's Update loop.
        if (winScreen.transform.IsChildOf(mapScreen.transform))
            winScreen.transform.SetParent(mapScreen.transform.parent, false);
        winScreen.SetActive(true);
        mapScreen.SetActive(false);
    }

    public void AdvanceNodes(int count) {
        if (prevNode == null) {
            pendingAdvances += count;
            for (int j = 0; j < count; j++)
                gameManager.GetComponent<GameManager>().PuzzleCompleted();
            return;
        }
        foreach (var p in puzzles) p.SetActive(false);
        if (prevNode != null) {
            prevNode.Rebind();
            prevNode.Update(0f);
        }
        for (int j = 0; j < count; j++) {
            gameManager.GetComponent<GameManager>().PuzzleCompleted();
            i++;
            if (i >= nodes.Length) {
                ShowWinScreen();
                gameManager.GetComponent<GameManager>().CompleteGame();
                return;
            }
            nodes[i].color = Color.white;
            prevNode.enabled = false;
            animator = nodes[i].GetComponent<Animator>();
            animator.enabled = true;
            prevNode = animator;
        }
        minigameManager.GetComponent<MinigameManager>().startPuzzle(puzzleSelector());
    }

    public GameObject puzzleSelector()
    {
        int randIndex = UnityEngine.Random.Range(0, puzzles.Length);
        return puzzles[randIndex];
    }
}
