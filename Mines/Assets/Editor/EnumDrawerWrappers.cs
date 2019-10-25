
using UnityEditor;
using UnityEngine;
/// <summary>
/// Draw wrapper for UICollections
/// </summary>
[CustomPropertyDrawer(typeof(UICollection))]
public class SomethingCollectionDrawer : EnumCollectionDrawer<GameObject, UIElements> { }