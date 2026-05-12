using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SliderPuzzle : MonoBehaviour
{

    public RectTransform box;
    public RectTransform spot;
    public GameObject minigameManager;
    public GameObject sliderPuzzle;
    public TMP_Text time_display;

    public Slider sliderH;
    public Slider sliderV;

    private float boxX;
    private float boxY;
    private float timer = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxX = -588;
        boxY = -328;
    }

    public void OnEnable()
    {
        if (minigameManager.GetComponent<MinigameManager>().minigameStart == true)
        {
            timer = 20;

            box.anchoredPosition = new Vector2(-588, -328);
            spot.anchoredPosition = new Vector2(UnityEngine.Random.Range(-588, -300), UnityEngine.Random.Range(-328, 100));
            minigameManager.GetComponent<MinigameManager>().minigameStart = false;
        }
        Debug.Log(spot);
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        time_display.text = "Time remaining: " + timer;

        if (timer <= 0)
        {
            sliderPuzzle.SetActive(false);
            minigameManager.GetComponent<MinigameManager>().minigameFailure = true;
        }

        box.anchoredPosition = new Vector2(boxX+sliderH.value, box.anchoredPosition.y);
        box.anchoredPosition = new Vector2(box.anchoredPosition.x, boxY + sliderV.value);

        if (Math.Abs(box.anchoredPosition.x-spot.anchoredPosition.x) <= 15 && Math.Abs(box.anchoredPosition.y - spot.anchoredPosition.y) <= 15)
        {
            sliderPuzzle.SetActive(false);
            Debug.Log("Complete");
            sliderH.value = 0;
            sliderV.value = 0;
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
    }
}
