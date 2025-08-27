using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : Singleton<PathFinding>
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private List<Node> grids;
    private bool isMoving = false;

    #region Unity Methods

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            Vector3Int mouseGridPos = GetMouseGridPosition();
            Vector3Int playerGridPos = GetPlayerGridPos();
            Node start = grids.Find(n => n.x == playerGridPos.x && n.y == playerGridPos.z);
            Node end = grids.Find(n => n.x == mouseGridPos.x && n.y == mouseGridPos.z);

            visited.Clear();
            path.Clear();
            DFS(start, end);

            isMoving = true;
            HighLightPath();
            MoveAlongPath();
        }
    }

    #endregion

    #region Get Methods

    public Vector3Int GetMouseGridPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundMask))
        {
            Vector3 hitPos = hit.point;

            Vector3Int mousePosInt = new Vector3Int(
                Mathf.RoundToInt(hitPos.x),
                Mathf.RoundToInt(hitPos.y),
                Mathf.RoundToInt(hitPos.z)
            );

            Debug.Log($"Transform EndNode gridPos = {mousePosInt}");

            return mousePosInt;
        }

        Debug.Log("Không raycast trúng ground!");
        return Vector3Int.zero;
    }

    public void GetGrids(List<Node> grids)
    {
        this.grids = grids;
    }

    public Vector3Int GetPlayerGridPos()
    {
        Vector3Int gridPos = new Vector3Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            Mathf.RoundToInt(transform.position.z)
        );

        Debug.Log($"Transform {transform.name} gridPos = {gridPos}");

        return gridPos;
    }

    #endregion

    #region DFS 

    [SerializeField] private List<Node> visited = new List<Node>();
    [SerializeField] private List<Node> waiting = new List<Node>();
    [SerializeField] private List<Node> path = new List<Node>();

    List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private void DFS(Node start, Node end)
    {
        Node currentNode = start;
        if (end.isWall)
        {
            Debug.Log("Không đi được vào tường");
            return;
        }

        if (!visited.Contains(currentNode))
            visited.Add(currentNode);

        if(!path.Contains(currentNode))
            path.Add(currentNode);

        // Find Neighbor 
        waiting.Clear();
        foreach (var dir in directions)
        {
            int neiX = currentNode.x + dir.x;
            int neiY = currentNode.y + dir.y;

            Node neighborNode = grids.Find(n => n.x == neiX && n.y == neiY);
            if (neighborNode != null && !visited.Contains(neighborNode) && !neighborNode.isWall)
            {
                waiting.Add(neighborNode);
            }
        }

        Node goalNode = waiting.Find(n => n.x == end.x && n.y == end.y);
        if (goalNode != null)
        {
            if (!path.Contains(goalNode))
            {
                path.Add(goalNode);
            }
            Debug.Log("Tìm thấy Node đích");
            return;
        }

        Node nextNode;

        if (waiting.Count > 0)
        {
            nextNode = waiting[0];

            currentNode = nextNode;
            if (!path.Contains(currentNode))
            {
                path.Add(currentNode);
            }
            DFS(currentNode, end);
        }
        else
        {
            if (path.Count > 0)
            {
                path.RemoveAt(path.Count - 1);
                currentNode = path[path.Count - 1];
                DFS(currentNode, end);
            }
        }
    }

    #endregion

    #region Move To Path 

    [SerializeField] private float moveSpeed = 3f;

    private void MoveAlongPath()
    {
        if (path == null || path.Count == 0) return;

        StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        foreach (var node in path)
        {
            Vector3 targetPos = new Vector3(node.x, transform.position.y, node.y); 
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null; 
            }
        }

        isMoving = false;
        ResetHighLightPath();
    }

    [SerializeField] private Material highlightMaterial; 

    private void HighLightPath()
    {
        Transform contain = MazeGenerator.Instance.ContainFloor;

        foreach(var node in path)
        {
            foreach (Transform child in contain)
            {
                if (Mathf.RoundToInt(child.position.x) == node.x &&
                Mathf.RoundToInt(child.position.z) == node.y)
                {
                    Renderer renderer = child.GetComponent<Renderer>();
                    if(renderer != null)
                    {
                        renderer.material.color = Color.green;
                    }
                }
            }
        }
    }

    private void ResetHighLightPath()
    {
        Transform contain = MazeGenerator.Instance.ContainFloor;

        foreach (var node in path)
        {
            foreach (Transform child in contain)
            {
                if (Mathf.RoundToInt(child.position.x) == node.x &&
                Mathf.RoundToInt(child.position.z) == node.y)
                {
                    Renderer renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.white;
                    }
                }
            }
        }
    }

    #endregion
}
