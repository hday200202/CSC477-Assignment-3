using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FacultyAI : MonoBehaviour {
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    [Tooltip("Leave 0 to auto-derive as a fraction of cellSize.")]
    public float arrivalDistance = 0f;

    [Header("Debug Path Drawing")]
    public bool drawPath = false;

    [Header("Wandering")]
    public float minRestTime = 2f;
    public float maxRestTime = 6f;

    [Header("Pathfinding Grid")]
    [Tooltip("Leave 0 to auto-derive from BoxCollider2D.")]
    public float cellSize = 0f;
    [Tooltip("Clearance fraction of cellSize added to corners in the per-step walkability check.")]
    public float edgePadding = 0.3f;

    [Header("Repulsion")]
    public float repulsionRadius = 0.9f;
    public float repulsionStrength = 1.2f;

    private static readonly List<FacultyAI> all = new List<FacultyAI>();

    private List<Collider2D> walkableColliders = new List<Collider2D>();
    private bool[,] grid;
    private Vector2 gridOrigin;
    private int gridWidth, gridHeight;

    private List<Vector2> currentPath = new List<Vector2>();
    private int pathIndex;
    private bool isResting;
    private LineRenderer pathLine;
    private Vector2 colliderHalfExtents;
    private Vector2 colliderOffset;
    private Vector2 currentDest;

    void Awake() {
        all.RemoveAll(f => f == null);
        all.Add(this);

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Walkable"))
            foreach (Collider2D col in go.GetComponents<Collider2D>())
                if (col != null) walkableColliders.Add(col);

        if (walkableColliders.Count == 0)
            Debug.LogWarning("[FacultyAI] No Walkable colliders found.");
    }

    void OnDestroy() { all.Remove(this); }

    void Start() {
        // Read scale here so all parent Awake/Start transforms are settled.
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null) {
            Vector2 s = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            colliderHalfExtents = box.size * 0.5f * s;
            colliderOffset = box.offset * s;
        } else {
            colliderHalfExtents = Vector2.one * 0.25f;
            colliderOffset = Vector2.zero;
            Debug.LogWarning("[FacultyAI] No BoxCollider2D found.");
        }

        if (cellSize <= 0f)
            cellSize = Mathf.Min(colliderHalfExtents.x, colliderHalfExtents.y);

        if (arrivalDistance <= 0f)
            arrivalDistance = cellSize * 0.6f;

        if (drawPath) {
            pathLine = gameObject.AddComponent<LineRenderer>();
            pathLine.material = new Material(Shader.Find("Sprites/Default"));
            pathLine.startWidth = cellSize * 0.5f;
            pathLine.endWidth = cellSize * 0.5f;
            pathLine.useWorldSpace = true;
            pathLine.sortingOrder = 999;
            pathLine.positionCount = 0;
        }

        if (walkableColliders.Count == 0) return;
        BuildGrid();
        StartCoroutine(StaggeredStart());
    }

    IEnumerator StaggeredStart() {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));
        StartCoroutine(WanderLoop());
    }

    void BuildGrid() {
        Bounds total = walkableColliders[0].bounds;
        foreach (Collider2D col in walkableColliders) total.Encapsulate(col.bounds);
        total.Expand(cellSize * 2f);

        gridOrigin = new Vector2(total.min.x, total.min.y);
        gridWidth = Mathf.CeilToInt(total.size.x / cellSize);
        gridHeight = Mathf.CeilToInt(total.size.y / cellSize);
        grid = new bool[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                grid[x, y] = IsWalkableCenter(GridToWorld(x, y));

        Debug.Log($"[FacultyAI] Grid {gridWidth}x{gridHeight} built.");
    }

    void Update() {
        if (pathLine != null) {
            int remaining = Mathf.Max(0, currentPath.Count - pathIndex);
            if (remaining == 0) {
                pathLine.positionCount = 0;
            } else {
                pathLine.positionCount = remaining + 1;
                pathLine.SetPosition(0, transform.position);
                for (int i = 0; i < remaining; i++)
                    pathLine.SetPosition(i + 1, (Vector3)currentPath[pathIndex + i] + Vector3.back * 0.1f);
                pathLine.startColor = pathLine.endColor = isResting ? Color.yellow : Color.cyan;
            }
        }

        Vector2 repulsion = Vector2.zero;
        Vector2 myCenter = (Vector2)transform.position + colliderOffset;
        foreach (var f in all) {
            if (f == this) continue;
            Vector2 otherCenter = (Vector2)f.transform.position + f.colliderOffset;
            float dist = Vector2.Distance(myCenter, otherCenter);
            if (dist < repulsionRadius && dist > 0.001f) {
                float t = 1f - (dist / repulsionRadius);
                repulsion += (myCenter - otherCenter).normalized * (t * repulsionStrength);
            }
        }
        if (repulsion.sqrMagnitude > 0f) {
            Vector2 nudged = (Vector2)transform.position + repulsion * Time.deltaTime;
            if (IsWalkableWorld(nudged))
                transform.position = nudged;
        }
    }

    IEnumerator WanderLoop() {
        while (true) {
            currentDest = PickDestination();
            currentPath = FindPath((Vector2)transform.position, currentDest);
            pathIndex = 0;

            if (currentPath.Count == 0) { yield return new WaitForSeconds(0.5f); continue; }

            int stuckFrames = 0;

            while (pathIndex < currentPath.Count) {
                Vector2 waypoint = currentPath[pathIndex];
                Vector2 toWaypoint = waypoint - (Vector2)transform.position;

                if (toWaypoint.magnitude <= arrivalDistance) {
                    pathIndex++;
                    stuckFrames = 0;
                    yield return null;
                    continue;
                }

                Vector2 dir = toWaypoint.normalized;
                Vector2 nextPos = (Vector2)transform.position + dir * moveSpeed * Time.deltaTime;

                if (IsWalkableWorld(nextPos)) {
                    transform.position = nextPos;
                    stuckFrames = 0;
                } else {
                    stuckFrames++;
                    if (stuckFrames > 30) {
                        pathIndex++;
                        stuckFrames = 0;
                    }
                }

                yield return null;
            }

            isResting = true;
            yield return new WaitForSeconds(Random.Range(minRestTime, maxRestTime));
            isResting = false;
        }
    }

    Vector2 PickDestination() {
        for (int attempt = 0; attempt < 30; attempt++) {
            Collider2D col = walkableColliders[Random.Range(0, walkableColliders.Count)];
            Bounds b = col.bounds;
            Vector2 c = new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
            if (IsWalkableCenter(c)) return c;
        }
        return transform.position;
    }

    /* A* pathfinding on the boolean grid. Snaps non-walkable start/end to the nearest
       walkable cell. Uses octile heuristic with no-corner-cutting diagonal moves. */
    List<Vector2> FindPath(Vector2 startWorld, Vector2 endWorld) {
        Vector2Int start = WorldToGrid(startWorld);
        Vector2Int end = WorldToGrid(endWorld);

        if (!InBounds(start) || !InBounds(end)) return new List<Vector2>();

        if (!grid[start.x, start.y]) start = NearestWalkable(start);
        if (!grid[end.x, end.y]) end = NearestWalkable(end);

        if (start == end) return new List<Vector2>();

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, end) };
        var openSet = new List<Vector2Int> { start };
        var closedSet = new HashSet<Vector2Int>();

        Vector2Int[] cardinals = {
            new Vector2Int( 1, 0), new Vector2Int(-1, 0),
            new Vector2Int( 0, 1), new Vector2Int( 0,-1),
        };
        Vector2Int[] diagonals = {
            new Vector2Int( 1, 1), new Vector2Int(-1, 1),
            new Vector2Int( 1,-1), new Vector2Int(-1,-1),
        };

        while (openSet.Count > 0) {
            Vector2Int current = openSet[0];
            float bestF = fScore.ContainsKey(current) ? fScore[current] : float.MaxValue;
            foreach (var n in openSet) {
                float f = fScore.ContainsKey(n) ? fScore[n] : float.MaxValue;
                if (f < bestF) { bestF = f; current = n; }
            }

            if (current == end) return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var d in cardinals) {
                Vector2Int nb = current + d;
                if (!InBounds(nb) || !grid[nb.x, nb.y] || closedSet.Contains(nb)) continue;

                float tentativeG = (gScore.ContainsKey(current) ? gScore[current] : float.MaxValue) + 1f;
                if (!gScore.ContainsKey(nb) || tentativeG < gScore[nb]) {
                    cameFrom[nb] = current;
                    gScore[nb] = tentativeG;
                    fScore[nb] = tentativeG + Heuristic(nb, end);
                    if (!openSet.Contains(nb)) openSet.Add(nb);
                }
            }

            foreach (var d in diagonals) {
                Vector2Int nb = current + d;
                Vector2Int cardA = new Vector2Int(current.x + d.x, current.y);
                Vector2Int cardB = new Vector2Int(current.x, current.y + d.y);
                if (!InBounds(nb) || !grid[nb.x, nb.y] || closedSet.Contains(nb)) continue;
                if (!InBounds(cardA) || !grid[cardA.x, cardA.y]) continue;
                if (!InBounds(cardB) || !grid[cardB.x, cardB.y]) continue;

                float tentativeG = (gScore.ContainsKey(current) ? gScore[current] : float.MaxValue) + 1.414f;
                if (!gScore.ContainsKey(nb) || tentativeG < gScore[nb]) {
                    cameFrom[nb] = current;
                    gScore[nb] = tentativeG;
                    fScore[nb] = tentativeG + Heuristic(nb, end);
                    if (!openSet.Contains(nb)) openSet.Add(nb);
                }
            }
        }
        return new List<Vector2>();
    }

    Vector2Int NearestWalkable(Vector2Int from) {
        float best = float.MaxValue;
        Vector2Int result = from;
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (grid[x, y]) {
                    float d = Vector2Int.Distance(new Vector2Int(x, y), from);
                    if (d < best) { best = d; result = new Vector2Int(x, y); }
                }
        return result;
    }

    List<Vector2> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int node) {
        var path = new List<Vector2>();
        while (cameFrom.ContainsKey(node)) {
            path.Add(GridToWorld(node.x, node.y));
            node = cameFrom[node];
        }
        path.Reverse();
        return path;
    }

    float Heuristic(Vector2Int a, Vector2Int b) {
        int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy) + (1.414f - 1f) * Mathf.Min(dx, dy);
    }

    Vector2 GridToWorld(int x, int y)
        => gridOrigin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);

    Vector2Int WorldToGrid(Vector2 w)
        => new Vector2Int(
            Mathf.Clamp(Mathf.FloorToInt((w.x - gridOrigin.x) / cellSize), 0, gridWidth - 1),
            Mathf.Clamp(Mathf.FloorToInt((w.y - gridOrigin.y) / cellSize), 0, gridHeight - 1));

    bool InBounds(Vector2Int p)
        => p.x >= 0 && p.x < gridWidth && p.y >= 0 && p.y < gridHeight;

    // Center-point check used for grid construction and destination picking.
    bool IsWalkableCenter(Vector2 center) {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center + colliderOffset, cellSize * 0.4f);
        foreach (var hit in hits)
            if (hit.gameObject != gameObject && hit.CompareTag("Walkable"))
                return true;
        return false;
    }

    // Four-corner padded check used for per-frame movement to keep the sprite off edges.
    bool IsWalkableWorld(Vector2 center) {
        Vector2 c = center + colliderOffset;
        float px = colliderHalfExtents.x + edgePadding * cellSize;
        float py = colliderHalfExtents.y + edgePadding * cellSize;
        Vector2[] corners = {
            c + new Vector2(-px, -py),
            c + new Vector2( px, -py),
            c + new Vector2(-px,  py),
            c + new Vector2( px,  py),
        };
        foreach (Vector2 corner in corners) {
            bool ok = false;
            foreach (var hit in Physics2D.OverlapCircleAll(corner, cellSize * 0.25f))
                if (hit.gameObject != gameObject && hit.CompareTag("Walkable"))
                { ok = true; break; }
            if (!ok) return false;
        }
        return true;
    }
}
