using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UnityEditorTools.FolderIcons
{
    public class FolderPostprocess : AssetPostprocessor
    {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            for (int i = 0; i < importedAssets.Length; i++)
            {
                try
                {
                    string currentImportPath = importedAssets[i];
                    UtilityFunctions.UpdateFolderEmptyDict(currentImportPath, ref IconManager.folderEmptyDict);
                    if (IconManager.folderEmptyDict.Count > 0)
                    {
                        ReDrawFolders();
                    }
                }
                catch
                {
                }
            }

            if (deletedAssets.Length > 0)
            {
                try
                {
                    UtilityFunctions.UpdateFolderEmptyDict(deletedAssets[0], ref IconManager.folderEmptyDict);

                    if (IconManager.folderEmptyDict.Count > 0)
                    {
                        ReDrawFolders();
                    }
                }
                catch { }
            }

            if (movedAssets.Length > 0 && movedFromAssetPaths.Length > 0)
            {

                UtilityFunctions.UpdateFolderEmptyDict(movedAssets[0], ref IconManager.folderEmptyDict);
                UtilityFunctions.UpdateFolderEmptyDict(movedFromAssetPaths[0], ref IconManager.folderEmptyDict);

                if (IconManager.folderEmptyDict.Count > 0)
                {
                    ReDrawFolders();
                }
            }
        }
         void OnPreprocessAsset()
        {
            if (IconManager.persistentData == null) return;
            if (IconManager.persistentData.currentIconSetIndex == 0) return;
            if (IconManager.persistentData.iconSetDataList.Count == 0) return;


            if (assetImporter.userData != Path.GetFileNameWithoutExtension(assetImporter.assetPath))
            {
                
                if (IconManager.persistentData.iconSetDataList.Count > IconManager.persistentData.currentIconSetIndex)
                {

                    bool partialContain = IconManager.persistentData.iconSetDataList[IconManager.persistentData.currentIconSetIndex].
                        iconSetData.Any(word => word.folderName == Path.GetFileNameWithoutExtension(assetPath));
                    if (partialContain)
                    {
                        IconSetData iconSetData = IconManager.persistentData.iconSetDataList[IconManager.persistentData.currentIconSetIndex].
                            iconSetData.Find(word => word.folderName == Path.GetFileNameWithoutExtension(assetPath));


                        string currentGUID = AssetDatabase.AssetPathToGUID(assetPath);

                        UtilityFunctions.CreateAndSaveDataToDict(currentGUID, IconManager.tempFolderIconDict, Color.clear, null, null, iconSetData.icon);

                    }
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

