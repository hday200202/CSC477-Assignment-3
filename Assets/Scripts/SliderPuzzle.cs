using UnityEngine;
using UnityEngine.UI;

public class SliderPuzzle : MonoBehaviour
{

    public RectTransform box;
    public GameObject spot;

    public Slider sliderH;
    public Slider sliderV;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        box.anchoredPosition = new Vector2(box.anchoredPosition.x+sliderH.value, box.anchoredPosition.y); 
    }
}
