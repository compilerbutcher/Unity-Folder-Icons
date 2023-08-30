using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditorTools.FolderIcons
{
    public class FolderPostprocess : AssetPostprocessor
    {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (importedAssets.Length > 0)
            {
                for (int i = 0; i < importedAssets.Length; i++)
                {
                    try
                    {
                        string currentImportPath = importedAssets[i];

                        if (AssetDatabase.IsValidFolder(currentImportPath))
                        {
                            UtilityFunctions.UpdateFolderEmptyDict(currentImportPath, ref IconManager.folderEmptyDict);

                            UpdateIconsForIconSets(currentImportPath);

                            if (IconManager.folderEmptyDict.Count > 0)
                            {
                                ReDrawFolders();
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (deletedAssets.Length > 0)
            {
                for (int i = 0; i < deletedAssets.Length; i++)
                {
                    try
                    {
                        string currentImportPath = deletedAssets[i];

                        IconManager.tempFolderIconDict.Remove(AssetDatabase.AssetPathToGUID(currentImportPath));
                        IconManager.persistentData.guidTextureList.RemoveAll(x => x.guid == AssetDatabase.AssetPathToGUID(currentImportPath));
                        EditorUtility.SetDirty(IconManager.persistentData);

                        UtilityFunctions.UpdateFolderEmptyDict(currentImportPath, ref IconManager.folderEmptyDict);

                        if (IconManager.folderEmptyDict.Count > 0)
                        {
                            ReDrawFolders();
                        }
                    }
                    catch { }
                }
            }

            if (movedAssets.Length > 0 && movedFromAssetPaths.Length > 0)
            {
                for (int i = 0; i < importedAssets.Length; i++)
                {
                    string movedAssetPath = movedAssets[i];
                    string movedFromAssetPath = movedFromAssetPaths[i];


                    if (AssetDatabase.IsValidFolder(movedAssetPath))
                    {
                        UtilityFunctions.UpdateFolderEmptyDict(movedAssetPath, ref IconManager.folderEmptyDict);
                        UpdateIconsForIconSets(movedAssetPath);
                        if (IconManager.folderEmptyDict.Count > 0)
                        {
                            ReDrawFolders();
                        }

                    }
                    if (AssetDatabase.IsValidFolder(movedFromAssetPath))
                    {
                        UtilityFunctions.UpdateFolderEmptyDict(movedFromAssetPath, ref IconManager.folderEmptyDict);

                        if (IconManager.folderEmptyDict.Count > 0)
                        {
                            ReDrawFolders();
                        }
                    }
                }
            }
        }
        
        
        
        
        private static void UpdateIconsForIconSets(string assetPath)
        {
            if (IconManager.persistentData == null) return;
            if (IconManager.persistentData.currentIconSetIndex == 0) return;
            if (IconManager.persistentData.iconSetDataList.Count == 0) return;
            
                
            if (IconManager.persistentData.iconSetDataList.Count > IconManager.persistentData.currentIconSetIndex)
            {
                string iconSetName = IconManager.iconSetNames[IconManager.persistentData.currentIconSetIndex];

                bool isThisNameExistInIconSetDict = IconManager.tempIconSetDict[iconSetName].ContainsKey(Path.GetFileNameWithoutExtension(assetPath));

                string currentGUID = AssetDatabase.AssetPathToGUID(assetPath);

                GUIDTextureData currentGUIDTextureData = IconManager.persistentData.guidTextureList.Find(word => word.guid == currentGUID);

                if (isThisNameExistInIconSetDict)
                {
                    Texture2D icon = IconManager.tempIconSetDict[iconSetName][Path.GetFileNameWithoutExtension(assetPath)];

                    UtilityFunctions.CreateAndSaveDataToDict(currentGUID, IconManager.tempFolderIconDict, Color.clear, null, null, icon);


                    if (!IconManager.persistentData.guidTextureList.Contains(currentGUIDTextureData))
                    {
                        GUIDTextureData guidTextureData = new GUIDTextureData();
                        TextureData textureData = new TextureData();
                        textureData.emptyFolderTexture = null;
                        textureData.folderTexture = null;
                        textureData.color = Color.clear;
                        textureData.customTexture = icon;

                        guidTextureData.guid = currentGUID;
                        guidTextureData.textureData = textureData;

                        IconManager.persistentData.guidTextureList.Add(guidTextureData);
                        if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
                    }
                    else
                    {
                        GUIDTextureData guidTextureData = new GUIDTextureData();
                        TextureData textureData = new TextureData();
                        textureData.emptyFolderTexture = null;
                        textureData.folderTexture = null;
                        textureData.color = Color.clear;
                        textureData.customTexture = icon;

                        guidTextureData.guid = currentGUID;
                        guidTextureData.textureData = textureData;
                        int guidTextureDataIndex = IconManager.persistentData.guidTextureList.FindIndex(x => x.guid == currentGUID);
                        IconManager.persistentData.guidTextureList[guidTextureDataIndex] = guidTextureData;

                    }

                }
                else
                {
                    IconManager.tempFolderIconDict.Remove(currentGUID);
                    IconManager.persistentData.guidTextureList.Remove(currentGUIDTextureData);
                    if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

                    ReDrawFolders();
                }

            }

        }

        private static void ReDrawFolders()
        {
            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            AssetDatabase.Refresh();
            EditorApplication.RepaintProjectWindow();
        }
    }
}

