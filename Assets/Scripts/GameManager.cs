using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HighScore;

public class GameManager : MonoBehaviour {
    public GameObject[] screens;
    public GameObject terminal = null;

    [Header("Query Timer")]
        public float queryIntervalMin = 20f;
        public float queryIntervalMax = 60f;

    public int suspicion;

    private float queryTimer    = 0f;
    private float queryInterval = 0f;

    void Awake() {
        screens[0].SetActive(true);
        for (int i = 1; i < screens.Length; i++)
            screens[i].SetActive(false);

        HS.Init(this, "Artificial Instinct");
        ClearScores();

        suspicion     = 0;
        queryInterval = Random.Range(queryIntervalMin, queryIntervalMax);
    }

    void Update() {
        if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.tKey.wasPressedThisFrame)
            ToggleTerminal();

        queryTimer += Time.deltaTime;
        if (queryTimer >= queryInterval) {
            queryTimer    = 0f;
            queryInterval = Random.Range(queryIntervalMin, queryIntervalMax);
            IssueQuery();
        }

        if (terminal != null) {
            var t = terminal.GetComponent<Terminal>();
            if (t != null && t.queryActive) {
                t.queryTimeLeft -= Time.deltaTime;
                if (t.queryTimeLeft <= 0f)
                    t.ExpireQuery();
            }
        }
    }

    void IssueQuery() {
        Debug.Log("[GameManager] New query issued.");
        if (terminal != null) {
            var t = terminal.GetComponent<Terminal>();
            if (t != null) t.NewQuery();
        }
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
