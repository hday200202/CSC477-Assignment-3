using System.Collections.Generic;
using TMPro;
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

    /*
        Builds the default filesystem tree and sets cwd to /home/user.
    */
    public void Init() {
        root        = new Directory("/");
        root.parent = root;

        var bin  = root.Mkdir("bin");
        var etc  = root.Mkdir("etc");
        var homeDir = root.Mkdir("home");
        var tmp  = root.Mkdir("tmp");
        var usr  = root.Mkdir("usr");
        var varDir = root.Mkdir("var");

        etc.Touch("hostname",  "mockos\n");
        etc.Touch("motd",      "Welcome to MockOS\n");
        etc.Touch("passwd",    "root:x:0:0:root:/root:/bin/sh\nuser:x:1000:1000::/home/user:/bin/sh\n");
        etc.Touch("shells",    "/bin/sh\n");

        usr.Mkdir("bin");
        usr.Mkdir("lib");
        usr.Mkdir("share");

        var log = varDir.Mkdir("log");
        log.Touch("syslog", "");

        var user = homeDir.Mkdir("user");
        user.Touch(".profile", "# ~/.profile: executed by sh at login\n");
        user.Touch("readme.txt", "Welcome to your home directory.\n", "txt");

        home = user;
        cwd  = user;
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

    /*
        Boots the filesystem and prints the motd on scene start.
    */
    void Start() { Boot(); }


    /*
        Handles cursor blink, key-repeat backspace, and character input each frame.
    */
    void Update() {
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= cursorBlinkRate) {
            cursorTimer   = 0f;
            cursorVisible = !cursorVisible;
            Refresh();
        }

        if (queryActive) {
            Refresh();
        }

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


    /*
        Initialises the filesystem, displays the motd, and shows the first prompt.
    */
    void Boot() {
        fileSystem.Init();
        foreach (var line in mockOSText) history += line + "\n";
        history += "\n";
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
            if (int.TryParse(answer, out int val) && val == QueryPool[queryAnswer].answer)
                ExitQuery(timedOut: false, correct: true);
            else
                ExitQuery(timedOut: false, correct: false);
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
            case "pwd":   CmdPwd();      break;
            case "cat":   CmdCat(args);  break;
            case "echo":  CmdEcho(args); break;
            default:
                history += $"{cmd}: command not found\n";
                break;
        }
    }

    /*
        Prints the list of available commands.
    */
    void CmdHelp() =>
        history += "commands: help  clear  ls  cd  pwd  cat  echo\n";


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
        foreach (var d in dir.directories) history += d.name + "/\n";
        foreach (var f in dir.files)       history += f.name + "\n";
    }


    /*
        Changes cwd to the given path, or home if no argument is provided.
    */
    void CmdCd(string[] args) {
        if (args.Length == 0) { fileSystem.cwd = fileSystem.home; return; }
        var dir = Resolve(args[0]);
        if (dir == null) { history += $"cd: {args[0]}: No such file or directory\n"; return; }
        fileSystem.cwd = dir;
    }

    /*
        Prints the absolute path of the current working directory.
    */
    void CmdPwd() => history += GetPath(fileSystem.cwd) + "\n";


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
        Prints all arguments joined by spaces followed by a newline.
    */
    void CmdEcho(string[] args) => history += string.Join(" ", args) + "\n";


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

    private static readonly (string prompt, int answer)[] QueryPool = {
        ("What is 7 + 5?",    12),
        ("What is 9 * 3?",    27),
        ("What is 64 / 8?",    8),
        ("What is 15 - 6?",    9),
        ("What is 12 + 19?",  31),
        ("What is 6 * 7?",    42),
        ("What is 100 - 37?", 63),
        ("What is 8 * 8?",    64),
        ("What is 81 / 9?",    9),
        ("What is 13 + 28?",  41),
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
            string[] cmds   = { "help", "clear", "ls", "cd", "pwd", "cat", "echo" };
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
    void Backspace() {
        if (inputBuffer.Length > 0) { inputBuffer = inputBuffer[..^1]; Refresh(); }
    }


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
            var (prompt, _) = QueryPool[queryAnswer];
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
    }


    /*
        Unsubscribes from keyboard text input when the component is disabled.
    */
    void OnDisable() { if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput; }


    /*
        Stores a printable character for consumption next Update.
    */
    void OnTextInput(char c) { if (c >= 32 && c != 127) _pendingChar = c; }
}