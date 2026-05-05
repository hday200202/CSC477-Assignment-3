using UnityEngine;
using UnityEngine.UI;
using System;

public class SliderPuzzle : MonoBehaviour
{

    public RectTransform box;
    public RectTransform spot;
    public GameObject minigameManager;
    public GameObject sliderPuzzle;

    public Slider sliderH;
    public Slider sliderV;

    private float boxX;
    private float boxY; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxX = -588;
        boxY = -328;
    }

    public void OnEnable()
    {
        box.anchoredPosition = new Vector2(-588, -328);
        spot.anchoredPosition = new Vector2(UnityEngine.Random.Range(-588, -300), UnityEngine.Random.Range(-328, 100));
    }

    // Update is called once per frame
    void Update()
    {
        box.anchoredPosition = new Vector2(boxX+sliderH.value, box.anchoredPosition.y);
        box.anchoredPosition = new Vector2(box.anchoredPosition.x, boxY + sliderV.value);

        if (Math.Abs(box.anchoredPosition.x-spot.anchoredPosition.x) <= 5 && Math.Abs(box.anchoredPosition.y - spot.anchoredPosition.y) <= 5)
        {
            sliderPuzzle.SetActive(false);
            Debug.Log("Complete");
            sliderH.value = 0;
            sliderV.value = 0;
            minigameManager.GetComponent<MinigameManager>().minigameSuccess = true;
        }
    }
}
