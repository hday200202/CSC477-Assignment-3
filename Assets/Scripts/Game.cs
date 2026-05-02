using UnityEngine;
using UnityEngine.InputSystem;
using HighScore;

public class Game : MonoBehaviour
{
    [Header("Terminal")]
        public GameObject terminal = null;

    void Awake()
    {
        HS.Init(this, "Artificial Instinct");
        ClearScores();
    }

    void Start() {}

    void Update()
    {
        if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.tKey.wasPressedThisFrame)
            ToggleTerminal();
    }

    void ToggleTerminal()
    {
        if (terminal == null) return;
        bool next = !terminal.activeSelf;
        terminal.SetActive(next);
        var t = terminal.GetComponent<Terminal>();
        if (t != null) t.active = next;
    }

    public void SubmitScore(System.String name, int score)
    { HS.SubmitHighScore(this, name, score); }

    public void ClearScores()
    { HS.Clear(this); }
}
