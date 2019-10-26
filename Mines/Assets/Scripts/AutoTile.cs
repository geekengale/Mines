using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AutoTile : TileBase {

    [Flags]
    public enum Direction
    {
        None = 0,
        SouthWest = 1,
        West = 2,
        NorthWest = 4,
        South = 8,
        North = 16,
        SouthEast = 32,
        East = 64,
        NorthEast = 128
    }

    [HideInInspector]
    public Sprite[] sprites;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            {
                var pos = position + new Vector3Int(x, y, 0);
                tilemap.RefreshTile(pos);
            }
    }
    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        var neighbors = Direction.None;

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var testingPosition = position + new Vector3Int(x, y, 0);
                var testTile = tilemap.GetTile<AutoTile>(testingPosition);
                if (testTile && testTile == this)
                {
                    neighbors |= GetDirection(x, y);
                }
            }
        }

        tileData.sprite = sprites[(int)neighbors];
    }
        
    private Direction GetDirection(int x, int y)
    {
        switch(x)
        {
            case -1:
                switch(y)
                {
                    case -1:
                        return Direction.SouthWest;
                    case 0:
                        return Direction.West;
                    case 1:
                        return Direction.NorthWest;
                }
                break;
            case 0:
                switch(y)
                {
                    case -1:
                        return Direction.South;
                    case 1:
                        return Direction.North;
                }
                break;
            case 1:
                switch(y)
                {
                    case -1:
                        return Direction.SouthEast;
                    case 0:
                        return Direction.East;
                    case 1:
                        return Direction.NorthEast;
                }
                break;
        }
        return Direction.None;
    }
}