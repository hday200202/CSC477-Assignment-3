using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour {
    public TextMeshProUGUI scoreText;

    void OnEnable() {
        var gm = FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (scoreText != null && gm != null)
            scoreText.text = "Score: " + gm.CalculateScore();
        else
            Debug.LogError($"ScoreDisplay: gm={gm} scoreText={scoreText}");
    }
}
