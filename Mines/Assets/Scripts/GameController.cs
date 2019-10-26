using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    public Vector2Int BoardSize = new Vector2Int(10,10);
    public float hazardPercentage = 0.1f;

    public TileMapCollection Tilemaps;
    public TileCollection TileCollection;

    private Vector2Int bottomleft;
    private Vector2Int topright;

    private int[] Mines;
    private bool[] Dug;

    private void Start()
    {
        Mines = new int[BoardSize.x * BoardSize.y];
        Dug = new bool[BoardSize.x * BoardSize.y];

        var hazards = Enumerable.Range(0, Mines.Length).OrderBy(o => Random.value).Take((int)(Mines.Length * hazardPercentage));
        foreach (var h in hazards)
        {
            Mines[h] = 9;
            //left
            if (h - 1 >= 0 && h % BoardSize.x != 0)
            {
                Mines[h - 1]++;
                //down
                if (h - BoardSize.x - 1 >= 0)
                    Mines[h - BoardSize.x - 1]++;
                //up
                if (h + BoardSize.x - 1 < Mines.Length)
                    Mines[h + BoardSize.x - 1]++;
            }
            //right
            if (h + 1 < Mines.Length && h % BoardSize.x != BoardSize.x - 1)
            {
                Mines[h + 1]++;
                //down
                if (h - BoardSize.x + 1 >= 0)
                    Mines[h - BoardSize.x + 1]++;
                //up
                if (h + BoardSize.x + 1 < Mines.Length)
                    Mines[h + BoardSize.x + 1]++;
            }

            //down
            if (h - BoardSize.x >= 0)
                Mines[h - BoardSize.x]++;
            //up
            if (h + BoardSize.x < Mines.Length)
                Mines[h + BoardSize.x]++;
        }
        
        bottomleft = new Vector2Int(BoardSize.x / 2, BoardSize.y / 2) * -1;
        topright = bottomleft + BoardSize;
        for (var x = bottomleft.x; x < topright.x; x++)
            for (var y = bottomleft.y; y < topright.y; y++)
                Tilemaps[TileMaps.Ground].SetTile(new Vector3Int(x, y, 0), TileCollection[Tiles.Grass]);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var p = (Vector2Int)Tilemaps[TileMaps.Ground].WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (p.x >= bottomleft.x && p.x < topright.x && p.y >= bottomleft.y && p.y < topright.y)
                Dig(p);
        }
    }

    private void Dig(Vector2Int point)
    {
        var p = point - bottomleft;
        var index = p.y * BoardSize.x + p.x;

        if (index < 0 || index >= Mines.Length || Dug[index])
            return;

        var m = Mines[index];
        Dug[index] = true;

        //Always dig this point.
        Tilemaps[TileMaps.Ground].SetTile((Vector3Int)point, TileCollection[Tiles.Sand]);

        //Found A Mine.
        if (m >= 9)
        {
            Tilemaps[TileMaps.Decoration].SetTile((Vector3Int)point, TileCollection[Tiles.Star]);
            return;
        }
        
        //Close to how many mines?
        else if (m > 0 && m < 9)
        {
            Tilemaps[TileMaps.Decoration].SetTile((Vector3Int)point, TileCollection[Tiles.Star + m]);
            return;
        }

        //No mines, lets get close.
        else
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    p = point + new Vector2Int(x, y);
                    if (p.x >= bottomleft.x && p.x < topright.x && p.y >= bottomleft.y && p.y < topright.y)
                        Dig(p);
                }
            }
        }
    }
}