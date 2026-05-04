using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Nodes : MonoBehaviour
{

    public Image[] nodes;
    public GameObject mapScreen;
    public GameObject winScreen;
    private int i = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nodes[0].color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            i++;
            if (i >= nodes.Length)
            {
                mapScreen.SetActive(false);
                winScreen.SetActive(true);
            }
            else
            {
                nodes[i].color = Color.white;
            }
        }
    }
}
