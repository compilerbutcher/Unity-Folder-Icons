using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace UnityEditorTools.FolderIcons
{

    [InitializeOnLoad]
    internal sealed class IconManager
    {
        // PersistentData variables
        internal static PersistentData persistentData;
        //internal static Dictionary<string, TextureData> tempFolderIconDict;
        //internal static Dictionary<string, Dictionary<string, Texture2D>> tempIconSetDict;

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
            DynamicConstants.UpdateDynamicConstants();

            AssetOperations();
            InitHeaderContents();
            //ExchangeFolderIconData(persistentData.guidTextureList, tempFolderIconDict, DataExchangeType.ListToDict);
            //ExchangeIconSetData(persistentData.iconSetDataList, tempIconSetDict, DataExchangeType.ListToDict);


            if (persistentData.guidTextureList.Count > 0)
            {
                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();
            }
            AssetDatabase.Refresh();
            EditorApplication.quitting += SaveDataToCollections;

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

        [MenuItem("Tools/Test")]
        static void a()
        {
            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture, ref IconManager.projectCurrentFolderTexture);
            byte[] emptyFolderTextureBytes = IconManager.projectCurrentEmptyFolderTexture.EncodeToPNG();
            byte[] folderTextureBytes = IconManager.projectCurrentFolderTexture.EncodeToPNG();

            string emptyTexturePath = $"{DynamicConstants.emptyIconFolderPath}/{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject))}.png";
            string texturePath = $"{DynamicConstants.iconFolderPath}/{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject))}.png";

            File.WriteAllBytes(Path.GetFullPath(emptyTexturePath), emptyFolderTextureBytes);
            File.WriteAllBytes(Path.GetFullPath(texturePath), folderTextureBytes);

            Debug.Log(Path.GetFullPath(emptyTexturePath));
            TextureImporter importer = TextureImporter.GetAtPath(emptyTexturePath) as TextureImporter;
            Debug.Log(emptyTexturePath);
            Debug.Log(importer);

        }

        // We need to assign and create data for temporary dictionary and persistentData and load default textures
        private static void AssetOperations()
        {

            UtilityFunctions.CheckAndCreateFolderStorage();




            folderEmptyDict = new Dictionary<string, bool>();
            //tempFolderIconDict ??= new Dictionary<string, TextureData>();
            //tempIconSetDict ??= new Dictionary<string, Dictionary<string, Texture2D>>();


            persistentData = AssetDatabase.LoadAssetAtPath<PersistentData>(DynamicConstants.persistentDataPath);
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref folderEmptyDict);

            if (persistentData == null)
            {
                throw new NullReferenceException($"Persistent Data is not exist at: {DynamicConstants.persistentDataPath}");
            }

            iconSetNames = new string[persistentData.iconSetDataList.Count];

            for (int i = 0; i < persistentData.iconSetDataList.Count; i++)
            {
                iconSetNames[i] = persistentData.iconSetDataList[i].iconSetName;
            }
        }


        // We need to save all dictionary elements to persistentData.guidTextureList to persist data between unity sessions
        internal static void SaveDataToCollections()
        {
            //if (isMarkedAsResetIcons)
            //{
            //    string[] allImmutableFolderIconsPath = Directory.GetFiles(DynamicConstants.folderStoragePath);

            //    for (int i = 0; i < allImmutableFolderIconsPath.Length; i++)
            //    {
            //        string currentPath = allImmutableFolderIconsPath[i];

            //        if (Path.GetFileNameWithoutExtension(currentPath) != "keep this folder" && Path.GetFileNameWithoutExtension(currentPath) != "keep this folder.gitkeep")
            //        {
            //            File.Delete(currentPath);
            //        }
            //    }
            //    File.WriteAllText("C:\\Users\\CodeParadise\\Desktop\\Hello.txt", "Helloismarkedrun!");
            //    isMarkedAsResetIcons = false;
            //}

            //UpdateMainImmutablePackage();

            //ExchangeFolderIconData(persistentData.guidTextureList, tempFolderIconDict, DataExchangeType.DictToList);
            //ExchangeIconSetData(persistentData.iconSetDataList, tempIconSetDict, DataExchangeType.DictToList);
            if (persistentData != null) EditorUtility.SetDirty(persistentData);
        }


        //[MenuItem("Tools/Test")]
        //private static void UpdateAllFolderIcons()
        //{
            //string[] emptyIconsFolderPath = Directory.GetFiles(Path.GetFullPath());
            //string[] iconsFolderPath = Directory.GetFiles(DynamicConstants.iconsFolderPath);



            //for (int i = 0; i < emptyIconsFolderPath.Length; i++)
            //{
            //    string emptyIconPath = emptyIconsFolderPath[i];
            //    string iconPath = iconsFolderPath[i];

            //    Texture2D emptyTexture2D = new Texture2D(0, 0);
            //    Texture2D texture2D = new Texture2D(0, 0);

            //    byte[] emptyImageBytes = File.ReadAllBytes(emptyIconPath);
            //    byte[] imageBytes = File.ReadAllBytes(iconPath);

            //    emptyTexture2D.LoadImage(emptyImageBytes);
            //    texture2D.LoadImage(imageBytes);

            //    string currentGUID = Path.GetFileNameWithoutExtension(emptyIconPath);

            //    UtilityFunctions.CreateAndSaveDataToDict(currentGUID, tempFolderIconDict, emptyTexture2D.GetPixel(0, 0), emptyTexture2D, texture2D, null);

            //}

            //string[] getFiles = Directory.GetFiles(DynamicConstants.persistentEditorFolderPath);

            //Dictionary<string, TextureData> textureDataList = new Dictionary<string, TextureData>();


            //foreach (var i in tempFolderIconDict)
            //{
            //    string folderTexturePath = getFiles.First(x => Path.GetFileNameWithoutExtension(x) == i.Key);
            //    string emptyFolderTexturePath = getFiles.First(x => Path.GetFileNameWithoutExtension(x)[5..] == i.Key);

            //    Texture2D folderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(folderTexturePath);
            //    Texture2D emptyFolderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyFolderTexturePath);

            //    TextureData createdTextureData = new TextureData();

            //    createdTextureData.color = folderTexture.GetPixel(0, 0);
            //    createdTextureData.emptyFolderTexture = emptyFolderTexture;
            //    createdTextureData.folderTexture = folderTexture;
            //    createdTextureData.customTexture = null;

            //    textureDataList.Add(i.Key, createdTextureData);
            //}
            //tempFolderIconDict.Clear();
            //tempFolderIconDict = textureDataList;
            //AssetDatabase.Refresh();
        //}




        internal static void LoadIconSetsFromJson(string selectedFile)
        {
            List<MainIconSetData> jsonDataList;
            JsonHelper.LoadJson<MainIconSetData>(selectedFile, out jsonDataList);


            for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
            {
                MainIconSetData data = jsonDataList[loadedJsonDataIndex];

                IconSetDataListWrapper newIconSetDataListWrapper = new IconSetDataListWrapper();
                newIconSetDataListWrapper.iconSetData = new List<IconSetData>();
                newIconSetDataListWrapper.iconSetName = data.iconSetName;

                IconSetData newIconSetData;

                for (int iconSetIndex = 0; iconSetIndex < data.iconSetData.Count; iconSetIndex++)
                {
                    newIconSetData = new();
                    string folderName = data.iconSetData[iconSetIndex].folderName;
                    string iconName = data.iconSetData[iconSetIndex].iconName;
                    string base64Texture = data.iconSetData[iconSetIndex].iconBase64;


                    string fullPackagePath = DynamicConstants.absolutePackagePath + Constants.iconsFolderName + Constants.loadedIconSetsName;
                    string unityRelativePackagePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconSetsName;

                    TextureFunctions.Base64ToTexture2D(base64Texture, fullPackagePath + $"/{iconName}.png");
                    TextureFunctions.ImportTexture(unityRelativePackagePath + $"/{iconName}.png");


                    newIconSetData.folderName = folderName;
                    newIconSetData.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(unityRelativePackagePath + $"/{iconName}.png");
                    newIconSetData.icon.name = iconName;

                    newIconSetDataListWrapper.iconSetData.Add(newIconSetData);
                }


                IconManager.persistentData.iconSetDataList.Add(newIconSetDataListWrapper);
                if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

            }
        }
        internal static void SaveIconSetsFromJson(string selectedFile)
        {
            List<MainIconSetData> packedIconSetData = new List<MainIconSetData>();

            for (int i = 0; i < IconManager.persistentData.iconSetDataList.Count; i++)
            {
                IconSetDataListWrapper iconSet = IconManager.persistentData.iconSetDataList[i];

                MainIconSetData newMainIconSetData = new();
                newMainIconSetData.iconSetName = iconSet.iconSetName;
                newMainIconSetData.iconSetData = new List<Base64IconSetData>();

                for (int i2 = 0; i2 < iconSet.iconSetData.Count; i2++)
                {
                    IconSetData iconSetData = iconSet.iconSetData[i2];


                    Base64IconSetData newIconSetData = new Base64IconSetData();
                    newIconSetData.folderName = iconSetData.folderName;
                    newIconSetData.iconName = iconSetData.icon.name;
                    newIconSetData.iconBase64 = Convert.ToBase64String(ImageConversion.EncodeToPNG(iconSetData.icon));

                    newMainIconSetData.iconSetData.Add(newIconSetData);
                }
                packedIconSetData.Add(newMainIconSetData);
            }

            JsonHelper.SaveJson<MainIconSetData>(selectedFile, packedIconSetData);
        }

        internal static void LoadIconsFromJson(string selectedFile)
        {
            string folderName = "";
            Color color;
            string emptyFolderBase64String;
            string folderTextureBase64String;
            string customTextureBase64String;

            List<JsonTextureData> jsonDataList;
            JsonHelper.LoadJson<JsonTextureData>(selectedFile, out jsonDataList);

            for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
            {
                JsonTextureData data = jsonDataList[loadedJsonDataIndex];


                folderName = data.folderName;
                color = new Color(data.color.x, data.color.y, data.color.z, data.color.w);


                emptyFolderBase64String = data.emptyFolderTextureBase64;
                folderTextureBase64String = data.folderTextureBase64;
                customTextureBase64String = data.customTextureBase64;


                string emptyFolderTextureFullPath = $"{DynamicConstants.absolutePackagePath}\\{Constants.iconsFolderName}\\{Constants.loadedIconsFolderName}\\{data.emptyFolderTextureName}.png";
                string folderTextureFullPath = $"{DynamicConstants.absolutePackagePath}\\{Constants.iconsFolderName}\\{Constants.loadedIconsFolderName}\\{data.folderTextureName}.png";
                string customFolderTextureFullPath = $"{DynamicConstants.absolutePackagePath}\\{Constants.iconsFolderName}\\{Constants.loadedIconsFolderName}\\{data.customTextureName}.png";

                TextureFunctions.Base64ToTexture2D(emptyFolderBase64String, emptyFolderTextureFullPath);
                TextureFunctions.Base64ToTexture2D(folderTextureBase64String, folderTextureFullPath);
                TextureFunctions.Base64ToTexture2D(customTextureBase64String, customFolderTextureFullPath);

                string emptyFolderTexturePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconsFolderName + $"/{data.emptyFolderTextureName}.png";
                string folderTexturePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconsFolderName + $"/{data.folderTextureName}.png";
                string customFolderTexturePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconsFolderName + $"/{data.customTextureName}.png";

                if (emptyFolderBase64String.Length > 0)
                    TextureFunctions.ImportTexture(emptyFolderTexturePath);
                if (folderTextureBase64String.Length > 0)
                    TextureFunctions.ImportTexture(folderTexturePath);
                if (customTextureBase64String.Length > 0)
                    TextureFunctions.ImportTexture(customFolderTexturePath);


                string newlyCreatedFolderGUID = AssetDatabase.CreateFolder("Assets", data.folderName);

                Color currentColor = new Color(data.color.x, data.color.y, data.color.z, data.color.w);
                Texture2D emptyFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyFolderTexturePath);
                Texture2D colorFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(folderTexturePath);
                Texture2D customFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(customFolderTexturePath);
                //UtilityFunctions.CreateAndSaveDataToDict(newlyCreatedFolderGUID, IconManager.tempFolderIconDict, currentColor, emptyFolder, colorFolder, customFolder);
            }


            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref IconManager.folderEmptyDict);
        }
        internal static void SaveIconsToJson(string selectedFile)
        {

            List<JsonTextureData> packedJsonList = new List<JsonTextureData>();
            for (int i = 0; i < persistentData.guidTextureList.Count; i++)
            {
                string guid = persistentData.guidTextureList[i].guid;
                TextureData textureData = persistentData.guidTextureList[i].textureData;



                string emptyFolderBase64String = "";
                string folderTextureBase64String = "";
                string customTextureBase64String = "";


                // Guid
                string folderName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));


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
