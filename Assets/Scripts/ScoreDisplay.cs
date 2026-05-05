using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour {
    public TextMeshProUGUI scoreText;

    void OnEnable() {
        var gm = FindFirstObjectByType<GameManager>();
        if (scoreText != null && gm != null)
            scoreText.text = "Score: " + gm.CalculateScore();
    }
}
