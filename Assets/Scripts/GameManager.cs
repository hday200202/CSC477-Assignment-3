using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public GameObject[] screens;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        screens[0].SetActive(true);
        for (int i = 1; i < screens.Length; i++)
        {
            screens[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
