using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AutoTileTools
{
    public static void AutoTileGenerate(Texture2D texture, int tileSize, string folder, string[] assetNames)
    {
        #region Defines
        var path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        //tile count horizontal/vertial defined as texture's size / tileSize.
        var tileCount = new Vector2Int(texture.width / tileSize, texture.height / tileSize);

        //AutoTile Sets defined area 2 by 3 tiles.
        //AutoTile Set count defined as tileCountHorizontal / 2 by tileCountVertical / 3.
        var tileSetCount = (tileCount.x / 2) * (tileCount.y / 3);

        //tiles per set on finished texture 47.
        //finished texture tile count defined as tileSetCount * 47
        var finishedTileCount = tileSetCount * 47;

        //texture size defined as sqrt(finishedTileCount) (if result has decimal place add one to casted int value)
        var finishedTileRoot = Mathf.Sqrt(finishedTileCount);
        var finishedSize = (finishedTileRoot % 1 != 0 ? (int)finishedTileRoot + 1 : (int)finishedTileRoot);
        #endregion

        #region DefineCornerStamps
        var allTileCorners = new List<List<Color[]>>();
        var loopOn = tileCount.x / 2;
        for (var i = 0; i < tileSetCount; i++)
        {
            allTileCorners.Add(
                GetCorners(
                    new Vector2Int(
                        (i % loopOn) * tileSize * 2,
                        (i / loopOn + 1) * tileSize * 3
                    ),
                    tileSize / 2,
                    texture
                )
            );
        }
        #endregion

        #region PrintTexture
        var finishedTexture = CreateTexture(finishedSize * (tileSize + 2));
        for (var x = 0; x < allTileCorners.Count; x++)
        {
            var tileCorners = allTileCorners[x];
            for (var y = 0; y < ToPaint.Count; y++)
            {
                var tileIndex = x * ToPaint.Count + y;

                var start = new Vector2Int(
                    tileIndex % finishedSize * tileSize + (tileIndex % finishedSize + 2),
                    tileIndex / finishedSize * tileSize + (tileIndex / finishedSize + 2)
                    );
                var stampIndex = ToPaint[y];

                PaintTile(finishedTexture, start, tileSize / 2,
                    tileCorners[stampIndex[0]], tileCorners[stampIndex[1]],
                    tileCorners[stampIndex[2]], tileCorners[stampIndex[3]]
                );
            }
        }
        #endregion

        #region SaveTexture
        finishedTexture.Apply();
        var pngData = finishedTexture.EncodeToPNG();
        Object.DestroyImmediate(finishedTexture);
        path = "Assets" + folder.Substring(Application.dataPath.Length) + "/AutoTile_" + texture.name + ".png";
        
        File.WriteAllBytes(path, pngData);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        #endregion

        #region PrepareImporter
        importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer.spriteImportMode == SpriteImportMode.Multiple)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = tileSize;
        #endregion

        #region CreateSprites
        List<SpriteMetaData> spriteData = new List<SpriteMetaData>();
        for (var i = 0; i < finishedTileCount; i++)
        {
            spriteData.Add(new SpriteMetaData()
            {
                pivot = new Vector2(0.5f, 0.5f),
                alignment = 9,
                name = (i / 47).ToString("00") + (i % 47).ToString("00"),
                rect = new Rect(i % finishedSize * tileSize + (i % finishedSize + 2), i / finishedSize * tileSize + (i / finishedSize + 2), tileSize, tileSize),
                border = Vector4.one
            });
        }

        importer.spritesheet = spriteData.ToArray();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        #endregion

        #region CreateAssets
        var allTileSprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToList();
		
        for (var i = 0; i < tileSetCount; i++)
        {
            var autotile = ScriptableObject.CreateInstance<AutoTile>();
            autotile.name = (assetNames != null && i < assetNames.Length && assetNames[i] != null ? assetNames[i] : texture.name + "_" + i.ToString("00"));
            autotile.sprites = IndexedSprites(allTileSprites.Skip(i * 47).Take(47).ToList());
            AssetDatabase.CreateAsset(autotile, "Assets" + folder.Substring(Application.dataPath.Length) + "/" + autotile.name + ".asset");
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        #endregion
    }

    private static List<int[]> _CornerIndexesCache;

    private static List<int[]> CornerIndexes
    {
        get
        {
            if (_CornerIndexesCache == null)
            {
                _CornerIndexesCache = new List<int[]>();

                for (var i = 0; i < 256; i++)
                {
                    var d = (AutoTile.Direction)i;
                    _CornerIndexesCache.Add(new int[4] { swIndex(d), seIndex(d), nwIndex(d), neIndex(d) });
                }
            }

            return _CornerIndexesCache;
        }
    }

    private static List<int[]> _ToPaintCache;

    public static List<int[]> ToPaint
    {
        get
        {
            if (_ToPaintCache == null)
            {
                _ToPaintCache = CornerIndexes.Distinct(new Comparer()).ToList();
            }
            return _ToPaintCache;
        }
    }

    public static List<int> Indexes()
    {
        var request = new List<int>();
        for (var i = 0; i < CornerIndexes.Count; i++)
            request.Add(ToPaint.FindIndex(CornerIndexes[i].SequenceEqual));
        return request;
    }

    private static Sprite[] IndexedSprites(List<Sprite> sprites)
    {
        var request = new List<Sprite>();
        for (var i = 0; i < CornerIndexes.Count; i++)
            request.Add(sprites[ToPaint.FindIndex(CornerIndexes[i].SequenceEqual)]);
        return request.ToArray();
    }

    private static void PaintTile(Texture2D canvasTexture, Vector2Int start, int cornerSize, params Color[][] stamps)
    {
        for (var i = 0; i < stamps.Length; i++)
            canvasTexture.SetPixels(start.x + (i % 2 * cornerSize), start.y + (i / 2 * cornerSize), cornerSize, cornerSize, stamps[i]);
    }


    private static List<Color[]> GetCorners(Vector2Int start, int cornerSize, Texture2D textureReference)
    {
        var request = new List<Color[]>();

        for (var y = 0; y < 6; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                if (x < 2 && y < 2) continue;

                request.Add(
                    textureReference.GetPixels(
                        start.x + x * cornerSize,
                        start.y - (y + 1) * cornerSize,
                        cornerSize, cornerSize));
            }
        }
        return request;
    }

    private static Texture2D CreateTexture(int size)
    {
        var request = new Texture2D(size, size);
        var color = Color.clear;
        var colors = new Color[size * size];

        for (var i = 0; i < colors.Length; i++)
            colors[i] = color;

        request.SetPixels(colors);
        request.Apply();
        return request;
    }

    private static int swIndex(AutoTile.Direction value)
    {
        //needed checks -> sw | s | w
        var toTest = AutoTile.Direction.None;
        if ((value & AutoTile.Direction.SouthWest) != 0)
            toTest |= AutoTile.Direction.SouthWest;
        if ((value & AutoTile.Direction.South) != 0)
            toTest |= AutoTile.Direction.South;
        if ((value & AutoTile.Direction.West) != 0)
            toTest |= AutoTile.Direction.West;

        switch (toTest)
        {
            case AutoTile.Direction.SouthWest | AutoTile.Direction.South | AutoTile.Direction.West:
                return 10;
            case AutoTile.Direction.SouthWest | AutoTile.Direction.West:
            case AutoTile.Direction.West:
                return 18;
            case AutoTile.Direction.SouthWest | AutoTile.Direction.South:
            case AutoTile.Direction.South:
                return 8;
            case AutoTile.Direction.West | AutoTile.Direction.South:
                return 2;
        }

        return 16;
    }

    private static int seIndex(AutoTile.Direction value)
    {
        //needed checks -> se | s | e
        var toTest = AutoTile.Direction.None;
        if ((value & AutoTile.Direction.SouthEast) != 0)
            toTest |= AutoTile.Direction.SouthEast;
        if ((value & AutoTile.Direction.South) != 0)
            toTest |= AutoTile.Direction.South;
        if ((value & AutoTile.Direction.East) != 0)
            toTest |= AutoTile.Direction.East;

        switch (toTest)
        {
            case AutoTile.Direction.SouthEast | AutoTile.Direction.South | AutoTile.Direction.East:
                return 9;
            case AutoTile.Direction.SouthEast | AutoTile.Direction.East:
            case AutoTile.Direction.East:
                return 17;
            case AutoTile.Direction.SouthEast | AutoTile.Direction.South:
            case AutoTile.Direction.South:
                return 11;
            case AutoTile.Direction.East | AutoTile.Direction.South:
                return 3;
        }
        return 19;
    }

    private static int nwIndex(AutoTile.Direction value)
    {
        //needed checks -> nw | n | w
        var toTest = AutoTile.Direction.None;
        if ((value & AutoTile.Direction.NorthWest) != 0)
            toTest |= AutoTile.Direction.NorthWest;
        if ((value & AutoTile.Direction.North) != 0)
            toTest |= AutoTile.Direction.North;
        if ((value & AutoTile.Direction.West) != 0)
            toTest |= AutoTile.Direction.West;

        switch (toTest)
        {
            case AutoTile.Direction.NorthWest | AutoTile.Direction.North | AutoTile.Direction.West:
                return 14;
            case AutoTile.Direction.NorthWest | AutoTile.Direction.West:
            case AutoTile.Direction.West:
                return 6;
            case AutoTile.Direction.NorthWest | AutoTile.Direction.North:
            case AutoTile.Direction.North:
                return 12;
            case AutoTile.Direction.West | AutoTile.Direction.North:
                return 0;
        }
        return 4;
    }

    private static int neIndex(AutoTile.Direction value)
    {
        //needed checks -> ne | n | e
        var toTest = AutoTile.Direction.None;
        if ((value & AutoTile.Direction.NorthEast) != 0)
            toTest |= AutoTile.Direction.NorthEast;
        if ((value & AutoTile.Direction.North) != 0)
            toTest |= AutoTile.Direction.North;
        if ((value & AutoTile.Direction.East) != 0)
            toTest |= AutoTile.Direction.East;

        switch (toTest)
        {
            case AutoTile.Direction.NorthEast | AutoTile.Direction.North | AutoTile.Direction.East:
                return 13;
            case AutoTile.Direction.NorthEast | AutoTile.Direction.East:
            case AutoTile.Direction.East:
                return 5;
            case AutoTile.Direction.NorthEast | AutoTile.Direction.North:
            case AutoTile.Direction.North:
                return 15;
            case AutoTile.Direction.East | AutoTile.Direction.North:
                return 1;
        }
        return 7;
    }

    public class Comparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; ++i)
                if (!x[i].Equals(y[i]))
                    return false;

            return true;
        }

        public int GetHashCode(int[] obj)
        {
            return string.Join("", obj.Select(s => s.ToString()).ToArray()).GetHashCode();
        }
    }
}

public class AutoTileGeneratorWindow : EditorWindow
{
    [MenuItem("Window/AutoTile Generator")]
    private static void OpenAutoTileGeneratorWindow()
    {
        var window = GetWindow<AutoTileGeneratorWindow>();
        window.Show();
    }

    private Texture2D _OriginalTexture;
    private Vector2 TextureScrollView, PreviewScrollView;
    private Color LineColor = Color.red;
    private List<Texture2D> Previews = new List<Texture2D>();
    private string[] TileNames;
    private int TileSizeSelected;
    private bool nameFoldout;
    private string textureName;

    private void OnGUI()
    {
        if (_OriginalTexture)
        {
            var sizes = ValidSizes();
            bool validsize = sizes.Length > 0;
			if (TileSizeSelected < 0 || TileSizeSelected >= sizes.Length)
				TileSizeSelected = sizes.Length - 1;
            PreviewScrollView = EditorGUILayout.BeginScrollView(PreviewScrollView, GUILayout.Width(position.width), GUILayout.Height(position.height));
            TextureScrollView = EditorGUILayout.BeginScrollView(TextureScrollView, GUILayout.Height(150));
            GUILayout.Box(_OriginalTexture, GUIStyle.none);
            if (validsize)
                DrawGrid(LineColor, _OriginalTexture.width, _OriginalTexture.height, sizes[TileSizeSelected]); 
            EditorGUILayout.EndScrollView();
            if (validsize)
            {
                LineColor = EditorGUILayout.ColorField("Line color", LineColor);

                EditorGUILayout.LabelField("Tile Size in Pixels", new GUIStyle() { stretchWidth = true, alignment = TextAnchor.MiddleCenter });
                var selected = GUILayout.SelectionGrid(TileSizeSelected, ValidSizes().Select(s => s.ToString()).ToArray(), ValidSizes().Length, EditorStyles.toolbarButton);
                if (TileSizeSelected != selected)
                {
                    TileSizeSelected = selected;
                    RefreshTilePreviews();
                }

                var tileCount = new Vector2Int(_OriginalTexture.width / sizes[TileSizeSelected], _OriginalTexture.height / sizes[TileSizeSelected]);
                var tileSetCount = (tileCount.x / 2) * (tileCount.y / 3);

                EditorGUILayout.LabelField("Autotile Sets Found:" + tileSetCount, new GUIStyle() { stretchWidth = true, alignment = TextAnchor.MiddleRight });
				
                nameFoldout = EditorGUILayout.Foldout(nameFoldout, "Tile Names " + Previews.Count);
                if (nameFoldout)
                    for (var i = 0; i < Previews.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Box(Previews[i]);
                        TileNames[i] = EditorGUILayout.TextField("Tile Name:", TileNames[i]);
                        EditorGUILayout.EndHorizontal();
                    }

                if (GUILayout.Button("Create Auto Tiles"))
                {
                    var saveFolder = EditorUtility.SaveFolderPanel("Select Folder", "Assets", "");
                    if (saveFolder.Length > 0)
                    {
                        AutoTileTools.AutoTileGenerate(_OriginalTexture, sizes[TileSizeSelected], saveFolder, TileNames);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Error in texture dimensions. Not valid for auto tile generation.");
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.LabelField("First select a Texture that is in the VX Ace auto tile format (2 by 3 tiles per auto tile). " +
                "Once selected this window will update with the options to generate autotiles for that texture. " +
                "The image at the top will show a grid over it as a visual to aid in the tile size selection. If there is any issues in seeing the grid lines " +
                "you can change the line color for a better view. Select the correct finished tile size in pixels (this is used for pixels per unity unit)." +
                "Click the refresh button to update the tile naming list. By default the tiles will be named by the texture and their sub index, " +
                "feel free to change the names as you see fit. When ready select the generate button at the bottom, the folder to save them in and a new texture" +
                "will be created as well as all the autotile assest for use with the tile palette and tile map.");
        }
    }

    private void OnSelectionChange()
    {
        var selection = Selection.activeObject as Texture2D;
        if (selection && selection != _OriginalTexture)
        {
            _OriginalTexture = selection;
            var path = AssetDatabase.GetAssetPath(_OriginalTexture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!ValidSizes().Any(s => s == Mathf.FloorToInt(importer.spritePixelsPerUnit)))
                TileSizeSelected = ValidSizes().Length - 1;
            if (importer && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(path);
            }
            RefreshTilePreviews();
            Repaint();
        }
    }

    private void RefreshTilePreviews()
    {
		Previews.Clear();

		if(TileSizeSelected >= ValidSizes().Length)
			TileSizeSelected = ValidSizes().Length - 1;

		if (TileSizeSelected < 0 || TileSizeSelected >= ValidSizes().Length)
			return;
        var size = ValidSizes()[TileSizeSelected];
        var tileNames = new List<string>();

        var tileCount = new Vector2Int(_OriginalTexture.width / size, _OriginalTexture.height / size);
        
        for(var y = 0; y < tileCount.y; y++)
        {
            for(var x = 0; x < tileCount.x; x++)
            {
                if(x % 2 == 0 && y % 3 == 2)
                {
                    var texture = new Texture2D(size, size);
                    
                    tileNames.Add(_OriginalTexture.name + "_" + Previews.Count.ToString("00"));

                    var colors = _OriginalTexture.GetPixels(x * size, y * size, size, size);
                    texture.SetPixels(colors);
                    texture.Apply();
                    Previews.Add(texture);
                }
            }
        }
        TileNames = tileNames.ToArray();
    }

    private int[] ValidSizes()
    {
        var request = new List<int>();
        var loop = _OriginalTexture.width > _OriginalTexture.height ? _OriginalTexture.height / 3 : _OriginalTexture.width / 2;
        for(var i = 2; i <= loop; i++)
        {
            if (_OriginalTexture.width % (i * 2) == 0 && _OriginalTexture.height % (i * 3) == 0)
                request.Add(i);
        }
        return request.ToArray();
    }

    private void DrawGrid(Color color, float width, float height, float cellsize)
    {
        Handles.BeginGUI();
        Handles.color = color;
        for (var i = 1; i < width / (cellsize * 2); i++)
            Handles.DrawLine(new Vector3(i * cellsize* 2, 0), new Vector3(i * cellsize * 2, height));
        for (var i = 1; i < height / (cellsize * 3); i++)
            Handles.DrawLine(new Vector3(0, i * cellsize * 3), new Vector3(width, i * cellsize * 3));
        Handles.EndGUI();
    }
}