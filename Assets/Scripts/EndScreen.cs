using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour {
    public TextMeshProUGUI nameField;
    public Button submitButton;
    public GameManager gameManager;

    private const int   MaxLength = 5;
    private const float BlinkRate = 0.5f;

    private string nameBuffer    = "";
    private bool   cursorVisible = true;
    private float  cursorTimer   = 0f;
    private bool   initialized   = false;

    void Start() {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmit);
    }

    void OnEnable() { nameBuffer = ""; cursorVisible = true; cursorTimer = 0f; initialized = false; Refresh(); }

    // Called every frame by GameManager.Update() so this works even if
    // this GameObject is inactive or in an inactive part of the hierarchy.
    public void Tick(float dt) {
        if (!initialized) { initialized = true; Refresh(); }

        cursorTimer += dt;
        if (cursorTimer >= BlinkRate) {
            cursorTimer  = 0f;
            cursorVisible = !cursorVisible;
        }

        var kb = Keyboard.current;
        if (kb != null) {
            if (kb.backspaceKey.wasPressedThisFrame && nameBuffer.Length > 0)
                nameBuffer = nameBuffer[..^1];

            if (nameBuffer.Length < MaxLength) {
                for (int i = 0; i < 26; i++) {
                    if (kb[(Key)(Key.A + i)].wasPressedThisFrame) {
                        nameBuffer += (char)('A' + i);
                        break;
                    }
                }
            }

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                OnSubmit();
        }

        Refresh();
    }

    void Refresh() {
        if (nameField == null) return;
        string display = "Enter Name: ";
        for (int i = 0; i < MaxLength; i++) {
            if      (i < nameBuffer.Length)  display += nameBuffer[i];
            else if (i == nameBuffer.Length) display += cursorVisible ? "_" : " ";
            else                             display += "_";
        }
        nameField.text = display;
    }

    void OnSubmit() {
        if (nameBuffer.Length == 0) return;
        if (gameManager != null)
            gameManager.SubmitScore(nameBuffer, gameManager.CalculateScore());
        if (submitButton != null) submitButton.interactable = false;
    }
}

