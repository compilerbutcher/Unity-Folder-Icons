using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace UnityEditorTools.FolderIcons
{

    internal static class TextureFunctions
    {
        internal static TextureData CreateTextureData(Color color, Texture2D emptyFolderTexture, Texture2D folderTexture, Texture2D customTexture)
        {
            TextureData newTextureData = new();

            newTextureData.color = color;
            newTextureData.emptyFolderTexture = emptyFolderTexture;
            newTextureData.folderTexture = folderTexture;
            newTextureData.customTexture = customTexture;

            return newTextureData;
        }

        internal static void CreateTexture(Texture2D importTexture, string assetFullPath)
        {
            byte[] bytes = importTexture.EncodeToPNG();

            File.WriteAllBytes(assetFullPath, bytes);
            AssetDatabase.Refresh();

        }
        internal static void ImportTexture(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            importer.textureType = TextureImporterType.GUI;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
        internal static void CreateTexture2DWithColor(ref Texture2D texture, Color color, int width, int height)
        {
            Texture2D newTexture2D = new(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newTexture2D.SetPixel(x, y, color);
                }
            }
            newTexture2D.Apply();
            texture = newTexture2D;
        }
        internal static void Base64ToTexture2D(string base64String, string path)
        {
            if (base64String.Length == 0) return;

            byte[] textureArray = Convert.FromBase64String(base64String);
            File.WriteAllBytes(path, textureArray);
            AssetDatabase.Refresh();


            //Texture2D newTexture2D = new Texture2D(1, 1);
            //ImageConversion.LoadImage(newTexture2D, textureArray);

            //return newTexture2D;
        }
        internal static void CreateDefaultFolderWithColor(Color currentColor, ref Texture2D emptyFolderTexture, ref Texture2D defaultFolderTexture)
        {

            emptyFolderTexture = new Texture2D(DynamicConstants.emptyDefaultFolderIcon.width, DynamicConstants.emptyDefaultFolderIcon.height);
            defaultFolderTexture = new Texture2D(DynamicConstants.defaultFolderIcon.width, DynamicConstants.defaultFolderIcon.height);
            for (int x = 0; x < DynamicConstants.defaultFolderIcon.width; x++)
            {
                for (int y = 0; y < DynamicConstants.defaultFolderIcon.height; y++)
                {
                    // Set empty folder 
                    Color emptyOldColor = DynamicConstants.emptyDefaultFolderIcon.GetPixel(x, y);
                    Color emptyNewCol = currentColor;

                    emptyNewCol.a = emptyOldColor.a;
                    emptyFolderTexture.SetPixel(x, y, emptyNewCol);



                    // Set default folder
                    Color defaultOldColor = DynamicConstants.defaultFolderIcon.GetPixel(x, y);
                    Color defaultNewCol = currentColor;

                    defaultNewCol.a = defaultOldColor.a;
                    defaultFolderTexture.SetPixel(x, y, defaultNewCol);

                }
            }

            emptyFolderTexture.Apply();
            defaultFolderTexture.Apply();
        }
    }

    internal static class PopupWindowContentFunctions
    {
        // Create and load a newly created colorful default folder texture and create TextureData as well
        internal static void CreateAndLoadDefaultFolderWithColor(string selectedAssetGUID, string emptyAssetPath, string emptyFullPath, string folderAssetPath, string folderFullPath)
        {
            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            TextureFunctions.CreateTexture(IconManager.projectCurrentEmptyFolderTexture, emptyFullPath);
            TextureFunctions.ImportTexture(emptyAssetPath);

            TextureFunctions.CreateTexture(IconManager.projectCurrentFolderTexture, folderFullPath);
            TextureFunctions.ImportTexture(folderAssetPath);

        }
        // Create default folder with custom color and delete existing ones if it exists (Do not use this in any update function)
        internal static void HandleCreateAndDeleteFoldersOnClose(string selectedAssetGUID, Dictionary<string, TextureData> tempDict)
        {
            string tempCache = Application.temporaryCachePath + "/ColorfulFolderIcons";
            if (!File.Exists(tempCache))
                Directory.CreateDirectory(tempCache);

            string emptyFolderAssetsPath = $"Assets/Empty{IconManager.persistentData.colorFolderNumber}.png";
            string emptyFolderAssetsFullPath = $"{Application.dataPath}\\Empty{IconManager.persistentData.colorFolderNumber}.png";
            string emptyFolderAssetsFullPathMeta = $"{Application.dataPath}\\Empty{IconManager.persistentData.colorFolderNumber}.png.meta";


            string folderAssetsPath = $"Assets/{IconManager.persistentData.colorFolderNumber}.png";
            string folderAssetsFullPath = $"{Application.dataPath}\\{IconManager.persistentData.colorFolderNumber}.png";
            string folderAssetsFullPathMeta = $"{Application.dataPath}\\{IconManager.persistentData.colorFolderNumber}.png.Meta";




            string emptyFolderPackagePath = $"{DynamicConstants.folderStoragePath}/Empty{IconManager.persistentData.colorFolderNumber}.png";
            string emptyFolderFullPackagePath = $"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}/Empty{IconManager.persistentData.colorFolderNumber}.png";
            string emptyFolderFullPackagePathMeta = $"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}/Empty{IconManager.persistentData.colorFolderNumber}.png.meta";


            string folderAssetPath = $"{DynamicConstants.folderStoragePath}{IconManager.persistentData.colorFolderNumber}.png";
            string folderFullPackagePath = $"{Path.GetFullPath(DynamicConstants.folderStoragePath)}/{IconManager.persistentData.colorFolderNumber}.png";
            string folderFullPackagePathMeta = $"{Path.GetFullPath(DynamicConstants.folderStoragePath)}/{IconManager.persistentData.colorFolderNumber}.png.meta";

            // Creating folder texture color with selected color
            if (!tempDict.ContainsKey(selectedAssetGUID))
            {
                if (!File.Exists(tempCache + $"/Empty{selectedAssetGUID}.png") &&
                    !File.Exists(tempCache + $"/Empty{selectedAssetGUID}.png.meta") &&
                    !File.Exists(tempCache + $"/{selectedAssetGUID}.png") &&
                    !File.Exists(tempCache + $"/{selectedAssetGUID}.png.meta"))
                {

                    CreateAndLoadDefaultFolderWithColor(selectedAssetGUID, emptyFolderAssetsPath, emptyFolderAssetsFullPath, folderAssetsPath, folderAssetsFullPath);
                    UtilityFunctions.CreateAndSaveDataToDict(selectedAssetGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentColor, IconManager.projectCurrentEmptyFolderTexture, IconManager.projectCurrentFolderTexture, null);

                    FileUtil.MoveFileOrDirectory(emptyFolderAssetsPath, tempCache + $"/Empty{selectedAssetGUID}.png");
                    FileUtil.MoveFileOrDirectory(emptyFolderAssetsPath + ".meta", tempCache + $"/Empty{selectedAssetGUID}.png.meta");

                    FileUtil.MoveFileOrDirectory(folderAssetsPath, tempCache + $"/{selectedAssetGUID}.png");
                    FileUtil.MoveFileOrDirectory(folderAssetsPath + ".meta", tempCache + $"/{selectedAssetGUID}.png.meta");


                    AssetDatabase.Refresh();
                }
                else
                {
                    UtilityFunctions.CreateAndSaveDataToDict(selectedAssetGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentColor, IconManager.projectCurrentEmptyFolderTexture, IconManager.projectCurrentFolderTexture, null);
                }
                IconManager.persistentData.colorFolderNumber++;

            }
            // Deleting and changing folder texture color with selected color
            else
            {

                if (!File.Exists(tempCache + $"/Empty{selectedAssetGUID}.png") &&
                    !File.Exists(tempCache + $"/Empty{selectedAssetGUID}.png.meta") &&
                    !File.Exists(tempCache + $"/{selectedAssetGUID}.png") &&
                    !File.Exists(tempCache + $"/{selectedAssetGUID}.png.meta"))
                {
                    CreateAndLoadDefaultFolderWithColor(selectedAssetGUID, emptyFolderAssetsPath, emptyFolderAssetsFullPath, folderAssetsPath, folderAssetsFullPath);
                    UtilityFunctions.CreateAndSaveDataToDict(selectedAssetGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentColor, IconManager.projectCurrentEmptyFolderTexture, IconManager.projectCurrentFolderTexture, null);

                    FileUtil.MoveFileOrDirectory(emptyFolderAssetsPath, tempCache + $"/Empty{selectedAssetGUID}.png");
                    FileUtil.MoveFileOrDirectory(emptyFolderAssetsPath + ".meta", tempCache + $"/Empty{selectedAssetGUID}.png.meta");

                    FileUtil.MoveFileOrDirectory(folderAssetsPath, tempCache + $"/{selectedAssetGUID}.png");
                    FileUtil.MoveFileOrDirectory(folderAssetsPath + ".meta", tempCache + $"/{selectedAssetGUID}.png.meta");


                    AssetDatabase.Refresh();
                }
                else
                {
                    UtilityFunctions.CreateAndSaveDataToDict(selectedAssetGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentColor, IconManager.projectCurrentEmptyFolderTexture, IconManager.projectCurrentFolderTexture, null);
                }
                IconManager.persistentData.colorFolderNumber++;



            }
        }

        internal static void HandleCreateCustomTexture(string selectedAssetGUID, Dictionary<string, TextureData> dict, Texture2D customTextureToSave)
        {
            if (dict.ContainsKey(selectedAssetGUID))
            {
                if (IconManager.tempFolderIconDict[selectedAssetGUID].emptyFolderTexture != null && IconManager.tempFolderIconDict[selectedAssetGUID].folderTexture != null)
                {
                    string emptyFolderName = dict[selectedAssetGUID].emptyFolderTexture.name;
                    string folderName = dict[selectedAssetGUID].folderTexture.name;

                    if (File.Exists($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{emptyFolderName}.png")
                          && File.Exists($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{emptyFolderName}.png.meta")
                          && File.Exists($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{folderName}.png")
                          && File.Exists($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{folderName}.png.meta"))
                    {
                        File.Delete($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{emptyFolderName}.png");
                        File.Delete($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{emptyFolderName}.png.meta");


                        File.Delete($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{folderName}.png");
                        File.Delete($"{DynamicConstants.absolutePackagePath}{Constants.iconsFolderName}{Constants.colorFolderStorageName}\\{folderName}.png.meta");
                    }
                }
            }

            UtilityFunctions.CreateAndSaveDataToDict(selectedAssetGUID, dict, Color.clear, null, null, customTextureToSave);

            AssetDatabase.Refresh();
        }


    }

    internal static class UtilityFunctions
    {
        // Handle if is a folder filled or not
        internal static bool IsFolderFilled(string folderPath)
        {
            if (Directory.Exists(folderPath))
                return Directory.GetFileSystemEntries(folderPath).Length > 0;
            else
                return false;
        }
        // Update folder empty dictionary according to parent folder
        internal static void UpdateFolderEmptyDict(string currentPath, ref Dictionary<string, bool> dict)
        {
            string parentDir = Path.GetDirectoryName(currentPath).Replace("\\", "/");

            string assetFullPath = Path.GetFullPath(parentDir);
            if (!dict.ContainsKey(parentDir))
            {
                dict.Add(parentDir, IsFolderFilled(assetFullPath));
            }
            else
            {
                dict[parentDir] = IsFolderFilled(assetFullPath);
            }
        }
        // Check all folders and assign folder empty dictionary all folders state
        internal static void CheckAllFoldersCurrentEmptiness(ref Dictionary<string, bool> folderEmptyDict)
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            //string[] allAssets = AssetDatabase.FindAssets("t:DefaultAsset", new string[] {"Packages"});
            for (int assetPathIndex = 0; assetPathIndex < allAssets.Length; assetPathIndex++)
            {
                string assetPath = allAssets[assetPathIndex];

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    if (!folderEmptyDict.ContainsKey(assetPath))
                    {
                        folderEmptyDict.Add(assetPath, Directory.GetFileSystemEntries(Path.GetFullPath(assetPath)).Length > 0);
                    }
                    else
                    {
                        folderEmptyDict[assetPath] = Directory.GetFileSystemEntries(Path.GetFullPath(assetPath)).Length > 0;
                    }
                }

            }
        }
        // Create and save TextureData to main texture dictionary
        internal static void CreateAndSaveDataToDict(string selectedAssetGUID, Dictionary<string, TextureData> dictionary, Color color, Texture2D emptyFolderTexture, Texture2D folderTexture, Texture2D customTexture)
        {
            // If there is no texture create and add one
            if (!dictionary.ContainsKey(selectedAssetGUID))
            {
                TextureData textureData;

                textureData = TextureFunctions.CreateTextureData(color, emptyFolderTexture, folderTexture, customTexture);
                dictionary.Add(selectedAssetGUID, textureData);

                EditorApplication.projectWindowItemOnGUI += DrawFolders;
                EditorApplication.RepaintProjectWindow();
            }
            // If there is already a texture change that texture
            else
            {
                TextureData textureData;

                textureData = TextureFunctions.CreateTextureData(color, emptyFolderTexture, folderTexture, customTexture);
                dictionary[selectedAssetGUID] = textureData;

                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += DrawFolders;
                EditorApplication.RepaintProjectWindow();
            }
        }
        
        // Draw one item in the project window
        internal static void DrawTextures(string guid, Rect rect, Texture2D texture2d)
        {
            bool treeView = rect.width > rect.height;
            bool sideView = rect.x != 14;

            // For vertical folder view
            if (treeView)
            {
                rect.width = rect.height = 16f;

                // Small offset
                if (!sideView) rect.x += 3f;
            }
            else rect.height -= 14f;

            if (texture2d == null) return;


            EditorGUI.DrawRect(rect, DynamicConstants.projectBackgroundColor);
            GUI.DrawTexture(rect, texture2d, ScaleMode.ScaleAndCrop);
        }


        //internal static void DrawFolderColor(string guid, Rect selectionRect)
        //{
        //    if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID))) return;

        //    string folderPath = AssetDatabase.GUIDToAssetPath(guid);

        //    if (AssetDatabase.IsValidFolder(folderPath))
        //    {
        //        IconManager.folderEmptyDict.TryGetValue(folderPath, out bool outputBool);
        //        if (outputBool)
        //            DrawTextures(guid, selectionRect, IconManager.projectCurrentFolderTexture);
        //        else
        //            DrawTextures(guid, selectionRect, IconManager.projectCurrentEmptyFolderTexture);
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}



        // Main function for drawing project window items



        internal static void DrawFolders(string guid, Rect selectionRect)
        {
            if (IconManager.tempFolderIconDict.Count == 0) return; // If there is not a custom icon return
            if (guid == "00000000000000000000000000000000") return; // Ignore main assets folder
            if (guid == "00000000000000001000000000000000") return; // Ignore main packages folder
            
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);



            //if (folderPath.StartsWith("Package")) return;
            if (!AssetDatabase.IsValidFolder(folderPath)) return;


            // Draw default and empty folders
            if (!IconManager.tempFolderIconDict.ContainsKey(guid))
            {

                IconManager.folderEmptyDict.TryGetValue(folderPath, out bool outputBool);
                if (outputBool)
                    DrawTextures(guid, selectionRect, DynamicConstants.defaultFolderIcon);
                else
                    DrawTextures(guid, selectionRect, DynamicConstants.emptyDefaultFolderIcon);
            }

            else
            {
                foreach (KeyValuePair<string, TextureData> i in IconManager.tempFolderIconDict)
                {
                    if (i.Key != guid) continue;

                    TextureData textureData = IconManager.tempFolderIconDict[guid];

                    if (textureData.folderTexture != null)
                    {
                        IconManager.folderEmptyDict.TryGetValue(folderPath, out bool outputBool);
                        if (outputBool)
                        {
                            DrawTextures(guid, selectionRect, textureData.folderTexture);
                        }
                        else
                        {
                            DrawTextures(guid, selectionRect, textureData.emptyFolderTexture);
                        }
                    }
                    else if (textureData.customTexture != null)
                    {
                        DrawTextures(guid, selectionRect, textureData.customTexture);
                    }
                }
            }


        }
    }



    internal static class JsonHelper
    {
        internal static void SaveJson<T>(string savePath, T item)
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(item));
        }
        internal static void SaveJson<T>(string savePath, T[] items)
        {
            JsonArray<T> itemData = new JsonArray<T>();
            itemData.objectArray = items;

            File.WriteAllText(savePath, JsonUtility.ToJson(itemData));
        }
        internal static void SaveJson<T>(string savePath, List<T> items)
        {
            JsonList<T> itemData = new JsonList<T>();
            itemData.objectList = items;

            File.WriteAllText(savePath, JsonUtility.ToJson(itemData));
        }


        internal static void LoadJson<T>(string loadPath, out T data)
        {
            data = JsonUtility.FromJson<T>(File.ReadAllText(loadPath));
        }
        internal static void LoadJson<T>(string loadPath, out T[] outArray)
        {
            outArray = JsonUtility.FromJson<JsonArray<T>>(File.ReadAllText(loadPath)).objectArray;
        }
        internal static void LoadJson<T>(string loadPath, out List<T> outList)
        {
            outList = JsonUtility.FromJson<JsonList<T>>(File.ReadAllText(loadPath)).objectList;
        }



        internal static void FromJsonOverwrite<T>(T json, T objectToOverWrite)
        {
            string jsonString = JsonUtility.ToJson(json);
            JsonUtility.FromJsonOverwrite(jsonString, objectToOverWrite);
        }

        [Serializable]
        private struct JsonArray<T>
        {
            [SerializeField] internal T[] objectArray;
        }
        [Serializable]
        private struct JsonList<T>
        {
            [SerializeField] internal List<T> objectList;
        }
    }


}
