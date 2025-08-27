using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region Variables
    [Header("Undo")]
    public Dictionary<SaveCell[,], string> moves = new Dictionary<SaveCell[,], string>();
    public int undoCount;
    public int maxUndoCount;

    [Header("Setup")]
    public int width;
    public int height;
    public Vector3 fingerOffset;
    public float lerpSpeed;
    public float startScale;

    [Space(5)]
    public Transform background;
    public GameObject gridHolderGO;

    [Space(5)]
    public GridCell currentGridCell;
    public Vector2Int gridMousePos;

    [Header("Colors")]
    public Color color1;
    public Color color2;
    public Color colorFull;
    public Color colorBlocked;
    public Color colorUnplaced;
    public Color colorHighlighted;
    public Color colorDisabled;

    public GridCell[,] gridCells;

    public static GridManager instance;
    #endregion

    void Awake()
    {
        instance = this;

        undoCount = maxUndoCount;

        GenerateGrid();
    }
    void Update()
    {
        CheckWinStates();

        UpdateCells();
        GetCurrentCell();

        if (GameManager.instance.debug)
        {
            if (GameManager.instance.gameOver)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (CheckGridCellExistance(gridMousePos))
                {
                    Place(gridMousePos);
                }
            }
        }
    }
    void LateUpdate()
    {
        CheckLooseStates();
    }

    void CheckWinStates()
    {
        // Check columns
        Dictionary<int, bool> checkColumns = new Dictionary<int, bool>();
        for (int x = 0; x < width; x++)
        {
            if (IsColFull(x, true))
            {
                HighlightCol(x);
            }

            checkColumns[x] = IsColFull(x, false);
        }
        // Check rows
        Dictionary<int, bool> checkRows = new Dictionary<int, bool>();
        for (int y = 0; y < height; y++)
        {
            if (IsRowFull(y, true))
            {
                HighlightRow(y);
            }

            checkRows[y] = IsRowFull(y, false);
        }
        // Check squares
        Dictionary<Vector2Int, bool> checkSquares = new Dictionary<Vector2Int, bool>();
        for (int x = 0; x < width / 3; x++)
        {
            for (int y = 0; y < height / 3; y++)
            {
                if (IsSquareFull(x, y, true))
                {
                    HighlightSquare(x, y);
                }

                checkSquares[new Vector2Int(x, y)] = IsSquareFull(x, y, false);
            }
        }

        // Win columns
        for (int x = 0; x < checkColumns.Count; x++)
        {
            if (checkColumns[x])
            {
                foreach (GridCell c in GetColCells(x))
                {
                    c.state = false;
                    c.highlighted = false;

                    c.cellGO.GetComponent<Animator>().SetTrigger("Collected");
                    c.cellGO.GetComponent<Animator>().SetBool("Grab", false);

                    GameManager.instance.ChangeScore(GameManager.instance.scoreIncreaseOnSquare, true);
                }
            }
        }
        // Win rows
        for (int y = 0; y < checkRows.Count; y++)
        {
            if (checkRows[y])
            {
                foreach (GridCell c in GetRowCells(y))
                {
                    c.state = false;
                    c.highlighted = false;

                    c.cellGO.GetComponent<Animator>().SetTrigger("Collected");
                    c.cellGO.GetComponent<Animator>().SetBool("Grab", false);

                    GameManager.instance.ChangeScore(GameManager.instance.scoreIncreaseOnSquare, true);
                }
            }
        }
        // Win squares
        for (int x = 0; x < width / 3; x++)
        {
            for (int y = 0; y < height / 3; y++)
            {
                if (checkSquares[new Vector2Int(x, y)])
                {
                    foreach (GridCell c in GetSquareCells(x, y))
                    {
                        c.state = false;
                        c.highlighted = false;

                        c.cellGO.GetComponent<Animator>().SetTrigger("Collected");
                        c.cellGO.GetComponent<Animator>().SetBool("Grab", false);

                        GameManager.instance.ChangeScore(GameManager.instance.scoreIncreaseOnSquare, true);
                    }
                }
            }
        }
    }
    void CheckLooseStates()
    {
        if (GameManager.instance.gameOver)
            return;

        List<GridPlacable> gps = new List<GridPlacable>();

        foreach (GameObject go in NewTileManager.instance.spawnedTiles)
        {
            if (go != null)
                gps.Add(go.GetComponent<GridPlacable>());
        }

        if (gps.Count == 0)
            return;

        bool[] check = new bool[gps.Count];
        int i = 0;

        foreach (GridPlacable g in gps)
        {
            if (g.gameObject != null)
            {
                if (g.draggable)
                {
                    check[i] = true;
                }
                else
                {
                    check[i] = false;
                }

                i++;
            }
        }

        if (!check.Contains(true))
        {
            UIManager.instance.GoToPage("gameover");
            GameManager.instance.gameOver = true;
        }
    }

    void GenerateGrid()
    {
        gridCells = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridCells[x, y] = new GridCell();

                GameObject t = Instantiate(gridHolderGO, new Vector3(x, y, 0), Quaternion.identity, transform);
                t.name = $"{x}, {y}";

                gridCells[x, y].state = false;
                gridCells[x, y].worldPos = t.transform.position;
                gridCells[x, y].cellGO = t;

                if (x / 3 != 1)
                {
                    if (y / 3 == 1)
                    {
                        t.GetComponent<SpriteRenderer>().color = color2;
                    }
                    else
                    {
                        t.GetComponent<SpriteRenderer>().color = color1;
                    }
                }
                else
                {
                    if (y / 3 == 1)
                    {
                        t.GetComponent<SpriteRenderer>().color = color1;
                    }
                    else
                    {
                        t.GetComponent<SpriteRenderer>().color = color2;
                    }
                }
            }
        }

        transform.position = new Vector3(-width / 2, -height / 2 + 1, 0);

        background.localScale = new Vector3(width + 0.5f, height + 0.5f, 1);
        background.localPosition = new Vector3(-transform.position.x, -transform.position.y + 1, 0);

        Camera.main.orthographicSize = width + 1;
    }
    void GetCurrentCell()
    {
        if (GameManager.instance.gameOver)
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gridMousePos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y)) - new Vector2Int((int)transform.position.x, (int)transform.position.y);

        foreach (GridCell gc in gridCells)
        {
            if (gc.worldPos == gridMousePos)
            {
                currentGridCell = gc;
            }
        }
    }

    void UpdateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridCells[x, y].state)
                {
                    gridCells[x, y].cellGO.GetComponent<SpriteRenderer>().color = colorFull;
                }
                else
                {
                    if (gridCells[x, y].highlighted)
                    {
                        gridCells[x, y].cellGO.GetComponent<SpriteRenderer>().color = colorHighlighted;
                    }
                    else
                    {
                        if (x / 3 != 1)
                        {
                            if (y / 3 == 1)
                            {
                                gridCells[x, y].cellGO.GetComponent<SpriteRenderer>().color = color2;
                            }
                            else
                            {
                                gridCells[x, y].cellGO.GetComponent<SpriteRenderer>().color = color1;
                            }
                        }
                        else
                        {
                            if (y / 3 == 1)
                            {
                                gridCells[x, y].cellGO.GetComponent<SpriteRenderer>().color = color1;
                            }
                            else
                            {
                                gridCells[x, y].cellGO.GetComponent<SpriteRenderer>().color = color2;
                            }
                        }
                    }
                }
            }
        }
    }

    public void Place(Vector2 pos)
    {
        SaveGridForUndo(null);

        gridCells[(int)pos.x, (int)pos.y].state = true;
        gridCells[(int)pos.x, (int)pos.y].highlighted = false;

        GameManager.instance.ChangeScore(GameManager.instance.scoreIncreaseOnPlace, false);
        AudioManager.instance.PlaySound("place");
    }
    public void Place(List<Vector2> poses, GameObject placed)
    {
        SaveGridForUndo(placed);

        foreach (var pos in poses)
        {
            gridCells[(int)pos.x, (int)pos.y].state = true;
            gridCells[(int)pos.x, (int)pos.y].highlighted = false;

            GameManager.instance.ChangeScore(GameManager.instance.scoreIncreaseOnPlace, false);
            AudioManager.instance.PlaySound("place");
        }
    }

    public void Undo()
    {
        if (GameManager.instance.gameOver)
            return;

        if (moves.Count == 0)
            return;

        if (undoCount == 0)
            return;

        undoCount--;

        LoadGridForUndo();
    }

    void LoadGridForUndo()
    {
        SaveCell[,] s = moves.Keys.ToArray()[moves.Count - 1];
        NewTileManager.instance.AddTile(moves[s]);

        moves.Remove(s);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridCells[x, y].state = s[x, y].state;
                gridCells[x, y].worldPos = s[x, y].worldPos;
            }
        }
    }
    void SaveGridForUndo(GameObject placed)
    {
        SaveCell[,] s = new SaveCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                s[x, y] = new SaveCell(gridCells[x, y].state, gridCells[x, y].worldPos);
            }
        }

        if (placed != null)
            moves.Add(s, placed.name);
        else
            moves.Add(s, "");
    }

    public void Highlight(Vector2 p)
    {
        foreach (GridCell gc in gridCells)
        {
            gc.highlighted = false;
            gc.cellGO.GetComponent<Animator>().SetBool("Grab", false);
        }

        gridCells[(int)p.x, (int)p.y].highlighted = true;
        gridCells[(int)p.x, (int)p.y].cellGO.GetComponent<Animator>().SetBool("Grab", true);
    }
    public void Highlight(List<Vector2> poses)
    {
        foreach (GridCell gc in gridCells)
        {
            gc.highlighted = false;
            gc.cellGO.GetComponent<Animator>().SetBool("Grab", false);
        }

        foreach (Vector2 p in poses)
        {
            gridCells[(int)p.x, (int)p.y].highlighted = true;
            gridCells[(int)p.x, (int)p.y].cellGO.GetComponent<Animator>().SetBool("Grab", true);
        }
    }
    public void Highlight(List<GridCell> gCells, bool dehighlight)
    {
        if (dehighlight)
        {
            foreach (GridCell gc in gridCells)
            {
                gc.highlighted = false;
                gc.cellGO.GetComponent<Animator>().SetBool("Grab", false);
            }
        }

        foreach (GridCell p in gCells)
        {
            p.highlighted = true;
            p.cellGO.GetComponent<Animator>().SetBool("Grab", true);
        }
    }

    public void HighlightRow(int row)
    {
        for (int x = 0; x < width; x++)
        {
            gridCells[x, row].highlighted = true;
            gridCells[x, row].cellGO.GetComponent<Animator>().SetBool("Grab", true);
        }
    }
    public void HighlightCol(int col)
    {
        for (int y = 0; y < height; y++)
        {
            gridCells[col, y].highlighted = true;
            gridCells[col, y].cellGO.GetComponent<Animator>().SetBool("Grab", true);
        }
    }
    public void HighlightSquare(int sx, int sy)
    {
        int startx = sx * 3;
        int endx = startx + 3;

        int starty = sy * 3;
        int endy = starty + 3;

        for (int y = starty; y < endy; y++)
        {
            for (int x = startx; x < endx; x++)
            {
                gridCells[x, y].highlighted = true;
                gridCells[x, y].cellGO.GetComponent<Animator>().SetBool("Grab", true);
            }
        }
    }

    public void Unhighlight()
    {
        foreach (GridCell gc in gridCells)
        {
            gc.highlighted = false;
            gc.cellGO.GetComponent<Animator>().SetBool("Grab", false);
        }
    }

    public List<GridCell> GetColCells(int col)
    {
        List<GridCell> l = new List<GridCell>();

        for (int y = 0; y < height; y++)
        {
            l.Add(gridCells[col, y]);
        }

        return l;
    }
    public List<GridCell> GetRowCells(int row)
    {
        List<GridCell> l = new List<GridCell>();

        for (int x = 0; x < width; x++)
        {
            l.Add(gridCells[x, row]);
        }

        return l;
    }
    public List<GridCell> GetSquareCells(int sx, int sy)
    {
        List<GridCell> l = new List<GridCell>();

        int startx = sx * 3;
        int endx = startx + 3;

        int starty = sy * 3;
        int endy = starty + 3;

        for (int y = starty; y < endy; y++)
        {
            for (int x = startx; x < endx; x++)
            {
                l.Add(gridCells[x, y]);
            }
        }

        return l;
    }

    public bool CheckGridCellExistance(Vector2 pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
        {
            return false;
        }
        else
        {
            if (!gridCells[(int)pos.x, (int)pos.y].state)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsColFull(int col, bool checkHighlight = true)
    {
        int fillCount = 0;
        for (int y = 0; y < height; y++)
        {
            if (gridCells[col, y].state || (checkHighlight && gridCells[col, y].highlighted))
                fillCount++;
        }

        return (fillCount == height);
    }
    public bool IsRowFull(int row, bool checkHighlight = true)
    {
        int fillCount = 0;
        for (int x = 0; x < width; x++)
        {
            if (gridCells[x, row].state || (checkHighlight && gridCells[x, row].highlighted))
                fillCount++;
        }

        return (fillCount == width);
    }
    public bool IsSquareFull(int sx, int sy, bool checkHighlight = true)
    {
        int fillCount = 0;

        int startx = sx * 3;
        int endx = startx + 3;

        int starty = sy * 3;
        int endy = starty + 3;

        for (int y = starty; y < endy; y++)
        {
            for (int x = startx; x < endx; x++)
            {
                if (gridCells[x, y].state || (checkHighlight && gridCells[x, y].highlighted))
                    fillCount++;
            }
        }

        return fillCount == 9;
    }
}

public class GridCell
{
    public bool state;
    public bool highlighted;

    public Vector2 worldPos;
    public GameObject cellGO;
}
public class SaveCell
{
    public bool state;
    public Vector2 worldPos;

    public SaveCell(bool s, Vector2 wP)
    {
        state = s;
        worldPos = wP;
    }
}