using UnityEngine;
using HighScore;
using System.Runtime.InteropServices;

public class Game : MonoBehaviour
{
    void Awake()
    {
        HS.Init(this, "Artificial Instinct");
        ClearScores();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public void SubmitScore(String name, int score)
    { HS.SubmitHighScore(this, name, score); }

    public void ClearScores()
    { HS.Clear(this); }
}
