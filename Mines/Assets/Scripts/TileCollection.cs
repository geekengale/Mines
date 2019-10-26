using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tiles
{
    Grass,
    Sand,
    Star,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight
}

[Serializable]
public class TileCollection : GenericEnumCollection<TileBase, Tiles> {}