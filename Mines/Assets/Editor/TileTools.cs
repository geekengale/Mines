using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class TileTools
    {
        private static int[][] toPaint = {
            new int[4]{16, 19, 4, 7},
            new int[4]{18, 19, 6, 7},
            new int[4]{8, 11, 4, 7},
            new int[4]{2, 11, 6, 7},
            new int[4]{10, 11, 6, 7},
            new int[4]{16, 19, 12, 15},
            new int[4]{18, 19, 0, 15},
            new int[4]{18, 19, 14, 15},
            new int[4]{8, 11, 12, 15},
            new int[4]{2, 11, 0, 15},
            new int[4]{10, 11, 0, 15},
            new int[4]{2, 11, 14, 15},
            new int[4]{10, 11, 14, 15},
            new int[4]{16, 17, 4, 5},
            new int[4]{18, 17, 6, 5},
            new int[4]{8, 3, 4, 5},
            new int[4]{2, 3, 6, 5},
            new int[4]{10, 3, 6, 5},
            new int[4]{16, 17, 12, 1},
            new int[4]{18, 17, 0, 1},
            new int[4]{18, 17, 14, 1},
            new int[4]{8, 3, 12, 1},
            new int[4]{2, 3, 0, 1},
            new int[4]{10, 3, 0, 1},
            new int[4]{2, 3, 14, 1},
            new int[4]{10, 3, 14, 1},
            new int[4]{8, 9, 4, 5},
            new int[4]{2, 9, 6, 5},
            new int[4]{10, 9, 6, 5},
            new int[4]{8, 9, 12, 1},
            new int[4]{2, 9, 0, 1},
            new int[4]{10, 9, 0, 1},
            new int[4]{2, 9, 14, 1},
            new int[4]{10, 9, 14, 1},
            new int[4]{16, 17, 12, 13},
            new int[4]{18, 17, 0, 13},
            new int[4]{18, 17, 14, 13},
            new int[4]{8, 3, 12, 13},
            new int[4]{2, 3, 0, 13},
            new int[4]{10, 3, 0, 13},
            new int[4]{2, 3, 14, 13},
            new int[4]{10, 3, 14, 13},
            new int[4]{8, 9, 12, 13},
            new int[4]{2, 9, 0, 13},
            new int[4]{10, 9, 0, 13},
            new int[4]{2, 9, 14, 13},
            new int[4]{10, 9, 14, 13}
        };
        private static int[] spriteIndex = new int[]
        {
            0,0,1,1,0,0,1,1,2,2,3,4,2,2,3,4,5,5,6,6,5,5,7,7,8,8,9,10,8,8,11,12,0,0,1,1,0,0,1,1,2,2,3,4,2,2,3,4,5,
            5,6,6,5,5,7,7,8,8,9,10,8,8,11,12,13,13,14,14,13,13,14,14,15,15,16,17,15,15,16,17,18,18,19,19,18,18,20,
            20,21,21,22,23,21,21,24,25,13,13,14,14,13,13,14,14,26,26,27,28,26,26,27,28,18,18,19,19,18,18,20,20,29,
            29,30,31,29,29,32,33,0,0,1,1,0,0,1,1,2,2,3,4,2,2,3,4,5,5,6,6,5,5,7,7,8,8,9,10,8,8,11,12,0,0,1,1,0,0,1,
            1,2,2,3,4,2,2,3,4,5,5,6,6,5,5,7,7,8,8,9,10,8,8,11,12,13,13,14,14,13,13,14,14,15,15,16,17,15,15,16,17,
            34,34,35,35,34,34,36,36,37,37,38,39,37,37,40,41,13,13,14,14,13,13,14,14,26,26,27,28,26,26,27,28,34,34,
            35,35,34,34,36,36,42,42,43,44,42,42,45,46
        };

        public static Texture2D CreateAutoTileTexture(Texture2D original, Vector2Int tileSize, params Vector2Int[] setLocations)
        {
            #region error handeling
            if (!original)
            {
                Debug.Log("Original texture not found.");
                return null;
            }

            if (setLocations == null || setLocations.Length < 1)
            {
                Debug.Log("no set locations to create Texture. Must have at least one set location.");
                return null;
            }

            if (setLocations.Any(l => l.x < 0 || l.y < 0 || l.x > original.width || l.y > original.height))
            {
                Debug.Log("Invalid set location detected. Locations must be inside the bounds of the texture.");
                Debug.Log(tileSize);
                Debug.Log(original.width + ", " + original.height);
                foreach (var loc in setLocations)
                    Debug.Log(loc);
                return null;
            }


            if (tileSize == null || setLocations.Any(l => l.x + tileSize.x * 2 > original.width || l.y + tileSize.y * 3 > original.height))
            {
                Debug.Log("All tile sets must remain in bounds of the original texture, the tileSize and/or (a) set Location is incorrect.");
                return null;
            }
            #endregion

            var quarterSize = new Vector2Int(tileSize.x / 2, tileSize.y / 2);
            var stamps = new List<List<Color[]>>();
            foreach (var location in setLocations)
                stamps.Add(GetsStampsFrom(original, quarterSize, location));

            //padding for boarder.
            tileSize.x += 2; tileSize.y += 2;

            var SqRt = Mathf.Sqrt(setLocations.Length * toPaint.Length);
            var endCountSqrt = (SqRt % 1 != 0 ? (int)SqRt + 1 : (int)SqRt);
            var finishedSize = tileSize * endCountSqrt;

            var request = CreateBlankTexture(finishedSize);
            quarterSize.x += 1;
            quarterSize.y += 1;
            var i = 0;
            foreach (var palet in stamps)
            {
                foreach(var brush in toPaint)
                {
                    var x = i % endCountSqrt * tileSize.x;
                    var y = i / endCountSqrt * tileSize.y;
                    request.SetPixels(x, y, quarterSize.x, quarterSize.y, palet[brush[0]]);
                    request.SetPixels(x + quarterSize.x, y, quarterSize.x, quarterSize.y, palet[brush[1]]);
                    request.SetPixels(x, y + quarterSize.y, quarterSize.x, quarterSize.y, palet[brush[2]]);
                    request.SetPixels(x + quarterSize.x, y + quarterSize.y, quarterSize.x, quarterSize.y, palet[brush[3]]);
                    i++;
                }
            }

            request.Apply();
            return request;
        }

        public static void SaveTexture(Texture2D texture, string path)
        {
            var pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public static void GenerateTileSprites(string path, Vector2Int tileSize, int tileCount, Vector2Int textureSize)
        {
            var importer = prepImporter(path, Mathf.Min(textureSize.x, textureSize.y));
            List<SpriteMetaData> spriteData = new List<SpriteMetaData>();

            var h = textureSize.x / tileSize.x;

            for (var i = 0; i < tileCount; i++)
            {
                spriteData.Add(new SpriteMetaData()
                {
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = 9,
                    name = (i / tileCount).ToString("00") + (i % tileCount).ToString("00"),
                    rect = new Rect(i % h * tileSize.x + 1, i / h * tileSize.y + 1, tileSize.x - 2, tileSize.y - 2),
                });
            }

            importer.spritesheet = spriteData.ToArray();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void CreateAutoTileAsset(string path, params string[] tilenames)
        {
            var alltilesprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToList();

            for (var i = 0; i < tilenames.Length; i++)
            {
                var spriteList = alltilesprites.Skip(i * toPaint.Length).Take(toPaint.Length).ToList();
                var autotile = new AutoTile()
                {
                    name = (tilenames[i]),
                    sprites = spriteIndex.Select(j => spriteList[j]).ToArray()
                };

                AssetDatabase.CreateAsset(autotile, "assets\\" + tilenames[i] + ".asset");
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static TextureImporter prepImporter(string path, int unityUnit)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = unityUnit;
            return importer;
        }

        private static List<Color[]> GetsStampsFrom(Texture2D original, Vector2Int stampSize, Vector2Int location)
        {
            var request = new List<Color[]>();

            for (var y = 0; y < 6; y++)
            {
                for(var x = 0; x < 4; x++)
                {
                    if (x < 2 && y < 2) continue;
                    
                    request.Add(CreateStamps(original, location + new Vector2Int(stampSize.x * x, stampSize.y * 6 - stampSize.y * (y + 1) ), stampSize, ApplyBorder.GetBorderFor(x, y)));
                }
            }

            return request;
        }

        private static Color[] CreateStamps(Texture2D original, Vector2Int start, Vector2Int size, ApplyBorder applyBorder)
        {
            var request = original.GetPixels(start.x, start.y, size.x, size.y);

            var borderTexture = CreateBlankTexture(new Vector2Int(size.x + 1, size.y + 1));
            borderTexture.SetPixels(applyBorder.Left == ApplyBorder.Style.None ? 0 : 1, applyBorder.Bottom == ApplyBorder.Style.None ? 0 : 1, size.x, size.y, request);

            if (applyBorder.Left == ApplyBorder.Style.Clone)
            {
                var clone = original.GetPixels(start.x, start.y, 1, size.y);
                borderTexture.SetPixels(0, applyBorder.Bottom == ApplyBorder.Style.None ? 0 : 1, 1, size.y, clone);
            }
            if (applyBorder.Right == ApplyBorder.Style.Clone)
            {
                var clone = original.GetPixels(start.x + size.x - 1, start.y, 1, size.y);
                borderTexture.SetPixels(size.x, applyBorder.Bottom == ApplyBorder.Style.None ? 0 : 1, 1, size.y, clone);
            }
            if (applyBorder.Bottom == ApplyBorder.Style.Clone)
            {
                var clone = original.GetPixels(start.x, start.y, size.x, 1);
                borderTexture.SetPixels(applyBorder.Left == ApplyBorder.Style.None ? 0 : 1, 0, size.x, 1, clone);
            }
            if (applyBorder.Top == ApplyBorder.Style.Clone)
            {
                var clone = original.GetPixels(start.x, start.y + size.y - 1, size.x, 1);
                borderTexture.SetPixels(applyBorder.Left == ApplyBorder.Style.None ? 0 : 1, size.y, size.x, 1, clone);
            }

            borderTexture.Apply();
            request = borderTexture.GetPixels();
            GameObject.DestroyImmediate(borderTexture);
            return request;
        }

        private static Texture2D CreateBlankTexture(Vector2Int size)
        {
            var request = new Texture2D(size.x, size.y);
            var color = Color.clear;
            var colors = new Color[size.x * size.y];

            for (var i = 0; i < colors.Length; i++)
                colors[i] = color;

            request.SetPixels(colors);
            request.Apply();
            return request;
        }

        private struct ApplyBorder
        {
            public enum Style
            {
                None,
                Transparent,
                Clone
            }

            public Style Left;
            public Style Right;
            public Style Top;
            public Style Bottom;

            public static ApplyBorder GetBorderFor(int x, int y)
            {
                var request = new ApplyBorder();

                switch (x)
                {
                    case 0:
                        request.Left = ApplyBorder.Style.Transparent;
                        break;
                    case 1:
                        request.Right = ApplyBorder.Style.Clone;
                        break;
                    case 2:
                        request.Left = ApplyBorder.Style.Clone;
                        break;
                    case 3:
                        if (y < 2)
                            request.Right = ApplyBorder.Style.Clone;
                        else
                            request.Right = ApplyBorder.Style.Transparent;
                        break;
                }

                switch (y)
                {
                    case 0:
                    case 4:
                        request.Top = ApplyBorder.Style.Clone;
                        break;
                    case 1:
                        request.Bottom = ApplyBorder.Style.Clone;
                        break;
                    case 2:
                        request.Top = ApplyBorder.Style.Transparent;
                        break;
                    case 3:
                        request.Bottom = ApplyBorder.Style.Clone;
                        break;
                    case 5:
                        request.Bottom = ApplyBorder.Style.Transparent;
                        break;
                }

                return request;
            }
        }
    }
}
