using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class MazeGenerator : Singleton<MazeGenerator>
{
    [Header("Input Property")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private Transform containWall;
    [SerializeField] private Transform containFloor;

    public Transform ContainFloor => containFloor;

    // Algorithm_Type
    [SerializeField] private Algorithm_Type algorithm_Type;
    [SerializeField] private List<Node> grids = new List<Node>();

    public List<Node> Grids => grids;

    private void Start()
    {
        CameraSetup();
        ChooseAlgorithmType(algorithm_Type);
    }

    #region Ready Methods

    private void CameraSetup()
    {
        Camera.main.transform.position = new Vector3(width / 2, width <= height ? height + 2 : width + 2, height / 2);
    }

    private void ChooseAlgorithmType(Algorithm_Type algorithm_Type)
    {
        switch (algorithm_Type)
        {
            case Algorithm_Type.DFS:
                CreateMazeByDFS();
                break;
            default:
                break;
        }
    }

    #endregion

    #region DFS 

    private void CreateMazeByDFS()
    {
        FillWall();

        DFS(grids[0]);

        StartCoroutine(DestroyThenGen(grids));

        PathFinding.Instance.GetGrids(grids);
    }

    private void FillWall()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = new Node()
                {
                    x = x,
                    y = y,
                    isWall = true,
                };
                grids.Add(node);
            }
        }

        GenWalls(grids);
    }

    private void GenWalls(List<Node> grids)
    {
        foreach (Node node in grids)
        {
            if (node.isWall)
            {
                Instantiate(wallPrefab, new Vector3(node.x, 0, node.y), Quaternion.identity, containWall);
                Instantiate(floorPrefab, new Vector3(node.x, -1, node.y), Quaternion.identity, containFloor);
            }
        }
    }

    private IEnumerator GenBound()
    {
        for (int x = -1; x <= width; x++)
        {
            for (int y = -1; y <= height; y++)
            {
                bool isBorder = (x == -1 || y == -1 || x == width || y == height);

                if (isBorder)
                {
                    if ((x == 0 && y == -1) || (x == width - 1 && y == height))
                        continue;

                    Instantiate(wallPrefab, new Vector3(x, 0, y), Quaternion.identity, containWall);

                    yield return new WaitForSeconds(0.001f);
                }
            }
        }
    }


    private IEnumerator DestroyWall(List<Node> grids)
    {
        foreach (Node node in grids)
        {
            if (!node.isWall)
            {
                foreach (Transform child in containWall)
                {
                    if (child.position.x == node.x && child.position.z == node.y)
                    {
                        Destroy(child.gameObject);
                        yield return new WaitForSeconds(0.001f);
                    }
                }
            }
        }
    }

    private IEnumerator DestroyThenGen(List<Node> grids)
    {
        yield return StartCoroutine(DestroyWall(grids));
        yield return StartCoroutine(GenBound());
    }

    [SerializeField] private List<Node> visited = new List<Node>();
    [SerializeField] private List<Node> waiting = new List<Node>();
    [SerializeField] private List<Node> path = new List<Node>();

    List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int(2, 0),
        new Vector2Int(-2, 0),
        new Vector2Int(0, 2),
        new Vector2Int(0, -2)
    };

    private void DFS(Node start)
    {
        Node currentNode = start;
        currentNode.isWall = false;
        if (!visited.Contains(currentNode))
            visited.Add(currentNode);

        // Find Neigbor
        waiting.Clear();
        foreach (var dir in directions)
        {
            int neiX = currentNode.x + dir.x;
            int neiY = currentNode.y + dir.y;

            Node neighborNode = grids.Find(n => n.x == neiX && n.y == neiY);
            if (neighborNode != null && !visited.Contains(neighborNode))
            {
                waiting.Add(neighborNode);
            }
        }

        Shuffle(waiting);
        Node nextNode;

        if (waiting.Count > 0)
        {
            nextNode = waiting[0];

            int midX = (currentNode.x + nextNode.x) / 2;
            int midY = (currentNode.y + nextNode.y) / 2;

            Node middleNode = grids.Find(n => n.x == midX && n.y == midY);
            if (middleNode != null && middleNode.isWall)
                middleNode.isWall = false;

            currentNode = nextNode;
            if (!path.Contains(currentNode))
            {
                path.Add(currentNode);
            }
            DFS(currentNode);
        }
        else
        {
            if (path.Count > 0)
            {
                currentNode = path[path.Count - 1];
                path.RemoveAt(path.Count - 1);
                DFS(currentNode);
            }
            else
            {
                return;
            }
        }

    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    #endregion
}

public enum Algorithm_Type
{
    DFS,
}

[System.Serializable]
public class Node
{
    public int x;
    public int y;
    public bool isWall;
}