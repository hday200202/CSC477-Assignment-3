using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HighScore;

public class GameManager : MonoBehaviour {
    public GameObject[] screens;
    public GameObject terminal = null;

    public int suspicion;

    void Awake() {
        screens[0].SetActive(true);
        for (int i = 1; i < screens.Length; i++)
            screens[i].SetActive(false);

        HS.Init(this, "Artificial Instinct");
        ClearScores();

        suspicion = 0;
    }

    void Update() {
        if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.tKey.wasPressedThisFrame)
            ToggleTerminal();
    }

    void ToggleTerminal() {
        if (terminal == null) return;
        bool next = !terminal.activeSelf;
        terminal.SetActive(next);
        var t = terminal.GetComponent<Terminal>();
        if (t != null) t.active = next;
    }

    public void SubmitScore(string name, int score) {
        HS.SubmitHighScore(this, name, score);
    }

    public void ClearScores() {
        HS.Clear(this);
    }
}
