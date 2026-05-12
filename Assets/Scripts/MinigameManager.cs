using TMPro;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public bool minigameSuccess = false;
    public bool minigameFailure = false;
    public bool minigameStart = false;

    public GameObject[] puzzles;
    public GameObject mapNotif;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //foreach (GameObject puzzle in puzzles)
        //{
        //    puzzle.SetActive(false);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (minigameSuccess == true || minigameFailure == true)
        {
            mapNotif.SetActive(true);
        }
        else
        {
            mapNotif.SetActive(false);
        }
    }

    public void startPuzzle(GameObject puzzle)
    {
        puzzle.SetActive(true);
        minigameStart = true;
    }
}
