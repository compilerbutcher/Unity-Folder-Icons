using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace UnityEditorTools.FolderIcons
{

    [InitializeOnLoad]
    internal sealed class IconManager
    {
        // Persi
        // stentData variables
        internal static PersistentData persistentData;
        internal static Dictionary<string, TextureData> tempFolderIconDict;
        internal static Dictionary<string, List<IconSetData>> tempIconSetDict;

        // Project current values
        internal static Color projectCurrentColor;
        internal static Texture2D projectCurrentEmptyFolderTexture;
        internal static Texture2D projectCurrentFolderTexture;

        internal static Texture2D projectCurrentCustomTexture;

        internal static Dictionary<string, bool> folderEmptyDict;

        



        static IconManager()
        {

            EditorApplication.delayCall += Main;
        }

        // Main function that includes everything that must be running for delayCall
        // We have to make sure use delayCall with asset operations otherwise, asset operations will sometimes fail or make weird behaviour
        private static void Main()
        {
            DynamicConstants.UpdateVariables();

            AssetOperations();
            InitHeaderContents();
            ExchangeFolderIconData(persistentData.guidTextureList, tempFolderIconDict, true);
            ExchangeIconSetData(persistentData.iconSetDataList, tempIconSetDict, true);

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
            tempIconSetDict ??= new Dictionary<string, List<IconSetData>>();

            persistentData = AssetDatabase.LoadAssetAtPath<PersistentData>(DynamicConstants.persistentDataPath);
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref folderEmptyDict);

            if (persistentData == null)
            {
                persistentData = ScriptableObject.CreateInstance<PersistentData>();
                AssetDatabase.CreateAsset(persistentData, DynamicConstants.persistentDataPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }


        // We need to save all dictionary elements to persistentData.guidTextureList to persist data between unity sessions
        internal static void SaveDataToCollections()
        {
            ExchangeFolderIconData(persistentData.guidTextureList, tempFolderIconDict, false);
            ExchangeIconSetData(persistentData.iconSetDataList, tempIconSetDict, false);
            if (persistentData != null) EditorUtility.SetDirty(persistentData);
        }



        // Exchange icon set data
        private static void ExchangeIconSetData(List<IconSetDataListWrapper> list, Dictionary<string, List<IconSetData>> dict, bool isListToDict)
        {
            // Move all data from persistent list to temporary dictionary
            if (isListToDict)
            {
                if (list.Count == 0) return;
                dict.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    if (!dict.ContainsKey(list[i].iconSetName))
                        dict.Add(list[i].iconSetName, list[i].iconSetData);
                }
            }


            // Move all data from temporary dictionary to persistent list
            else
            {
                if (dict.Count == 0) return;
                list.Clear();

                foreach (KeyValuePair<string, List<IconSetData>> i in dict)
                {
                    IconSetDataListWrapper wrapper = new IconSetDataListWrapper();
                    wrapper.iconSetName = i.Key;
                    wrapper.iconSetData = i.Value;

                    if (!list.Contains(wrapper))
                    {
                        list.Add(wrapper);
                    }
                }
            }

        }


        // Exchange folder texture data
        internal static void ExchangeFolderIconData(List<GUIDTextureData> list, Dictionary<string, TextureData> dict, bool isListToDict)
        {
            // Move all data from persistent list to temporary dictionary
            if (isListToDict)
            {
                if (list.Count == 0) return;
                dict.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    if (!dict.ContainsKey(list[i].guid))
                        dict.Add(list[i].guid, list[i].textureData);
                }
            }


            // Move all data from temporary dictionary to persistent list
            else
            {

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
            }
        }

    }
}
