using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileMaps 
{
    Ground,
    Decoration
}

[Serializable]
public class TileMapCollection : GenericEnumCollection<Tilemap, TileMaps> {}