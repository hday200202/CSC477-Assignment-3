using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NoahsPuzzlePiece : MonoBehaviour
{
    public Button button;
    public GameObject parent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parent = gameObject;
    }

    public void OnButtonClick()
    {
        transform.parent.GetComponent<NoahsPuzzle>().Swap(parent);
    }
}
