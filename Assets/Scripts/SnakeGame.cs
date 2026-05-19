using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SnakeDirection { U, D, L, R }

public class SnakeGame {

    public bool isDead        { get; private set; }
    public int  applesEaten   { get; private set; }
    public bool hasWon        => applesEaten >= WinScore;
    public bool justAteApple  { get; private set; }
    public const int WinScore  = 10;

    private string unlockTarget = "";

    private readonly char[,]               grid;
    private          SnakeDirection        direction;
    private readonly Queue<SnakeDirection> inputQueue  = new();
    private          SnakeDirection        lastQueued;
    private          Vector2Int            head;
    private          Vector2Int            applePos;
    private          float                 timer;
    private          float                 countdown;
    private          int                   growth;
    private readonly Queue<Vector2Int>     body        = new();
    private readonly Queue<char>           bodyChars   = new();
    private          int                   letterIndex;
    private const    string                Letters     = "SNAKE";

    private const int   InputQueueMax = 2;
    private const float UpdateDelay   = 0.25f;

    private readonly InputActionReference upAction;
    private readonly InputActionReference downAction;
    private readonly InputActionReference leftAction;
    private readonly InputActionReference rightAction;

    public SnakeGame(int rows, int cols,
        InputActionReference up, InputActionReference down,
        InputActionReference left, InputActionReference right) {
        grid        = new char[rows, cols];
        upAction    = up;
        downAction  = down;
        leftAction  = left;
        rightAction = right;
    }

    public void Init(string unlockTarget = "") {
        this.unlockTarget = unlockTarget;
        body.Clear();
        bodyChars.Clear();
        inputQueue.Clear();
        head        = new Vector2Int(20, 8);
        direction   = SnakeDirection.U;
        lastQueued  = SnakeDirection.U;
        isDead      = false;
        applesEaten = 0;
        growth      = 0;
        timer       = 0f;
        countdown   = 3.0f;
        letterIndex = 0;
        body.Enqueue(new Vector2Int(20, 10)); bodyChars.Enqueue('A');
        body.Enqueue(new Vector2Int(20, 9));  bodyChars.Enqueue('N');
        body.Enqueue(head);                   bodyChars.Enqueue('S');
        applePos = new Vector2Int(10, 8);
        SpawnApple();
        RefreshGrid();
    }

    public bool Update(float deltaTime) {
        if (countdown > 0f) { countdown -= deltaTime; return true; }
        HandleInput();
        timer += deltaTime;
        if (timer < UpdateDelay) return false;
        timer = 0f;

        if (!isDead) {
            justAteApple = false;
            if (inputQueue.Count > 0) direction = inputQueue.Dequeue();
            HandleMovement();
        }
        return true;
    }

    public string BuildDisplay() {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        if (countdown > 0f) {
            RefreshGrid();
            WriteOverlay("GET READY", rows / 2 - 2);
            WriteOverlay(Mathf.Clamp(Mathf.CeilToInt(countdown), 1, 3).ToString(), rows / 2);
            WriteOverlay("[arrows] to move", rows / 2 + 2);
        }
        var sb   = new StringBuilder(rows * (cols + 1));
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++)
                sb.Append(grid[r, c]);
            sb.Append('\n');
        }
        string target = unlockTarget.Length > 0 ? $" Unlocking: {unlockTarget}  " : " ";
        sb.Append($"{target}Apples: {applesEaten}/{WinScore}\n");
        return sb.ToString();
    }

    void WriteOverlay(string text, int row) {
        int cols = grid.GetLength(1);
        int start = (cols - text.Length) / 2;
        for (int i = 0; i < text.Length; i++) {
            int c = start + i;
            if (c >= 1 && c < cols - 1) grid[row, c] = text[i];
        }
    }

    void SpawnApple() {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        Vector2Int candidate;
        do {
            int c = Random.Range(1, cols - 1);
            int r = Random.Range(1, rows - 1);
            candidate = new Vector2Int(c, r);
        } while (body.Contains(candidate));
        applePos = candidate;
    }

    void RefreshGrid() {
        System.Array.Clear(grid, 0, grid.Length);
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = (r == 0 || r == rows - 1) ? '#'
                    : (c == 0 || c == cols - 1)         ? '#'
                    : ' ';
        var positions = body.ToArray();
        var chars     = bodyChars.ToArray();
        for (int i = 0; i < positions.Length; i++)
            grid[positions[i].y, positions[i].x] = chars[i];
        grid[applePos.y, applePos.x] = 'O';
    }

    void HandleMovement() {
        int delta = (direction == SnakeDirection.R || direction == SnakeDirection.D) ? 1 : -1;
        letterIndex = ((letterIndex + delta) % Letters.Length + Letters.Length) % Letters.Length;
        char headChar = Letters[letterIndex];

        Vector2Int move = direction switch {
            SnakeDirection.U => new Vector2Int( 0, -1),
            SnakeDirection.D => new Vector2Int( 0,  1),
            SnakeDirection.L => new Vector2Int(-1,  0),
            SnakeDirection.R => new Vector2Int( 1,  0),
            _                => Vector2Int.zero,
        };

        Vector2Int next = head + move;
        char cell = grid[next.y, next.x];

        if (cell == '#' || "SNAKE".IndexOf(cell) >= 0) { isDead = true; return; }

        head = next;
        body.Enqueue(head);
        bodyChars.Enqueue(headChar);

        if (cell == 'O') {
            applesEaten++;
            justAteApple = true;
            growth += 5;
            SpawnApple();
        }

        if (growth > 0) growth--;
        else { body.Dequeue(); bodyChars.Dequeue(); }

        RefreshGrid();
    }

    void HandleInput() {
        bool ku = upAction.action.WasPressedThisFrame();
        bool kd = downAction.action.WasPressedThisFrame();
        bool kl = leftAction.action.WasPressedThisFrame();
        bool kr = rightAction.action.WasPressedThisFrame();

        SnakeDirection last = inputQueue.Count > 0 ? lastQueued : direction;

        SnakeDirection? input = null;
        if      (ku && last != SnakeDirection.D) input = SnakeDirection.U;
        else if (kd && last != SnakeDirection.U) input = SnakeDirection.D;
        else if (kl && last != SnakeDirection.R) input = SnakeDirection.L;
        else if (kr && last != SnakeDirection.L) input = SnakeDirection.R;

        if (input.HasValue && inputQueue.Count < InputQueueMax) {
            lastQueued = input.Value;
            inputQueue.Enqueue(input.Value);
        }
    }
}
