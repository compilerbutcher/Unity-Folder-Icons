using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Permissions;
using System;

namespace UnityEditorTools.FolderIcons
{

    [InitializeOnLoad]
    internal sealed class IconManager
    {
        // PersistentData variables
        internal static PersistentData persistentData;
        internal static Dictionary<string, TextureData> tempFolderIconDict;
        internal static Dictionary<string, Dictionary<string, Texture2D>> tempIconSetDict;

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
            ExchangeFolderIconData(persistentData.guidTextureList, tempFolderIconDict, DataExchangeType.ListToDict);
            ExchangeIconSetData(persistentData.iconSetDataList, tempIconSetDict, DataExchangeType.ListToDict);


            if (tempFolderIconDict.Count > 0)
            {
                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();
            }
            AssetDatabase.Refresh();
            EditorApplication.quitting += SaveDataToCollections;

            //if (!SessionState.GetBool("OnlyRunWhenEditorStarted", false))
            //{
            //    SessionState.SetBool("OnlyRunWhenEditorStarted", true);
            //}
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
                        ref persistentData.headerContents.openButton, DynamicConstants.buttonDefaultColor,
                        DynamicConstants.buttonHoverColor);

                    persistentData.isHeaderContentsCreated = true;
                    EditorUtility.SetDirty(persistentData);
                }
            }
        }

        // We need to assign and create data for temporary dictionary and persistentData and load default textures
        private static void AssetOperations()
        {
            folderEmptyDict = new Dictionary<string, bool>();
            tempFolderIconDict ??= new Dictionary<string, TextureData>();
            tempIconSetDict ??= new Dictionary<string, Dictionary<string, Texture2D>>();

            persistentData = AssetDatabase.LoadAssetAtPath<PersistentData>(DynamicConstants.persistentDataPath);
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref folderEmptyDict);

            if (persistentData == null)
            {
                persistentData = ScriptableObject.CreateInstance<PersistentData>();
                AssetDatabase.CreateAsset(persistentData, DynamicConstants.persistentDataPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }




            if (persistentData.iconSetDataList.Count == 0)
            {
                LoadIconSetsFromJson(DynamicConstants.absolutePackagePath + Constants.dataName + Constants.defaultIconSetName);
            }
            else
            {
                if (persistentData.iconSetDataList[0].iconSetName != "Default" && persistentData.iconSetDataList[1].iconSetName != "Medieval")
                {
                    LoadIconSetsFromJson(DynamicConstants.absolutePackagePath + Constants.dataName + Constants.defaultIconSetName);
                }
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
            ExchangeFolderIconData(persistentData.guidTextureList, tempFolderIconDict, DataExchangeType.DictToList);
            ExchangeIconSetData(persistentData.iconSetDataList, tempIconSetDict, DataExchangeType.DictToList);
            if (persistentData != null) EditorUtility.SetDirty(persistentData);
            for (int i = 0; i < persistentData.iconSetDataList.Count; i++)
            {
                File.WriteAllText("C:\\Users\\CodeParadise\\Desktop\\A.txt", tempIconSetDict.ElementAt(i).Key);
            }
        }



        // Exchange icon set data
        internal static void ExchangeIconSetData(List<IconSetDataListWrapper> list, Dictionary<string, Dictionary<string, Texture2D>> dict, DataExchangeType dataExchangeType)
        {
            // Move all data from persistent list to temporary dictionary
            switch (dataExchangeType)
            {
                case DataExchangeType.ListToDict:
                    ListToDict(ref list, ref dict);
                    break;


                // Move all data from temporary dictionary to persistent list
                case DataExchangeType.DictToList:
                    DictToList(ref list, ref dict);
                    break;

            }

        }

        private static void ListToDict(ref List<IconSetDataListWrapper> list, ref Dictionary<string, Dictionary<string, Texture2D>> dict)
        {
            if (list.Count == 0) return;
            dict.Clear();


            for (int i = 0; i < list.Count; i++)
            {
                Dictionary<string, Texture2D> iconSetDataDict = new Dictionary<string, Texture2D>();

                for (int x = 0; x < list[i].iconSetData.Count; x++)
                {
                    if (!iconSetDataDict.ContainsKey(list[i].iconSetData[x].folderName))
                    {
                        iconSetDataDict.Add(list[i].iconSetData[x].folderName, list[i].iconSetData[x].icon);
                    }
                }
                dict.Add(list[i].iconSetName, iconSetDataDict);
            }

        }

        private static void ListToDictOneElement(ref List<IconSetDataListWrapper> list, ref Dictionary<string, Texture2D> dict)
        {
            if (list.Count == 0) return;

            IconSetDataListWrapper iconSetData = list[^1];

            //if (!dict.ContainsKey(iconSetData))
            //{

            //}
            
        }

        private static void DictToList(ref List<IconSetDataListWrapper> list, ref Dictionary<string, Dictionary<string, Texture2D>> dict)
        {
            if (dict.Count == 0) return;
            list.Clear();

            foreach (KeyValuePair<string, Dictionary<string, Texture2D>> iconSet in dict)
            {
                IconSetDataListWrapper wrapper = new IconSetDataListWrapper();
                List<IconSetData> iconSetDatas = new List<IconSetData>();
                wrapper.iconSetName = iconSet.Key;
                for (int i = 0; i < iconSet.Value.Count; i++)
                {
                    IconSetData iconSetData = new IconSetData();
                    iconSetData.folderName = iconSet.Value.ElementAt(i).Key;
                    iconSetData.icon = iconSet.Value.ElementAt(i).Value;

                    iconSetDatas.Add(iconSetData);
                }
                wrapper.iconSetData = iconSetDatas;
                if (!list.Contains(wrapper))
                {
                    list.Add(wrapper);
                }
            }
        }
        // Exchange folder texture data
        internal static void ExchangeFolderIconData(List<GUIDTextureData> list, Dictionary<string, TextureData> dict, DataExchangeType dataExchangeType)
        {
            switch (dataExchangeType)
            {
                case DataExchangeType.ListToDict:
                    if (list.Count == 0) return;
                    dict.Clear();

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!dict.ContainsKey(list[i].guid))
                            dict.Add(list[i].guid, list[i].textureData);
                    }
                    break;

                case DataExchangeType.DictToList:
                    if (dict.Count == 0) return;
                    list.Clear();
                    EditorUtility.SetDirty(persistentData);


                    GUIDTextureData keyValueData = new();

                    foreach (KeyValuePair<string, TextureData> i in dict)
                    {
                        keyValueData.guid = i.Key;
                        keyValueData.textureData.color = i.Value.color;
                        keyValueData.textureData.emptyFolderTexture = i.Value.emptyFolderTexture;
                        keyValueData.textureData.folderTexture = i.Value.folderTexture;
                        keyValueData.textureData.customTexture = i.Value.customTexture;

                        if (!list.Contains(keyValueData))
                            list.Add(keyValueData);

                        keyValueData.Clear();
                    }
                    break;
            }

        }



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
            Debug.Log(IconManager.tempIconSetDict.Count);

            for (int i = 0; i < IconManager.tempIconSetDict.Count; i++)
            {
                KeyValuePair<string, Dictionary<string, Texture2D>> iconSetDict = IconManager.tempIconSetDict.ElementAt(i);

                MainIconSetData newMainIconSetData = new();
                newMainIconSetData.iconSetName = iconSetDict.Key;
                newMainIconSetData.iconSetData = new List<Base64IconSetData>();

                foreach (KeyValuePair<string, Texture2D> keyValue in iconSetDict.Value)
                {
                    Base64IconSetData newIconSetData = new Base64IconSetData();
                    newIconSetData.folderName = keyValue.Key;
                    newIconSetData.iconName = keyValue.Value.name;
                    newIconSetData.iconBase64 = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value));

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
                UtilityFunctions.CreateAndSaveDataToDict(newlyCreatedFolderGUID, IconManager.tempFolderIconDict, currentColor, emptyFolder, colorFolder, customFolder);
            }


            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref IconManager.folderEmptyDict);
        }
        internal static void SaveIconsToJson(string selectedFile)
        {

            List<JsonTextureData> packedJsonList = new List<JsonTextureData>();
            foreach (KeyValuePair<string, TextureData> keyValue in IconManager.tempFolderIconDict)
            {
                string emptyFolderBase64String = "";
                string folderTextureBase64String = "";
                string customTextureBase64String = "";


                // Guid
                string folderName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(keyValue.Key));


                if (keyValue.Value.emptyFolderTexture != null)
                {
                    emptyFolderBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value.emptyFolderTexture));
                }
                if (keyValue.Value.folderTexture != null)
                {
                    folderTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value.folderTexture));
                }
                if (keyValue.Value.customTexture != null)
                {
                    customTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value.customTexture));
                }

                JsonTextureData packedJsonData = new JsonTextureData();

                if (keyValue.Value.emptyFolderTexture != null)
                {
                    packedJsonData.emptyFolderTextureName = keyValue.Value.emptyFolderTexture.name;
                    packedJsonData.folderTextureName = keyValue.Value.folderTexture.name;
                }
                if (keyValue.Value.customTexture != null)
                    packedJsonData.customTextureName = keyValue.Value.customTexture.name;




                packedJsonData.folderName = folderName;
                packedJsonData.color = new Vector4(keyValue.Value.color.r, keyValue.Value.color.g, keyValue.Value.color.b, keyValue.Value.color.a);

                packedJsonData.emptyFolderTextureBase64 = emptyFolderBase64String;
                packedJsonData.folderTextureBase64 = folderTextureBase64String;
                packedJsonData.customTextureBase64 = customTextureBase64String;

                packedJsonList.Add(packedJsonData);
            }

            JsonHelper.SaveJson<JsonTextureData>(selectedFile, packedJsonList);
            AssetDatabase.Refresh();
            Debug.Log(IconManager.tempFolderIconDict.Count);
        }
    }
}
