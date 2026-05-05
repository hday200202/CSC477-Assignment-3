using UnityEngine;
using TMPro;

public class Suspicion : MonoBehaviour
{
    public TMP_Text suspicionText;
    public GameObject gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int suspicionCount = gameManager.GetComponent<GameManager>().suspicion;
        suspicionText.text = "Suspicion: " + suspicionCount;
    }
}
