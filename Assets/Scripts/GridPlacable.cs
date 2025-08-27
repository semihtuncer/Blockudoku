using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GridPlacable : MonoBehaviour
{
    #region Variables
    [Header("Setup")]
    public List<GameObject> squares;
    public List<Vector2Int> gridOccupations;

    [Header("States")]
    public bool draggable;
    public bool isDragging;

    Vector3 offset;
    Vector3 startPosition;
    #endregion

    void Start()
    {
        startPosition = transform.position;

        offset = GridManager.instance.fingerOffset;

        foreach (GameObject g in squares)
        {
            g.GetComponent<SpriteRenderer>().color = GridManager.instance.colorUnplaced;
        }
    }
    void Update()
    {
        if (GameManager.instance.gameOver)
        {
            if (isDragging)
                Drop();

            if (!draggable)
            {
                foreach (GameObject g in squares)
                {
                    g.GetComponent<SpriteRenderer>().color = GridManager.instance.colorDisabled;
                }
            }

            draggable = false;

            return;
        }
        else
        {
            if (!draggable)
            {
                foreach (GameObject g in squares)
                {
                    g.GetComponent<SpriteRenderer>().color = GridManager.instance.colorDisabled;
                }
            }
            else if (!isDragging)
            {
                foreach (GameObject g in squares)
                {
                    g.GetComponent<SpriteRenderer>().color = GridManager.instance.colorUnplaced;
                }
            }
        }

        bool[] check = new bool[GridManager.instance.width * GridManager.instance.height];
        int i = 0;

        for (int x = 0; x < GridManager.instance.width; x++)
        {
            for (int y = 0; y < GridManager.instance.height; y++)
            {
                if(CheckIfPlacable(new Vector2(x, y)))
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

        if (check.ToList().Contains(true))
            draggable = true;
        else
            draggable = false;
    }

    void OnMouseDown()
    {
        if (GameManager.instance.gameOver)
            return;

        StartDrag();
    }
    void OnMouseDrag()
    {
        if (GameManager.instance.gameOver)
            return;

        Drag();
    }
    void OnMouseUp()
    {
        if (GameManager.instance.gameOver)
            return;

        Drop();
    }

    public void StartDrag()
    {
        if (!draggable)
            return;

        offset = new Vector3(offset.x, offset.y, 20);

        foreach (GameObject a in squares)
        {
            a.GetComponent<Animator>().SetBool("Grab", true);
        }

        transform.localScale = Vector3.one;
    }
    public void Drag()
    {
        if (!draggable)
            return;

        isDragging = true;

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = Vector2.Lerp(transform.position, curPosition, Time.deltaTime * GridManager.instance.lerpSpeed);

        if (!CheckIfPlacable())
        {
            foreach (GameObject g in squares)
            {
                g.GetComponent<SpriteRenderer>().color = GridManager.instance.colorBlocked;
            }

            GridManager.instance.Unhighlight();
        }
        else
        {
            foreach (GameObject g in squares)
            {
                g.GetComponent<SpriteRenderer>().color = GridManager.instance.colorUnplaced;
            }

            List<Vector2> poses = new List<Vector2>();

            for (int i = 0; i < gridOccupations.Count; i++)
            {
                poses.Add(gridOccupations[i] + GridManager.instance.gridMousePos + (Vector2)offset);
            }

            GridManager.instance.Highlight(poses);
        }
    }
    public void Drop()
    {
        if (!draggable)
            return;

        if (CheckIfPlacable())
        {
            List<Vector2> p = new List<Vector2>();

            foreach (Vector2Int occ in gridOccupations)
            {
                p.Add(occ + GridManager.instance.gridMousePos + (Vector2)offset);
            }

            GridManager.instance.Place(p, this.gameObject);

            DestroyImmediate(gameObject);

            GridManager.instance.Unhighlight();

            NewTileManager.instance.CheckForTile();
        }
        else
        {
            isDragging = false;

            foreach (GameObject a in squares)
            {
                a.GetComponent<Animator>().SetBool("Grab", false);
                a.GetComponent<SpriteRenderer>().color = GridManager.instance.colorUnplaced;
            }

            transform.position = startPosition;
            transform.localScale = new Vector3(GridManager.instance.startScale, GridManager.instance.startScale, GridManager.instance.startScale);

            GridManager.instance.Unhighlight();
        }
    }

    public bool CheckIfPlacable()
    {
        bool[] allPlacable = new bool[gridOccupations.Count];

        int i = 0;

        foreach (Vector2Int g in gridOccupations)
        {
            if (!GridManager.instance.CheckGridCellExistance(g + GridManager.instance.gridMousePos + (Vector2)offset))
            {
                allPlacable[i] = false;
            }
            else
            {
                allPlacable[i] = true;
            }

            i++;
        }

        if (allPlacable.ToList().Contains(false))
            return false;
        else
            return true;
    }
    public bool CheckIfPlacable(Vector2 pos)
    {
        bool[] allPlacable = new bool[gridOccupations.Count];

        int i = 0;

        foreach (Vector2Int g in gridOccupations)
        {
            if (!GridManager.instance.CheckGridCellExistance(pos + g))
            {
                allPlacable[i] = false;
            }
            else
            {
                allPlacable[i] = true;
            }

            i++;
        }

        if (allPlacable.ToList().Contains(false))
            return false;
        else
            return true;
    }
}