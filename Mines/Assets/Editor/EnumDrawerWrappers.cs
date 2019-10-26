using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomPropertyDrawer(typeof(UICollection))]
public class UICollectionDrawer : EnumCollectionDrawer<GameObject, UIElements> { }

[CustomPropertyDrawer(typeof(TileMapCollection))]
public class TilemapsCollectionDrawer : EnumCollectionDrawer<Tilemap, TileMaps> { }

[CustomPropertyDrawer(typeof(TileCollection))]
public class TileCollectionDrawer : EnumCollectionDrawer<TileBase, Tiles> { }