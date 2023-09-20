using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;


namespace UnityEditorTools.FolderIcons
{
    internal static class TextureFunctions
    {
        // Create TextureData and return it
        internal static TextureData CreateTextureData(Color color, Texture2D emptyFolderTexture, Texture2D folderTexture, Texture2D customTexture)
        {
            TextureData newTextureData = new();

            newTextureData.color = color;
            newTextureData.emptyFolderTexture = emptyFolderTexture;
            newTextureData.folderTexture = folderTexture;
            newTextureData.customTexture = customTexture;

            return newTextureData;
        }
        // Write bytes of a texture
        internal static void CreateTexture(Texture2D importTexture, string assetFullPath)
        {
            byte[] bytes = importTexture.EncodeToPNG();

            File.WriteAllBytes(assetFullPath, bytes);
            AssetDatabase.Refresh();
        }
        // Import texture as Editor GUI, readable and Uncompressed, we need these because when converting texture to base64 these are necessary
        internal static void ImportTexture(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            importer.textureType = TextureImporterType.GUI;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
        // Create a texture with specified color
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
        // Convert base64 info to texture2D
        internal static void Base64ToTexture2D(string base64String, string path)
        {
            if (base64String.Length == 0) return;

            byte[] textureArray = Convert.FromBase64String(base64String);
            File.WriteAllBytes(path, textureArray);
            AssetDatabase.Refresh();

        }
        // Create both empty and filled unity's default texture. We need both because a folder can be empty or not
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
        internal static void CreateAndLoadDefaultFolderWithColor(string emptyAssetPath, string emptyFullPath, string folderAssetPath, string folderFullPath)
        {
            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            TextureFunctions.CreateTexture(IconManager.projectCurrentEmptyFolderTexture, emptyFullPath);
            TextureFunctions.ImportTexture(emptyAssetPath);

            TextureFunctions.CreateTexture(IconManager.projectCurrentFolderTexture, folderFullPath);
            TextureFunctions.ImportTexture(folderAssetPath);

        }
        // Create color folder texture, its data then assign it to relevant places. update folder empty dictionary
        private static void HandleCreatingColorFolderTexture(string selectedAssetGUID, ref Texture2D emptyTexture, ref Texture2D texture)
        {
            UtilityFunctions.CheckAndCreateFolderStorage();


            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            byte[] emptyFolderTextureBytes = emptyTexture.EncodeToPNG();
            byte[] folderTextureBytes = texture.EncodeToPNG();


            string emptyTexturePath = $"{DynamicConstants.emptyIconFolderPath}/{selectedAssetGUID}.png";
            string texturePath = $"{DynamicConstants.iconFolderPath}/{selectedAssetGUID}.png";


            File.WriteAllBytes(Path.GetFullPath(emptyTexturePath), emptyFolderTextureBytes);
            File.WriteAllBytes(Path.GetFullPath(texturePath), folderTextureBytes);

            AssetDatabase.Refresh();

            TextureFunctions.ImportTexture(emptyTexturePath);
            TextureFunctions.ImportTexture(texturePath);

            Texture2D loadedEmptyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyTexturePath);
            Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            GUIDTextureData guidTextureData = new GUIDTextureData();
            guidTextureData.guid = selectedAssetGUID;
            guidTextureData.textureData = TextureFunctions.CreateTextureData(IconManager.projectCurrentColor, loadedEmptyTexture, loadedTexture, null);

            if (IconManager.persistentData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                int index = IconManager.persistentData.guidTextureList.FindIndex(x => x.guid == selectedAssetGUID);
                IconManager.persistentData.guidTextureList[index] = guidTextureData;
            }
            else
            {
                IconManager.persistentData.guidTextureList.Add(guidTextureData);
            }

            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);


            UtilityFunctions.UpdateFolderEmptyDict($"{DynamicConstants.emptyIconFolderPath}\\{selectedAssetGUID}.png",
                ref IconManager.folderEmptyDict);
            UtilityFunctions.UpdateFolderEmptyDict($"{DynamicConstants.iconFolderPath}\\{selectedAssetGUID}.png",
                ref IconManager.folderEmptyDict);

        }
        // Create default folder with custom color and delete existing ones if it exists (Do not use this in any update function)
        internal static void HandleColorFoldersTexture(string selectedAssetGUID)
        {
            IconManager.projectCurrentCustomTexture = null;

            // Creating folder texture color with selected color
            if (!IconManager.persistentData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                HandleCreatingColorFolderTexture(selectedAssetGUID, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            }
            // Deleting and changing folder texture color with selected color
            else
            {
                if (!File.Exists($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png") &&
                    !File.Exists($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png"))
                {
                    HandleCreatingColorFolderTexture(selectedAssetGUID, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
                }
                else
                {
                    if (File.Exists($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png") &&
                        File.Exists($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png"))
                    {
                        File.Delete($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png");
                        File.Delete($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png");
                        File.Delete($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png.meta");
                        File.Delete($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png.meta");

                        HandleCreatingColorFolderTexture(selectedAssetGUID, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
                    }
                
                }
            }
        }
        // Create custom texture data and assign it to relevant places
        internal static void HandleCustomTexture(string selectedAssetGUID)
        {
            UtilityFunctions.CheckAndCreateFolderStorage();


            IconManager.projectCurrentEmptyFolderTexture = null;
            IconManager.projectCurrentFolderTexture = null;

            if (!IconManager.persistentData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                GUIDTextureData guidTextureData = new GUIDTextureData();
                guidTextureData.guid = selectedAssetGUID;
                guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, IconManager.projectCurrentCustomTexture);

                IconManager.persistentData.guidTextureList.Add(guidTextureData);
                if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
            }
            else if (IconManager.persistentData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                if (File.Exists($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png") &&
                    File.Exists($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png"))
                {
                    File.Delete($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png");
                    File.Delete($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png");
                    File.Delete($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{selectedAssetGUID}.png.meta");
                    File.Delete($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{selectedAssetGUID}.png.meta");
                    AssetDatabase.Refresh();
                }

                GUIDTextureData guidTextureData = new GUIDTextureData();
                guidTextureData.guid = selectedAssetGUID;
                guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, IconManager.projectCurrentCustomTexture);

                int index = IconManager.persistentData.guidTextureList.FindIndex(x => x.guid == selectedAssetGUID);
                IconManager.persistentData.guidTextureList[index] = guidTextureData;
                if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
            }

        }
    }

    internal static class UtilityFunctions
    {
        // Check and create necessary folders and PersistentData if they are not exist
        internal static void CheckAndCreateFolderStorage()
        {
            // Project Installed Git
            if (IconManager.isProjectInstalledExternally)
            {
                if (!AssetDatabase.IsValidFolder(DynamicConstants.pluginsPath))
                {
                    AssetDatabase.CreateFolder(Constants.rootAssetsName, Constants.pluginsName);
                    AssetDatabase.CreateFolder(DynamicConstants.pluginsPath, Constants.mainFolderIconName);

                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.emptyFolderIconsName);
                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.folderIconsName);
                }
                else if (!AssetDatabase.IsValidFolder(DynamicConstants.mainFolderPath))
                {
                    AssetDatabase.CreateFolder(DynamicConstants.pluginsPath, Constants.mainFolderIconName);

                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.emptyFolderIconsName);
                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.folderIconsName);

                    if (IconManager.persistentData == null)
                    {
                        IconManager.persistentData = ScriptableObject.CreateInstance<PersistentData>();
                        AssetDatabase.CreateAsset(IconManager.persistentData, DynamicConstants.persistentDataPath);
                        AssetDatabase.ImportAsset(DynamicConstants.persistentDataPath);

                        IconManager.LoadDefaultIconSetsFromPackages($"{DynamicConstants.absolutePackagePath}\\{Constants.dataFolderName}\\{Constants.defaultIconJsonName}");

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                if (!AssetDatabase.IsValidFolder(DynamicConstants.emptyIconFolderPath))
                {
                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, DynamicConstants.emptyIconFolderPath);
                }

                if (!AssetDatabase.IsValidFolder(DynamicConstants.iconFolderPath))
                {
                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.folderIconsName);
                }

                if (!AssetDatabase.IsValidFolder(DynamicConstants.loadedIconSetPath))
                {
                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.loadedIconSetsName);
                }

                if (!AssetDatabase.IsValidFolder(DynamicConstants.loadedIconsPath))
                {
                    AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, Constants.loadedIconsFolderName);
                }
            }

            // Project Installed Locally
            else if(!IconManager.isProjectInstalledExternally)
            {
                if (AssetDatabase.IsValidFolder(DynamicConstants.mainFolderPath))
                {
                    if (!AssetDatabase.IsValidFolder($"{DynamicConstants.mainFolderPath}/{Constants.dataFolderName}"))
                    {
                        AssetDatabase.CreateFolder(DynamicConstants.mainFolderPath, $"{DynamicConstants.mainFolderPath}/{Constants.dataFolderName}");
                    }
                    if (!AssetDatabase.IsValidFolder(DynamicConstants.emptyIconFolderPath))
                    {
                        AssetDatabase.CreateFolder($"{DynamicConstants.mainFolderPath}/{Constants.allIconsFolderName}", Constants.emptyFolderIconsName);
                    }
                    if (!AssetDatabase.IsValidFolder(DynamicConstants.iconFolderPath))
                    {
                        AssetDatabase.CreateFolder($"{DynamicConstants.mainFolderPath}/{Constants.allIconsFolderName}", Constants.folderIconsName);
                    }
                    if (!AssetDatabase.IsValidFolder(DynamicConstants.loadedIconSetPath))
                    {
                        AssetDatabase.CreateFolder($"{DynamicConstants.mainFolderPath}/{Constants.allIconsFolderName}", Constants.loadedIconSetsName);
                    }
                    if (!AssetDatabase.IsValidFolder(DynamicConstants.loadedIconsPath))
                    {
                        AssetDatabase.CreateFolder($"{DynamicConstants.mainFolderPath}/{Constants.allIconsFolderName}", Constants.loadedIconsFolderName);
                    }
                }
            }
            
        }

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

        // When creating a new folder, update its icon to given name to itself. This is for Icon Sets.
        internal static void UpdateCurrentFolderIconWithIconSet(string assetPath)
        {
            if (IconManager.persistentData == null) return;
            if (IconManager.persistentData.currentIconSetIndex == 0) return;
            if (IconManager.persistentData.iconSetDataList.Count == 0) return;


            if (IconManager.persistentData.iconSetDataList.Count > IconManager.persistentData.currentIconSetIndex)
            {
                int iconSetIndex = IconManager.persistentData.currentIconSetIndex;

                bool isThisNameExistInIconSetDict = IconManager.persistentData.iconSetDataList[iconSetIndex].iconSetData.
                    Any(iconSetData => iconSetData.folderName == Path.GetFileNameWithoutExtension(assetPath));

                string currentGUID = AssetDatabase.AssetPathToGUID(assetPath);

                GUIDTextureData currentGUIDTextureData = IconManager.persistentData.guidTextureList.Find(word => word.guid == currentGUID);

                if (isThisNameExistInIconSetDict)
                {
                    Texture2D icon = IconManager.persistentData.iconSetDataList[iconSetIndex].iconSetData.
                        Find(x => x.folderName == Path.GetFileNameWithoutExtension(assetPath)).icon;

                    if (!IconManager.persistentData.guidTextureList.Contains(currentGUIDTextureData))
                    {
                        GUIDTextureData guidTextureData = new GUIDTextureData();
                        guidTextureData.guid = currentGUID;
                        guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, icon);

                        IconManager.persistentData.guidTextureList.Add(guidTextureData);
                        if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
                    }
                    else
                    {
                        GUIDTextureData guidTextureData = new GUIDTextureData();
                        guidTextureData.guid = currentGUID;
                        guidTextureData.textureData = TextureFunctions.CreateTextureData(Color.clear, null, null, icon);

                        int guidTextureDataIndex = IconManager.persistentData.guidTextureList.FindIndex(x => x.guid == currentGUID);
                        IconManager.persistentData.guidTextureList[guidTextureDataIndex] = guidTextureData;

                    }

                }
                else
                {
                    IconManager.persistentData.guidTextureList.Remove(currentGUIDTextureData);
                    if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

                    UtilityFunctions.ReDrawFolders();
                }

            }
        }

        // Draw one item in the project window
        internal static void DrawTextures(Rect rect, Texture2D texture2d)
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

        // Main function for drawing project window items
        internal static void DrawFolders(string guid, Rect selectionRect)
        {
            if (IconManager.persistentData.guidTextureList.Count == 0) return; // If there is not a custom icon return
            if (guid == "00000000000000000000000000000000") return; // Ignore main assets folder
            if (guid == "00000000000000001000000000000000") return; // Ignore main packages folder
            
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);



            //if (folderPath.StartsWith("Package")) return;
            if (!AssetDatabase.IsValidFolder(folderPath)) return;


            // Draw default and empty folders
            if (!IconManager.persistentData.guidTextureList.Any(x => x.guid == guid))
            {

                IconManager.folderEmptyDict.TryGetValue(folderPath, out bool outputBool);
                if (outputBool)
                    DrawTextures(selectionRect, DynamicConstants.defaultFolderIcon);
                else
                    DrawTextures(selectionRect, DynamicConstants.emptyDefaultFolderIcon);
            }

            else
            {
                for (int i = 0; i < IconManager.persistentData.guidTextureList.Count; i++)
                {
                    GUIDTextureData textureData = IconManager.persistentData.guidTextureList[i];

                    if (textureData.guid != guid) continue;

                    if (textureData.textureData.folderTexture != null)
                    {
                        IconManager.folderEmptyDict.TryGetValue(folderPath, out bool outputBool);
                        if (outputBool)
                        {
                            DrawTextures(selectionRect, textureData.textureData.folderTexture);
                        }
                        else
                        {
                            DrawTextures(selectionRect, textureData.textureData.emptyFolderTexture);
                        }
                    }
                    else if (textureData.textureData.customTexture != null)
                    {
                        DrawTextures(selectionRect, textureData.textureData.customTexture);
                    }
                }
            }


        }

        // Redraw EveryFolders with DrawFolders Function
        internal static void ReDrawFolders()
        {
            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            AssetDatabase.Refresh();
            EditorApplication.RepaintProjectWindow();
        }
    }



    // Without JsonHelper we have to wrap every list to a serializable class
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
