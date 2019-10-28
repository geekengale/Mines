using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public TileMapCollection Tilemaps;
    public TileCollection TileCollection;
    public GameObject GameOverPanel;
    public Text ResultsText;

    [HideInInspector]
    public Vector3 WorldBottomLeft;
    [HideInInspector]
    public Vector3 WorldTopRight;

    private Vector2Int boardSize = new Vector2Int(40,10);
    private float hazardPercentage = 0.1f;
    
    private Vector2Int bottomLeftCell;
    private Vector2Int topRightCell;
    
    private Cell[] cells;

    public void SetBoardWidth(int width)
    {
        boardSize.x = width;
    }

    public void SetBoardHeight(int height)
    {
        boardSize.y = height;
    }

    public void SetHazardPercentage(float percentage)
    {
        hazardPercentage = percentage;
    }

    public void SetUpGame()
    {
        foreach (var m in Tilemaps.Collection)
            m.ClearAllTiles();

        cells = new Cell[boardSize.x * boardSize.y];
        
        //todo: simplify?
        var hazards = Enumerable.Range(0, cells.Length).OrderBy(o => Random.value).Take((int)(cells.Length * hazardPercentage));
        foreach (var h in hazards)
        {
            cells[h].Mines = 9;
            //left
            if (h - 1 >= 0 && h % boardSize.x != 0)
            {
                cells[h - 1].Mines++;
                //down
                if (h - boardSize.x - 1 >= 0)
                    cells[h - boardSize.x - 1].Mines++;
                //up
                if (h + boardSize.x - 1 < cells.Length)
                    cells[h + boardSize.x - 1].Mines++;
            }
            //right
            if (h + 1 < cells.Length && h % boardSize.x != boardSize.x - 1)
            {
                cells[h + 1].Mines++;
                //down
                if (h - boardSize.x + 1 >= 0)
                    cells[h - boardSize.x + 1].Mines++;
                //up
                if (h + boardSize.x + 1 < cells.Length)
                    cells[h + boardSize.x + 1].Mines++;
            }

            //down
            if (h - boardSize.x >= 0)
                cells[h - boardSize.x].Mines++;
            //up
            if (h + boardSize.x < cells.Length)
                cells[h + boardSize.x].Mines++;
        }
        
        bottomLeftCell = new Vector2Int(boardSize.x / 2, boardSize.y / 2) * -1;
        topRightCell = bottomLeftCell + boardSize - Vector2Int.one;

        WorldBottomLeft = Tilemaps[TileMaps.Decoration].GetCellCenterWorld((Vector3Int)bottomLeftCell);
        WorldBottomLeft += new Vector3(-0.5f, -0.5f, 0);

        WorldTopRight = Tilemaps[TileMaps.Decoration].GetCellCenterWorld((Vector3Int)topRightCell);
        WorldTopRight += new Vector3(0.5f, 0.5f, 0);

        for (var x = bottomLeftCell.x; x <= topRightCell.x; x++)
            for (var y = bottomLeftCell.y; y <= topRightCell.y; y++)
                Tilemaps[TileMaps.Ground].SetTile(new Vector3Int(x, y, 0), TileCollection[Tiles.Grass]);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var p = (Vector2Int)Tilemaps[TileMaps.Ground].WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (p.x >= bottomLeftCell.x && p.x <= topRightCell.x && p.y >= bottomLeftCell.y && p.y <= topRightCell.y)
                Dig(p);
        }
    }

    private void Dig(Vector2Int point)
    {
        var p = point - bottomLeftCell;
        var index = p.y * boardSize.x + p.x;

        if (index < 0 || index >= cells.Length || cells[index].Dug)
            return;

        var m = cells[index].Mines;
        cells[index].Dug = true;

        //Always dig this point.
        Tilemaps[TileMaps.Ground].SetTile((Vector3Int)point, TileCollection[Tiles.Sand]);

        //Found A Mine.
        if (m >= 9)
        {
            Tilemaps[TileMaps.Decoration].SetTile((Vector3Int)point, TileCollection[Tiles.Star]);
            ResultsText.text = "You dug up a mine. You lost.";
            GameOverPanel.SetActive(true);
            return;
        }
        
        //Close to how many mines?
        else if (m > 0 && m < 9)
        {
            Tilemaps[TileMaps.Decoration].SetTile((Vector3Int)point, TileCollection[Tiles.Star + m]);
        }

        //No mines, lets get close.
        else
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    p = point + new Vector2Int(x, y);
                    if (p.x >= bottomLeftCell.x && p.x <= topRightCell.x && p.y >= bottomLeftCell.y && p.y <= topRightCell.y)
                        Dig(p);
                }
            }
        }

        //Debug.LogError(cells.Where(c => c.Mines < 9 && c.Dug != true).Count());
        if (cells.Where(c => c.Mines < 9).All(c => c.Dug))
        {
            ResultsText.text = "You cleared the field. You win.";
            GameOverPanel.SetActive(true);
        }
    }

    private struct Cell
    {
        public int Mines;
        public bool Dug;
    }
}
