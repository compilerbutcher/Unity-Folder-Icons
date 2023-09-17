using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace UnityEditorTools.FolderIcons
{

    [InitializeOnLoad]
    internal sealed class IconManager
    {
        public static void CheckInstallPlace()
        {
            string packageName = "com.compilerbutcher.foldericons";

            // Check if the package is installed
            {
                // Check if the package is locally installed
                isProjectInstalledExternally = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/" + packageName).source != UnityEditor.PackageManager.PackageSource.Local;

                if (isProjectInstalledExternally)
                {
                    Debug.Log(packageName + "project is installed from git.");
                }
                else
                {
                    Debug.Log(packageName + "project is installed from local");
                }
            }
        }



        // PersistentData variables
        internal static PersistentData persistentData;
        internal static bool isProjectInstalledExternally;

        // Project current values
        internal static Color projectCurrentColor;
        internal static Texture2D projectCurrentEmptyFolderTexture;
        internal static Texture2D projectCurrentFolderTexture;

        internal static Texture2D projectCurrentCustomTexture;

        internal static Dictionary<string, bool> folderEmptyDict;
        internal static string[] iconSetNames;

        static IconManager()
        {
            EditorApplication.delayCall += Main;
        }

        // Main function that includes everything that must be running for delayCall
        // We have to make sure use delayCall with asset operations otherwise, asset operations will sometimes fail or make weird behaviour
        private static void Main()
        {
            CheckInstallPlace();
            DynamicConstants.UpdateDynamicConstants();
            AssetOperations();
            InitHeaderContents();


            if (persistentData.guidTextureList.Count > 0)
            {
                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();
            }
            AssetDatabase.Refresh();
            EditorApplication.quitting += SavePersistentData;

            if (!SessionState.GetBool("OnlyRunWhenEditorStarted", false))
            {
                SessionState.SetBool("OnlyRunWhenEditorStarted", true);
            }
        }

        private static void InitHeaderContents()
        {
            if (!persistentData.isHeaderContentsCreated)
            {
                if (persistentData != null)
                {
                    persistentData.headerContents = new HeaderContents();
                    HeaderFunctions.CreateInspectorHeaderContents(ref persistentData.headerContents.folderPopupWindowContent,
                        ref persistentData.headerContents.buttonBackgroundTexture, ref persistentData.headerContents.buttonHoverTexture,
                        ref persistentData.headerContents.headerIconGUIStyle, ref persistentData.headerContents.resetButtonGUIContent,
                        ref persistentData.headerContents.openButton, DynamicConstants.buttonDefaultColor, DynamicConstants.buttonHoverColor);

                    persistentData.isHeaderContentsCreated = true;
                    EditorUtility.SetDirty(persistentData);
                }
            }
        }

        //[MenuItem("Tools/Test")]
        //static void a()
        //{
        //    string path = "Assets/Ha/Deneme2/Merid/DASDAS/EN Ýyi Benim/HAHAHAHA/NiCE/NONONO/SUCKERS/GETDEStroyed";

        //    string[] pathSegments = path.Split('/');

        //    string[] resultPathLevels = new string[pathSegments.Length];

        //    for (int i = 0; i < pathSegments.Length; i++)
        //    {
        //        resultPathLevels[i] = string.Join("/", pathSegments.Take(i + 1));


        //        string currentFolderPath = resultPathLevels[i];

        //        if (currentFolderPath == "Assets") continue;

        //        if (!AssetDatabase.IsValidFolder(currentFolderPath))
        //        {
        //            AssetDatabase.CreateFolder(Path.GetDirectoryName(currentFolderPath), Path.GetFileName(currentFolderPath));
        //        }
        //        AssetDatabase.Refresh();

        //    }

        //}




        private static void AssetOperations()
        {
            UtilityFunctions.CheckAndCreateFolderStorage();

            folderEmptyDict = new Dictionary<string, bool>();


            persistentData = AssetDatabase.LoadAssetAtPath<PersistentData>(DynamicConstants.persistentDataPath);
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref folderEmptyDict);

            if (isProjectInstalledExternally)
            {
                if (persistentData == null)
                {
                    throw new NullReferenceException($"Persistent Data is not exist at: {DynamicConstants.persistentDataPath}");
                }
            }
            else if (!isProjectInstalledExternally)
            {
                if (persistentData == null)
                {
                    persistentData = ScriptableObject.CreateInstance<PersistentData>();
                    AssetDatabase.CreateAsset(persistentData, DynamicConstants.persistentDataPath);
                    AssetDatabase.ImportAsset(DynamicConstants.persistentDataPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }



            iconSetNames = new string[persistentData.iconSetDataList.Count];

            for (int i = 0; i < persistentData.iconSetDataList.Count; i++)
            {
                iconSetNames[i] = persistentData.iconSetDataList[i].iconSetName;
            }
        }


        internal static void SavePersistentData()
        {
            if (persistentData != null) EditorUtility.SetDirty(persistentData);
        }


        internal static void LoadIconSetsFromJson(string selectedFile)
        {
            UtilityFunctions.CheckAndCreateFolderStorage();


            List<MainIconSetData> jsonDataList;
            JsonHelper.LoadJson<MainIconSetData>(selectedFile, out jsonDataList);

            if (jsonDataList.Count == 0)
            {
                Debug.LogWarning("There is no icon sets in the json file to be load! You can create it via: Tools > Folder Icon Settings > Save Icon Sets!");
                return;
            }

            for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
            {
                MainIconSetData data = jsonDataList[loadedJsonDataIndex];

                IconSetDataListWrapper newIconSetDataListWrapper = new IconSetDataListWrapper();
                newIconSetDataListWrapper.iconSetData = new List<IconSetData>();
                newIconSetDataListWrapper.iconSetName = data.iconSetName;

                string iconSetImportPath = $"{DynamicConstants.loadedIconSetPath}/{data.iconSetName}";

                if (!AssetDatabase.IsValidFolder(iconSetImportPath))
                {
                    AssetDatabase.CreateFolder(DynamicConstants.loadedIconSetPath, data.iconSetName);
                }

                IconSetData newIconSetData;

                for (int iconSetIndex = 0; iconSetIndex < data.iconSetData.Count; iconSetIndex++)
                {
                    newIconSetData = new();
                    string folderName = data.iconSetData[iconSetIndex].folderName;
                    string iconName = data.iconSetData[iconSetIndex].iconName;
                    string base64Texture = data.iconSetData[iconSetIndex].iconBase64;

                    string fullPackagePath = Path.GetFullPath(iconSetImportPath);

                    TextureFunctions.Base64ToTexture2D(base64Texture, fullPackagePath + $"/{iconName}.png");
                    TextureFunctions.ImportTexture($"{iconSetImportPath}/{iconName}.png");


                    newIconSetData.folderName = folderName;
                    newIconSetData.icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconSetImportPath}/{iconName}.png");
                    newIconSetData.icon.name = iconName;

                    newIconSetDataListWrapper.iconSetData.Add(newIconSetData);
                }


                IconManager.persistentData.iconSetDataList.Add(newIconSetDataListWrapper);
                if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
            }
            AssetDatabase.Refresh();

        }
        internal static void SaveIconSetsFromJson(string selectedFile)
        {
            if (persistentData.iconSetDataList.Count <= 2)
            {
                Debug.LogWarning("There is no created icon sets! You can create it via: Tools/Folder Icon Settings!");
                return;
            }

            List<MainIconSetData> packedIconSetData = new List<MainIconSetData>();

            for (int i = 2; i < IconManager.persistentData.iconSetDataList.Count; i++)
            {
                IconSetDataListWrapper iconSet = IconManager.persistentData.iconSetDataList[i];

                MainIconSetData newMainIconSetData = new();
                newMainIconSetData.iconSetName = iconSet.iconSetName;
                newMainIconSetData.iconSetData = new List<Base64IconSetData>();

                for (int i2 = 0; i2 < iconSet.iconSetData.Count; i2++)
                {
                    IconSetData iconSetData = iconSet.iconSetData[i2];

                    Base64IconSetData newBase64IconSetData = new Base64IconSetData();
                    newBase64IconSetData.folderName = iconSetData.folderName;
                    newBase64IconSetData.iconName = iconSetData.icon.name;
                    newBase64IconSetData.iconBase64 = Convert.ToBase64String(ImageConversion.EncodeToPNG(iconSetData.icon));

                    newMainIconSetData.iconSetData.Add(newBase64IconSetData);
                }
                packedIconSetData.Add(newMainIconSetData);
            }

            JsonHelper.SaveJson<MainIconSetData>(selectedFile, packedIconSetData);
        }

        internal static void LoadIconsFromJson(string selectedFile)
        {
            UtilityFunctions.CheckAndCreateFolderStorage();



            string folderName = "";
            Color color;
            string emptyFolderBase64String;
            string folderTextureBase64String;
            string customTextureBase64String;

            List<JsonTextureData> jsonDataList;
            JsonHelper.LoadJson<JsonTextureData>(selectedFile, out jsonDataList);


            if (jsonDataList.Count == 0)
            {
                Debug.LogWarning("There is no icons in the json file to be load! You can create it via: Tools > Folder Icon Settings > Save Icons!");
                return;
            }


            for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
            {
                JsonTextureData jsonTextureData = jsonDataList[loadedJsonDataIndex];
                

                folderName = jsonTextureData.folderName;
                color = new Color(jsonTextureData.color.x, jsonTextureData.color.y, jsonTextureData.color.z, jsonTextureData.color.w);


                emptyFolderBase64String = jsonTextureData.emptyFolderTextureBase64;
                folderTextureBase64String = jsonTextureData.folderTextureBase64;
                customTextureBase64String = jsonTextureData.customTextureBase64;


                string emptyFolderTexturePath = $"{DynamicConstants.emptyIconFolderPath}\\{jsonTextureData.emptyFolderTextureName}.png";
                string folderTexturePath = $"{DynamicConstants.iconFolderPath}\\{jsonTextureData.folderTextureName}.png";
                string customFolderTexturePath = $"{DynamicConstants.loadedIconsPath}\\{jsonTextureData.customTextureName}.png";

                TextureFunctions.Base64ToTexture2D(emptyFolderBase64String, Path.GetFullPath(emptyFolderTexturePath));
                TextureFunctions.Base64ToTexture2D(folderTextureBase64String, Path.GetFullPath(folderTexturePath));
                TextureFunctions.Base64ToTexture2D(customTextureBase64String, Path.GetFullPath(customFolderTexturePath));

                AssetDatabase.Refresh();

                if (emptyFolderBase64String.Length > 0)
                    TextureFunctions.ImportTexture(emptyFolderTexturePath);
                if (folderTextureBase64String.Length > 0)
                    TextureFunctions.ImportTexture(folderTexturePath);
                if (customTextureBase64String.Length > 0)
                    TextureFunctions.ImportTexture(customFolderTexturePath);





                Color currentColor = new Color(jsonTextureData.color.x, jsonTextureData.color.y, jsonTextureData.color.z, jsonTextureData.color.w);
                Texture2D emptyFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyFolderTexturePath);
                Texture2D colorFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(folderTexturePath);
                Texture2D customFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(customFolderTexturePath);





                // Creating all folders within json data
                string[] pathSegments = jsonTextureData.folderName.Split('/');

                string[] resultPathLevels = new string[pathSegments.Length];

                for (int i = 0; i < pathSegments.Length; i++)
                {
                    resultPathLevels[i] = string.Join("/", pathSegments.Take(i + 1));


                    string currentFolderPath = resultPathLevels[i];


                    if (currentFolderPath == Constants.rootAssetsName) continue;


                    if (!AssetDatabase.IsValidFolder(currentFolderPath))
                    {
                        string currentFolderGUID = AssetDatabase.CreateFolder(Path.GetDirectoryName(currentFolderPath), Path.GetFileName(currentFolderPath));

                        if (currentFolderPath != jsonTextureData.folderName) continue;

                        GUIDTextureData newGuidTextureData = new GUIDTextureData();

                        TextureData newTextureData = new TextureData();
                        newTextureData.emptyFolderTexture = emptyFolderTexture;
                        newTextureData.folderTexture = colorFolderTexture;
                        newTextureData.customTexture = customFolderTexture;


                        newGuidTextureData.guid = currentFolderGUID;
                        newGuidTextureData.textureData = newTextureData;

                        persistentData.guidTextureList.Add(newGuidTextureData);
                        UtilityFunctions.UpdateFolderEmptyDict(Path.GetFullPath(currentFolderPath), ref IconManager.folderEmptyDict);

                    }
                    else if (AssetDatabase.IsValidFolder(currentFolderPath))
                    {
                        if (currentFolderPath != jsonTextureData.folderName) continue;

                        if (!persistentData.guidTextureList.Any(textureData => textureData.guid == AssetDatabase.AssetPathToGUID(currentFolderPath)))
                        {
                            GUIDTextureData newGuidTextureData = new GUIDTextureData();

                            TextureData textureData = TextureFunctions.CreateTextureData(IconManager.projectCurrentColor, emptyFolderTexture, colorFolderTexture, customFolderTexture);



                            newGuidTextureData.guid = AssetDatabase.AssetPathToGUID(currentFolderPath);
                            newGuidTextureData.textureData = textureData;

                            persistentData.guidTextureList.Add(newGuidTextureData);
                            UtilityFunctions.UpdateFolderEmptyDict(Path.GetFullPath(currentFolderPath), ref IconManager.folderEmptyDict);

                        }
                        else if (persistentData.guidTextureList.Any(textureData => textureData.guid == AssetDatabase.AssetPathToGUID(currentFolderPath)))
                        {
                            GUIDTextureData newGuidTextureData = new GUIDTextureData();
                            TextureData textureData = TextureFunctions.CreateTextureData(IconManager.projectCurrentColor, emptyFolderTexture, colorFolderTexture, customFolderTexture);


                            newGuidTextureData.guid = AssetDatabase.AssetPathToGUID(currentFolderPath);
                            newGuidTextureData.textureData = textureData;

                            int index = persistentData.guidTextureList.FindIndex(guidData => guidData.guid == AssetDatabase.AssetPathToGUID(currentFolderPath));
                            persistentData.guidTextureList[index] = newGuidTextureData;

                            UtilityFunctions.UpdateFolderEmptyDict(Path.GetFullPath(currentFolderPath), ref IconManager.folderEmptyDict);

                        }
                    }

                }

            }
            if (persistentData != null) EditorUtility.SetDirty(persistentData);


            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();
            AssetDatabase.Refresh();

        }
        internal static void SaveIconsToJson(string selectedFile)
        {
            if (persistentData.iconSetDataList.Count == 0)
            {
                Debug.LogWarning("There is no icons assigned to be save! You can create it via: clicking any folder then clicking folder icon in the inspector!");
                return;
            }


            List<JsonTextureData> packedJsonList = new List<JsonTextureData>();
            for (int i = 0; i < persistentData.guidTextureList.Count; i++)
            {
                string guid = persistentData.guidTextureList[i].guid;
                TextureData textureData = persistentData.guidTextureList[i].textureData;

                if (textureData.emptyFolderTexture != null)
                {
                    if (!textureData.emptyFolderTexture.isReadable)
                    {
                        Debug.LogWarning($"{textureData.emptyFolderTexture.name} is not readable! Please make sure every texture is readable before saving them!");

                        return;
                    }
                    else if (!textureData.folderTexture.isReadable)
                    {
                        Debug.LogWarning($"{textureData.folderTexture.name} is not readable! Please make sure every texture is readable before saving them!");

                        return;
                    }
                }
                else if (textureData.customTexture != null)
                {
                    if (!textureData.customTexture.isReadable)
                    {
                        Debug.LogWarning($"{textureData.customTexture.name} is not readable! Please make sure every texture is readable before saving them!");

                        return;
                    }
                }


                string emptyFolderBase64String = "";
                string folderTextureBase64String = "";
                string customTextureBase64String = "";


                // Guid
                string folderName = AssetDatabase.GUIDToAssetPath(guid);


                if (textureData.emptyFolderTexture != null)
                {
                    emptyFolderBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.emptyFolderTexture));
                }
                if (textureData.folderTexture != null)
                {
                    folderTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.folderTexture));
                }
                if (textureData.customTexture != null)
                {
                    customTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(textureData.customTexture));
                }

                JsonTextureData packedJsonData = new JsonTextureData();

                if (textureData.emptyFolderTexture != null)
                {
                    packedJsonData.emptyFolderTextureName = textureData.emptyFolderTexture.name;
                    packedJsonData.folderTextureName = textureData.folderTexture.name;
                }
                if (textureData.customTexture != null)
                    packedJsonData.customTextureName = textureData.customTexture.name;




                packedJsonData.folderName = folderName;
                packedJsonData.color = new Vector4(textureData.color.r, textureData.color.g, textureData.color.b, textureData.color.a);

                packedJsonData.emptyFolderTextureBase64 = emptyFolderBase64String;
                packedJsonData.folderTextureBase64 = folderTextureBase64String;
                packedJsonData.customTextureBase64 = customTextureBase64String;

                packedJsonList.Add(packedJsonData);
            }

            JsonHelper.SaveJson<JsonTextureData>(selectedFile, packedJsonList);
            AssetDatabase.Refresh();
        }
    }
}
