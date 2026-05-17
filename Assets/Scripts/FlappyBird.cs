using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlappyBird {
    public bool isDead      { get; private set; }
    public int  pipesPassed { get; private set; }
    public bool hasWon      => pipesPassed >= WinScore;
    public const int WinScore = 10;

    private readonly int      rows, cols;
    private readonly char[,]  grid;

    private float birdYf;
    private float birdVel;
    private const int   BirdX        = 5;
    private const int   BirdW        = 2;
    private const int   BirdH        = 2;
    private const float Gravity      = 0.40f;
    private const float FlapStrength = -1.6f;
    private const float UpdateDelay  = 0.10f;

    private float  timer;
    private float  countdown;
    private bool   flapQueued;
    private string unlockTarget = "";

    private struct Pipe { public int x, gapTop; public bool passed; }
    private const int GapSize     = 5;
    private const int PipeSpacing = 18;

    private int                    ticksSincePipe;
    private readonly List<Pipe>    pipes = new();
    private readonly System.Random rng   = new();

    private readonly InputActionReference upAction;

    public FlappyBird(int rows, int cols, InputActionReference up) {
        this.rows = rows;
        this.cols = cols;
        grid      = new char[rows, cols];
        upAction  = up;
    }

    public void Init(string unlockTarget = "") {
        this.unlockTarget = unlockTarget;
        isDead         = false;
        pipesPassed    = 0;
        birdYf         = (rows - BirdH) / 2f;
        birdVel        = 0f;
        flapQueued     = false;
        ticksSincePipe = PipeSpacing / 2;
        pipes.Clear();
        timer     = 0f;
        countdown = 3.0f;
        RefreshGrid();
    }

    public bool Update(float deltaTime) {
        if (isDead || hasWon) return false;
        if (countdown > 0f) { countdown -= deltaTime; return true; }
        HandleInput();
        timer += deltaTime;
        if (timer < UpdateDelay) return false;
        timer -= UpdateDelay;
        Tick();
        return true;
    }

    void HandleInput() {
        if (upAction != null && upAction.action.WasPressedThisFrame())
            flapQueued = true;
    }

    void Tick() {
        if (flapQueued) { birdVel = FlapStrength; flapQueued = false; }
        birdVel += Gravity;
        birdYf  += birdVel;

        for (int i = pipes.Count - 1; i >= 0; i--) {
            var p = pipes[i];
            p.x--;
            if (!p.passed && p.x < BirdX) { p.passed = true; pipesPassed++; }
            pipes[i] = p;
            if (p.x < 1) pipes.RemoveAt(i);
        }

        ticksSincePipe++;
        if (ticksSincePipe >= PipeSpacing) {
            ticksSincePipe = 0;
            SpawnPipe();
        }

        CheckCollision();
        if (!isDead) RefreshGrid();
    }

    void SpawnPipe() {
        int gapTop = rng.Next(2, rows - GapSize - 2);
        pipes.Add(new Pipe { x = cols - 2, gapTop = gapTop, passed = false });
    }

    void CheckCollision() {
        int by = Mathf.RoundToInt(birdYf);
        if (by < 1 || by + BirdH - 1 > rows - 2) { isDead = true; return; }
        foreach (var p in pipes) {
            if (p.x > BirdX + BirdW - 1 || p.x < BirdX) continue;
            for (int r = by; r < by + BirdH; r++)
                if (r < p.gapTop || r >= p.gapTop + GapSize) { isDead = true; return; }
        }
    }

    void RefreshGrid() {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = (r == 0 || r == rows - 1 || c == 0 || c == cols - 1) ? '#' : ' ';
        foreach (var p in pipes) {
            if (p.x < 1 || p.x >= cols - 1) continue;
            for (int r = 1; r < rows - 1; r++)
                if (r < p.gapTop || r >= p.gapTop + GapSize)
                    grid[r, p.x] = '|';
        }
        int by = Mathf.Clamp(Mathf.RoundToInt(birdYf), 1, rows - 1 - BirdH);
        for (int dr = 0; dr < BirdH; dr++)
            for (int dc = 0; dc < BirdW; dc++)
                grid[by + dr, BirdX + dc] = '#';
    }

    public string BuildDisplay() {
        if (countdown > 0f) {
            RefreshGrid();
            WriteOverlay("GET READY", rows / 2 - 2);
            WriteOverlay(Mathf.Clamp(Mathf.CeilToInt(countdown), 1, 3).ToString(), rows / 2);
            WriteOverlay("[up] to flap", rows / 2 + 2);
        }
        var sb = new System.Text.StringBuilder();
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) sb.Append(grid[r, c]);
            sb.Append('\n');
        }
        sb.Append($" Unlocking: {unlockTarget}  Pipes: {pipesPassed}/{WinScore}");
        return sb.ToString();
    }

    void WriteOverlay(string text, int row) {
        int start = (cols - text.Length) / 2;
        for (int i = 0; i < text.Length; i++) {
            int c = start + i;
            if (c >= 1 && c < cols - 1) grid[row, c] = text[i];
        }
    }
}
