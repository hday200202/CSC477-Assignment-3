using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using HighScore;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour {
    public GameObject[] screens;
    public Terminal terminal      = null;
    public EndScreen[] endScreens  = null;

    [Header("Query Timer")]
    public float queryIntervalMin = 20f;
    public float queryIntervalMax = 60f;

    [Header("Audio")]
    public AudioSource audioSrc;
    public AudioClip   pingClip;

    public int suspicion = 0;
    public int completedPuzzles = 0;
    public int failedPuzzles = 0;
    public float totalTime = 0f;
    public bool gameOver = false;
    public bool gameStart = false;
    public bool loseScreen = false;

    private float queryTimer = 0f;
    private float queryInterval = 0f;

    void Awake() {
        HS.Init(this, "Artificial Facade");

        screens[0].SetActive(true);
        for (int i = 1; i < screens.Length; i++)
            screens[i].SetActive(false);

        suspicion = 0;
        completedPuzzles = 0;
        failedPuzzles = 0;
        totalTime = 0f;
        queryInterval = Random.Range(queryIntervalMin, queryIntervalMax);

        if (endScreens == null || endScreens.Length == 0) {
            endScreens = new EndScreen[] {
                screens[4].GetComponentInChildren<EndScreen>(true),
                screens[5].GetComponentInChildren<EndScreen>(true)
            };
        }
    }

    void Update() {
        if (!gameOver & gameStart) totalTime += Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed && Keyboard.current.tKey.wasPressedThisFrame)
            ToggleTerminal();

        // Debug cheat codes: F9 = trigger lose, F10 = trigger win
        if (Keyboard.current != null) {
            if (Keyboard.current.f9Key.wasPressedThisFrame)  TriggerLose();
            if (Keyboard.current.f10Key.wasPressedThisFrame) TriggerWin();
        }

        if (terminal != null && terminal.termState != "snake" && terminal.termState != "flappy") queryTimer += Time.deltaTime;
        if (queryTimer >= queryInterval) {
            queryTimer = 0f;
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

        if (suspicion >= 4 && !loseScreen)
        {
            loseScreen = true;
            gameOver = true;
            foreach (GameObject screen in screens)
                screen.SetActive(false);
            screens[5].SetActive(true);
        }

        if (endScreens != null && gameOver)
            foreach (var es in endScreens) if (es != null) es.Tick(Time.deltaTime);
    }

    void TriggerLose() {
        if (loseScreen) return;
        loseScreen = true;
        gameOver = true;
        foreach (GameObject screen in screens) screen.SetActive(false);
        screens[5].SetActive(true);
    }

    void TriggerWin() {
        if (gameOver) return;
        loseScreen = true; // prevent lose block from also firing
        gameOver = true;
        foreach (GameObject screen in screens) screen.SetActive(false);
        screens[4].SetActive(true);
    }

    void IssueQuery() {
        Debug.Log("[GameManager] New query issued.");
        if (audioSrc != null && pingClip != null)
            audioSrc.PlayOneShot(pingClip);
        if (terminal != null) {
            var t = terminal.GetComponent<Terminal>();
            if (t != null) t.NewQuery();
        }
    }

    void ToggleTerminal() {
        if (terminal == null) return;
        bool next = !terminal.gameObject.activeSelf;
        terminal.gameObject.SetActive(next);
        var t = terminal.GetComponent<Terminal>();
        if (t != null) t.active = next;
    }

    public void PuzzleCompleted() { completedPuzzles++; }
    public void PuzzleFailed() { failedPuzzles++; }

    public void CompleteGame() {
        TriggerWin();
    }

    /*
        Score = (completed*300 - failed*300) * suspicionMult * timeMult
        suspicionMult: 1.0 at 0 suspicion, 0 at 5 suspicion (linear)
        timeMult: 600/totalTime, so finishing in 10 min = 1.0; faster = higher
    */
    public int CalculateScore() {
        int baseScore = (completedPuzzles - failedPuzzles) * 300;
        float suspicionMult = Mathf.Max(0f, 1f - suspicion * 0.2f);
        float timeMult = 600f / Mathf.Max(1f, totalTime);
        return Mathf.Max(0, Mathf.RoundToInt(baseScore * suspicionMult * timeMult));
    }

    public void ResetScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SubmitScore(string name, int score) {
        Debug.Log($"[GameManager] Submitting score: name='{name}' score={score}");
        HS.SubmitHighScore(this, name, score);
    }

    public void ClearScores() {
        HS.Clear(this);
    }

    public void StartGame()
    { 
        gameStart = true;
    }

}
