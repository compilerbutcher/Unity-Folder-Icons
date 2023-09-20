using UnityEditor;

namespace UnityEditorTools.FolderIcons
{
    public sealed class FolderPostprocess : AssetPostprocessor
    {
        // Handle updating dictionary that checks if a folder is empty or not. Handle applying icons to the newly created folders with icon sets
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (importedAssets.Length > 0)
            {
                for (int i = 0; i < importedAssets.Length; i++)
                {
                    try
                    {
                        string currentImportPath = importedAssets[i];

                        UtilityFunctions.UpdateFolderEmptyDict(currentImportPath, ref IconManager.folderEmptyDict);

                        UtilityFunctions.UpdateCurrentFolderIconWithIconSet(currentImportPath);

                        if (IconManager.folderEmptyDict.Count > 0)
                        {
                            UtilityFunctions.ReDrawFolders();
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

                        IconManager.persistentData.guidTextureList.RemoveAll(x => x.guid == AssetDatabase.AssetPathToGUID(currentImportPath));
                        EditorUtility.SetDirty(IconManager.persistentData);

                        UtilityFunctions.UpdateFolderEmptyDict(currentImportPath, ref IconManager.folderEmptyDict);

                        if (IconManager.folderEmptyDict.Count > 0)
                        {
                            UtilityFunctions.ReDrawFolders();
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


                    UtilityFunctions.UpdateFolderEmptyDict(movedAssetPath, ref IconManager.folderEmptyDict);
                    UtilityFunctions.UpdateCurrentFolderIconWithIconSet(movedAssetPath);
                    if (IconManager.folderEmptyDict.Count > 0)
                    {
                        UtilityFunctions.ReDrawFolders();
                    }

                    UtilityFunctions.UpdateFolderEmptyDict(movedFromAssetPath, ref IconManager.folderEmptyDict);

                    if (IconManager.folderEmptyDict.Count > 0)
                    {
                        UtilityFunctions.ReDrawFolders();
                    }
                }
            }
        }
    }
}