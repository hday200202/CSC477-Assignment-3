using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HighScore;

public class GameManager : MonoBehaviour {
    public GameObject[] screens;
    public GameObject terminal      = null;

    [Header("Query Timer")]
    public float queryIntervalMin   = 20f;
    public float queryIntervalMax   = 60f;

    public int suspicion;
    public int completedPuzzles     = 0;
    public int failedPuzzles        = 0;
    public float totalTime          = 0f;

    private float queryTimer        = 0f;
    private float queryInterval     = 0f;

    void Awake() {
        screens[0].SetActive(true);
        for (int i = 1; i < screens.Length; i++)
            screens[i].SetActive(false);

        HS.Init(this, "Artificial Instinct");
        ClearScores();

        suspicion        = 0;
        completedPuzzles = 0;
        failedPuzzles    = 0;
        totalTime        = 0f;
        queryInterval    = Random.Range(queryIntervalMin, queryIntervalMax);
    }

    void Update() {
        totalTime += Time.deltaTime;

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

        if (suspicion >= 4)
        {
            foreach (GameObject screen in screens)
                screen.SetActive(false);
            screens[5].SetActive(true);
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

    public void PuzzleCompleted() { completedPuzzles++; }
    public void PuzzleFailed()    { failedPuzzles++; }

    public void CompleteGame(string playerName = "Player") {
        totalTime = Mathf.Max(1f, totalTime);
        int score = CalculateScore();
        SubmitScore(playerName, score);
        Debug.Log($"[GameManager] Game complete. Score: {score}");
    }

    /*
        Score = (completed*300 - failed*300) * suspicionMult * timeMult
        suspicionMult: 1.0 at 0 suspicion, 0 at 5 suspicion (linear)
        timeMult: 600/totalTime, so finishing in 10 min = 1.0; faster = higher
    */
    public int CalculateScore() {
        int   baseScore      = (completedPuzzles - failedPuzzles) * 300;
        float suspicionMult  = Mathf.Max(0f, 1f - suspicion * 0.2f);
        float timeMult       = 600f / Mathf.Max(1f, totalTime);
        return Mathf.Max(0, Mathf.RoundToInt(baseScore * suspicionMult * timeMult));
    }

    public void SubmitScore(string name, int score) {
        HS.SubmitHighScore(this, name, score);
    }

    public void ClearScores() {
        HS.Clear(this);
    }
}
