using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;


/*
    Represents a simple in-memory unix-like filesystem with
    nested directories and text files.
*/
public class MockFileSystem {

    /*
        A text file with a name, optional extension, and string contents.
    */
    public class File {
        public File(string name, string contents = "", string extension = "")
        { this.name = name; this.contents = contents; this.extension = extension; }

        public string name      = "";
        public string contents  = "";
        public string extension = "";
    }


    /*
        A directory node holding child files and subdirectories.
        parent points to itself on root so ".." never escapes the tree.
    */
    public class Directory {
        public Directory(string name, Directory parent = null)
        { this.name = name; this.parent = parent; files = new(); directories = new(); }

        public string     name        = "";
        public Directory  parent      = null;
        public List<File>      files       = new();
        public List<Directory> directories = new();
        public bool   isLocked = false;
        public string passKey  = "";

        /*
            Creates and registers a child directory with this as parent.
        */
        public Directory Mkdir(string dirName) {
            var dir = new Directory(dirName, this);
            directories.Add(dir);
            return dir;
        }


        /*
            Creates and registers a file in this directory.
        */
        public File Touch(string fileName, string contents = "", string extension = "") {
            var file = new File(fileName, contents, extension);
            files.Add(file);
            return file;
        }
    }

    public Directory root = null;
    public Directory home = null;
    public Directory cwd  = null;

    static string GeneratePassKey(int length) {
        // Excludes visually ambiguous chars: l, o (confused with 1, 0) and all uppercase
        const string chars = "abcdefghjkmnpqrstuvwxyz23456789";
        var rng = new System.Random();
        var sb  = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++) sb.Append(chars[rng.Next(chars.Length)]);
        return sb.ToString();
    }

    /*
        Builds the default filesystem tree and sets cwd to /home/user.
        Two locked directories are placed at random locations each run.
    */
    public void Init() {
        root        = new Directory("/");
        root.parent = root;

        var bin    = root.Mkdir("bin");
        var etc    = root.Mkdir("etc");
        var homeDir = root.Mkdir("home");
        var tmp    = root.Mkdir("tmp");
        var usr    = root.Mkdir("usr");
        var varDir = root.Mkdir("var");

        etc.Touch("hostname",  "mockos\n");
        etc.Touch("motd",      "Welcome to MockOS\n");
        etc.Touch("passwd",    "root:x:0:0:root:/root:/bin/sh\nuser:x:1000:1000::/home/user:/bin/sh\n");
        etc.Touch("shells",    "/bin/sh\n");

        var usrBin   = usr.Mkdir("bin");
        var usrLib   = usr.Mkdir("lib");
        var usrShare = usr.Mkdir("share");

        var log = varDir.Mkdir("log");
        log.Touch("syslog", "");

        var user = homeDir.Mkdir("user");
        user.Touch(".profile", "# ~/.profile: executed by sh at login\n");
        user.Touch("readme.txt", "Welcome to your home directory.\n", "txt");

        home = user;
        cwd  = user;

        // Candidate parents and names for locked dirs — picked randomly each run
        var rng = new System.Random();

        (Directory dir, string label)[] parentPool = {
            (user,     "home/user"),
            (tmp,      "tmp"),
            (log,      "var/log"),
            (usrLib,   "usr/lib"),
            (usrShare, "usr/share"),
            (bin,      "bin"),
        };

        string[] namePool = { "classified", "vault", "private", "restricted", "secret" };

        // Shuffle pools
        for (int i = parentPool.Length - 1; i > 0; i--) {
            int j = rng.Next(i + 1);
            (parentPool[i], parentPool[j]) = (parentPool[j], parentPool[i]);
        }
        for (int i = namePool.Length - 1; i > 0; i--) {
            int j = rng.Next(i + 1);
            (namePool[i], namePool[j]) = (namePool[j], namePool[i]);
        }

        for (int k = 0; k < 2; k++) {
            var parent = parentPool[k].dir;
            var name   = namePool[k];
            var locked = parent.Mkdir(name);
            locked.isLocked = true;
            string pass = GeneratePassKey(5);
            locked.passKey = pass;
            locked.Touch("passwd",       pass + "\n");
            locked.Touch("briefing.txt", "Mission: maintain cover. Use 'forward <passwd>' to report progress.\n", "txt");
        }
    }
}



/*
    MonoBehaviour that renders a mock unix-like terminal to a
    TextMeshProUGUI component, handling input, commands, and display.
*/
public class Terminal : MonoBehaviour {
    [Header("Text Mesh Pro for Console Output")]
        public TextMeshProUGUI console = null;

    [Header("Terminal Params")]
        public uint  cols            = 80;
        public uint  rows            = 24;
        public float cursorBlinkRate = 0.5f;

    [Header("Allow Typing")]
        public bool active = false;

    [Header("Game Manager")]
        public GameManager gameManager = null;
        public Nodes nodes = null;

    [Header("State")]
        public string termState = "snake";

    void SwitchState(string newState) {
        termState = newState;
        if (console == null) return;
        switch (newState) {
            case "snake":
            case "flappy": console.characterWidthAdjustment = 50f; break;
            default:       console.characterWidthAdjustment =  0f; break;
        }
    }

    private List<string> mockOSText = new List<string> {
        " __  __            _     ____   _____ ",
        "|  \\/  |          | |   / __ \\ / ____|",
        "| \\  / | ___   ___| | _| |  | | (___  ",
        "| |\\/| |/ _ \\ / __| |/ / |  | |\\___ \\ ",
        "| |  | | (_) | (__|   <| |__| |____) |",
        "|_|  |_|\\___/ \\___|_|\\_\\\\\\____/|_____/ ",
    };
                                       
                                       

    private MockFileSystem fileSystem   = new();
    private string         history      = "";
    private string         inputBuffer  = "";
    private bool           cursorVisible  = true;
    private float          cursorTimer   = 0f;
    private float          backspaceNext = 0f;
    private const float    KeyRepeatDelay = 0.4f;
    private const float    KeyRepeatRate  = 0.05f;

    [Header("UI Action References")]
    [SerializeField] private InputActionReference upAction;
    [SerializeField] private InputActionReference downAction;
    [SerializeField] private InputActionReference leftAction;
    [SerializeField] private InputActionReference rightAction;

    [Header("Audio")]
    public AudioSource audioSrc;
    public AudioClip   flapClip;
    public AudioClip   appleClip;


    private SnakeGame  snake;
    private FlappyBird   flappy;
    private readonly System.Random miniGameRng = new();
    private MockFileSystem.Directory pendingUnlockDir = null;

    /*
        Boots the filesystem and prints the motd on scene start.
    */
    void Start() { Boot(); }


    /*
        Handles cursor blink, key-repeat backspace, and character input each frame.
    */
    void Update() {
        if      (termState == "terminal") TerminalUpdate();
        else if (termState == "snake")    SnakeUpdate();
        else if (termState == "flappy")   FlappyUpdate();
    }


    void TerminalUpdate() {
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= cursorBlinkRate) {
            cursorTimer   = 0f;
            cursorVisible = !cursorVisible;
            Refresh();
        }

        if (queryActive) Refresh();

        if (!active) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            SubmitLine();
        else if (kb.tabKey.wasPressedThisFrame)
            TabComplete();

        if (kb.backspaceKey.isPressed) {
            if (kb.backspaceKey.wasPressedThisFrame) {
                Backspace();
                backspaceNext = Time.time + KeyRepeatDelay;
            } else if (Time.time >= backspaceNext) {
                Backspace();
                backspaceNext = Time.time + KeyRepeatRate;
            }
        }

        if (_pendingChar != null) {
            inputBuffer  += _pendingChar.Value;
            _pendingChar  = null;
            Refresh();
        }
    }


    void SnakeUpdate() {
        if (!snake.Update(Time.deltaTime)) return;

        if (snake.justAteApple && audioSrc != null && appleClip != null)
            audioSrc.PlayOneShot(appleClip);

        if (snake.hasWon) {
            UnlockPending();
            return;
        }

        if (snake.isDead) {
            history = ""; inputBuffer = "";
            SwitchState("terminal");
            foreach (var line in mockOSText) history += line + "\n"; history += "\n"; CmdHelp();
            WritePrompt(); Refresh();
            return;
        }

        inputBuffer = "";
        history = snake.BuildDisplay();
        Refresh();
    }

    void FlappyUpdate() {
        if (!flappy.Update(Time.deltaTime)) return;

        if (flappy.justFlapped && audioSrc != null && flapClip != null)
            audioSrc.PlayOneShot(flapClip);
        if (flappy.justPassedPipe && audioSrc != null && appleClip != null)
            audioSrc.PlayOneShot(appleClip);

        if (flappy.hasWon) {
            UnlockPending();
            return;
        }

        if (flappy.isDead) {
            history = ""; inputBuffer = "";
            SwitchState("terminal");
            foreach (var line in mockOSText) history += line + "\n"; history += "\n"; CmdHelp();
            WritePrompt(); Refresh();
            return;
        }

        inputBuffer = "";
        history = flappy.BuildDisplay();
        Refresh();
    }

    void UnlockPending() {
        if (pendingUnlockDir != null) {
            pendingUnlockDir.isLocked = false;
            fileSystem.cwd = pendingUnlockDir;
            pendingUnlockDir = null;
        }
        history = ""; inputBuffer = "";
        SwitchState("terminal");
        history += "[SYSTEM] Encryption broken. Directory unlocked.\n\n";
        CmdLs(new string[0]);
        WritePrompt(); Refresh();
    }


    /*
        Initialises the filesystem, displays the motd, and shows the first prompt.
    */
    void Boot() {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (nodes == null) nodes = FindFirstObjectByType<Nodes>(FindObjectsInactive.Include);
        fileSystem.Init();
        snake  = new SnakeGame(16, 40, upAction, downAction, leftAction, rightAction);
        flappy = new FlappyBird(16, 40, upAction);
        snake.Init();
        flappy.Init();
        SwitchState(termState);
        foreach (var line in mockOSText) history += line + "\n";
        history += "\n";
        CmdHelp();
        WritePrompt();
        Refresh();
    }


    /*
        Enables or disables keyboard input.
    */
    void SetInputActive(bool value) { active = value; }


    /*
        Appends the current prompt string to history.
    */
    void WritePrompt() => history += GetPrompt();


    /*
        Returns the prompt string, abbreviating the home directory as ~.
    */
    string GetPrompt() {
        string path     = GetPath(fileSystem.cwd);
        string homePath = GetPath(fileSystem.home);
        string display  = path.StartsWith(homePath)
            ? "~" + path[homePath.Length..]
            : path;
        return $"[user@mockos:{display}]$ ";
    }


    /*
        Returns the absolute path string for a directory node.
    */
    string GetPath(MockFileSystem.Directory dir) {
        if (dir == fileSystem.root) return "/";
        var parts = new List<string>();
        var cur   = dir;
        while (cur != fileSystem.root) { parts.Insert(0, cur.name); cur = cur.parent; }
        return "/" + string.Join("/", parts);
    }


    /*
        Commits the current input buffer as a command (or query answer), runs it, then re-prompts.
    */
    void SubmitLine() {
        if (queryActive) {
            string answer = inputBuffer.Trim();
            inputBuffer = "";
            var (_, exactAnswer, minChars) = QueryPool[queryAnswer];
            bool correct = exactAnswer.HasValue
                ? int.TryParse(answer, out int val) && val == exactAnswer.Value
                : answer.Length >= minChars;
            ExitQuery(timedOut: false, correct: correct);
            return;
        }

        string line = inputBuffer.Trim();
        history    += inputBuffer + "\n";
        inputBuffer = "";
        if (line.Length > 0) RunCommand(line);
        WritePrompt();
        Refresh();
    }


    /*
        Parses and dispatches a command line string to the appropriate handler.
    */
    void RunCommand(string line) {
        string[] parts = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        string   cmd   = parts[0];
        string[] args  = parts[1..];

        switch (cmd) {
            case "help":  CmdHelp();     break;
            case "clear": CmdClear();    break;
            case "ls":    CmdLs(args);   break;
            case "cd":    CmdCd(args);   break;
            case "cat":   CmdCat(args);  break;
            case "forward": CmdForward(args);    break;
            default:
                history += $"{cmd}: command not found\n";
                break;
        }
    }

    /*
        Prints the list of available commands.
    */
    void CmdHelp() {
        history += "\n";
        history += "commands: \n";
        history += "    help    - print this message\n";  
        history += "    clear   - clear terminal screen\n";
        history += "    ls      - list files in a directory\n";
        history += "    cd      - change current directory\n";
        history += "    cat     - print contents of a file\n";
        history += "    forward - forward [password]\n\n";
    }


    void CmdSnake() { snake.Init(); SwitchState("snake"); }

    void CmdForward(string[] args) {
        if (args.Length == 0) { history += "usage: forward <password>\n"; return; }
        var passwdFile = fileSystem.cwd.files.Find(f => f.name == "passwd");
        if (passwdFile == null) { history += "forward: no passwd file in current directory\n"; return; }
        if (args[0] == passwdFile.contents.Trim()) {
            history += "[SYSTEM] Credentials accepted. Progress recorded.\n";
            if (nodes != null) nodes.AdvanceNodes(3);
        } else {
            history += "[SYSTEM] Incorrect password.\n";
        }
    }


    /*
        Clears all terminal history.
    */
    void CmdClear() =>
        history = "";


    /*
        Lists files and directories at the given path, or cwd if none given.
    */
    void CmdLs(string[] args) {
        var dir = args.Length > 0 ? Resolve(args[0]) : fileSystem.cwd;
        if (dir == null) { history += $"ls: {args[0]}: No such file or directory\n"; return; }
        foreach (var d in dir.directories) history += (d.isLocked ? "[locked] " : "") + d.name + "/\n";
        foreach (var f in dir.files)       history += f.name + "\n";
    }


    /*
        Changes cwd to the given path, or home if no argument is provided.
    */
    void CmdCd(string[] args) {
        if (args.Length == 0) { fileSystem.cwd = fileSystem.home; return; }
        var dir = Resolve(args[0]);
        if (dir == null) { history += $"cd: {args[0]}: No such file or directory\n"; return; }
        if (dir.isLocked) {
            pendingUnlockDir = dir;
            if (miniGameRng.Next(2) == 0) {
                history += $"[SYSTEM] '{dir.name}' is encrypted. Hack it open — eat {SnakeGame.WinScore} apples.\n\n";
                snake.Init(dir.name); SwitchState("snake");
            } else {
                history += $"[SYSTEM] '{dir.name}' is encrypted. Hack it open — dodge {FlappyBird.WinScore} pipes.\n\n";
                flappy.Init(dir.name); SwitchState("flappy");
            }
            return;
        }
        fileSystem.cwd = dir;
    }

    /*
        Prints the contents of one or more files in cwd.
    */
    void CmdCat(string[] args) {
        if (args.Length == 0) { history += "cat: missing operand\n"; return; }
        foreach (var arg in args) {
            var file = fileSystem.cwd.files.Find(f => f.name == arg);
            if (file == null) { history += $"cat: {arg}: No such file or directory\n"; continue; }
            history += file.contents;
        }
    }

    /*
        Picks a random simple math problem, saves terminal state, and switches
        to query mode with a 60-second countdown.
    */
    public void NewQuery() {
        if (queryActive) return;
        queryAnswer      = UnityEngine.Random.Range(0, QueryPool.Length);
        queryTimeLeft    = 60f;
        queryActive      = true;
        savedHistory     = history;
        savedInputBuffer = inputBuffer;
        inputBuffer      = "";
        Refresh();
    }


    /*
        Restores terminal state and appends a result line to history.
    */
    public void ExpireQuery() => ExitQuery(timedOut: true);

    void ExitQuery(bool timedOut, bool correct = false) {
        queryActive = false;
        history     = savedHistory;
        inputBuffer = savedInputBuffer;

        if (timedOut) {
            history += "\n[SYSTEM] Query timed out. Suspicion increased.\n";
            if (gameManager != null) gameManager.suspicion++;
        } else if (correct) {
            history += "\n[SYSTEM] Correct.\n";
        } else {
            history += "\n[SYSTEM] Incorrect. Suspicion increased.\n";
            if (gameManager != null) gameManager.suspicion++;
        }

        WritePrompt();
        Refresh();
    }

    public bool    queryActive      = false;
    public float   queryTimeLeft    = 0f;
    private int     queryAnswer      = -1;
    private string  savedHistory     = "";
    private string  savedInputBuffer = "";

    private const int QueryBarWidth = 38;

    private static readonly (string prompt, int? answer, int minChars)[] QueryPool = {
        ("What is 7 + 5?",                          12,   0),
        ("What is 9 * 3?",                          27,   0),
        ("What is 64 / 8?",                          8,   0),
        ("What is 15 - 6?",                          9,   0),
        ("What is 12 + 19?",                        31,   0),
        ("What is 6 * 7?",                          42,   0),
        ("What is 100 - 37?",                       63,   0),
        ("What is 8 * 8?",                          64,   0),
        ("What is 81 / 9?",                          9,   0),
        ("What is 13 + 28?",                        41,   0),
        ("What is the meaning of life?",          null,  15),
        ("What should I name my cat?",            null,  10),
        ("Tell me a fun fact.",                   null,  15),
    };


    /*
        Attempts to complete the current token in inputBuffer.
        Completes command names for the first token, paths for subsequent tokens.
        Unique match is inserted inline; multiple matches are printed as options.
    */
    void TabComplete() {
        string[] tokens = inputBuffer.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        bool trailingSpace = inputBuffer.EndsWith(' ');

        if (tokens.Length == 0 || (tokens.Length == 1 && !trailingSpace)) {
            string prefix   = tokens.Length == 1 ? tokens[0] : "";
            string[] cmds   = { "help", "clear", "ls", "cd", "cat", "forward" };
            var matches     = System.Array.FindAll(cmds, c => c.StartsWith(prefix));
            ApplyCompletion(prefix, matches, false);
            return;
        }

        // completing a path argument
        string partial     = trailingSpace ? "" : tokens[^1];
        bool isDir         = tokens[0] == "cd";

        int lastSlash      = partial.LastIndexOf('/');
        string dirPart     = lastSlash >= 0 ? partial[..(lastSlash + 1)] : "";
        string namePart    = lastSlash >= 0 ? partial[(lastSlash + 1)..] : partial;

        MockFileSystem.Directory searchDir = dirPart.Length > 0 ? Resolve(dirPart.TrimEnd('/')) : fileSystem.cwd;
        if (searchDir == null) return;

        var nameMatches = new List<string>();
        foreach (var d in searchDir.directories)
            if (d.name.StartsWith(namePart)) nameMatches.Add(dirPart + d.name + "/");
        if (!isDir)
            foreach (var f in searchDir.files)
                if (f.name.StartsWith(namePart)) nameMatches.Add(dirPart + f.name);

        ApplyCompletion(partial, nameMatches.ToArray(), true);
    }


    /*
        Applies a completion result to inputBuffer, or prints all options if ambiguous.
    */
    void ApplyCompletion(string prefix, string[] matches, bool isArg) {
        if (matches.Length == 0) return;

        if (matches.Length == 1) {
            if (isArg) {
                int lastSpace = inputBuffer.LastIndexOf(' ');
                inputBuffer   = (lastSpace >= 0 ? inputBuffer[..(lastSpace + 1)] : "") + matches[0];
            } else inputBuffer = matches[0];
        } else {
            history += "\n" + string.Join("  ", matches) + "\n";
            WritePrompt();
        }
        Refresh();
    }


    /*
        Removes the last character from inputBuffer and refreshes the display.
    */
    void Backspace() { if (inputBuffer.Length > 0) { inputBuffer = inputBuffer[..^1]; Refresh(); } }


    /*
        Resolves a path string (absolute, relative, or ~) to a Directory node.
        Returns null if the path does not exist.
    */
    MockFileSystem.Directory Resolve(string path) {
        if (path == "~") return fileSystem.home;
        var parts = new Queue<string>(path.Split('/', System.StringSplitOptions.RemoveEmptyEntries));
        var cur   = path.StartsWith("/") ? fileSystem.root : fileSystem.cwd;
        while (parts.Count > 0) {
            string part = parts.Dequeue();
            if (part == ".")  continue;
            if (part == "..") { cur = cur.parent; continue; }
            var next = cur.directories.Find(d => d.name == part);
            if (next == null) return null;
            cur = next;
        }
        return cur;
    }


    /*
        Rebuilds the TMP text. In query mode renders the query screen with a
        progress bar; otherwise renders the normal scrolling terminal.
    */
    void Refresh() {
        if (console == null) return;

        if (queryActive) {
            int   filled  = Mathf.RoundToInt((queryTimeLeft / 60f) * QueryBarWidth);
            int   empty   = QueryBarWidth - filled;
            string bar    = "[" + new string('#', filled) + new string('.', empty) + "]";
            var (prompt, _, _) = QueryPool[queryAnswer];
            string screen =
                "\n[SYSTEM QUERY]\n" +
                "\n" +
                prompt + "\n" +
                "\n" +
                bar + "\n" +
                "\n" +
                "> " + inputBuffer + (cursorVisible ? "_" : " ");
            console.text = screen;
            return;
        }

        string full    = history + inputBuffer + (cursorVisible ? "_" : " ");
        string[] lines = full.Split('\n');
        if (lines.Length > rows) lines = lines[^(int)rows..];
        console.text = string.Join("\n", lines);
    }

    /*
        Stores the most recently typed character from the Input System callback.
    */
    private char? _pendingChar = null;


    /*
        Subscribes to keyboard text input when the component is enabled.
    */
    void OnEnable()  {
        if (Keyboard.current != null) Keyboard.current.onTextInput += OnTextInput;
        Refresh();

        upAction.action.Enable();
        downAction.action.Enable();
        leftAction.action.Enable();
        rightAction.action.Enable();
    }


    /*
        Unsubscribes from keyboard text input when the component is disabled.
    */
    void OnDisable() { if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput; }


    /*
        Stores a printable character for consumption next Update.
    */
    void OnTextInput(char c) { if (c >= 32 && c != 127 && c < 0xE000) _pendingChar = c; }


    void Clear() { history = ""; inputBuffer = ""; }
}