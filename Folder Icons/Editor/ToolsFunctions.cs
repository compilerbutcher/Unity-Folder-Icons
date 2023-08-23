using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;



namespace UnityEditorTools.FolderIcons
{

    public class ToolsFunctions : MonoBehaviour
    {

        [MenuItem("Tools/Clear")]
        static void ClearEverything()
        {

            //foreach(var i in Directory.GetFileSystemEntries(AssetDatabase.GetAssetPath(Selection.activeObject)))
            //{
            //    Debug.Log(i.Replace("\\", "/"));
            //}
            IconManager.persistentData.guidTextureList.Clear();
            IconManager.tempFolderIconDict.Clear();
            IconManager.folderEmptyDict.Clear();
            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
        }
        [MenuItem("Tools/Test")]
        static void Test()
        {
            Debug.Log(AssetDatabase.GUIDFromAssetPath("Packages").ToString());
            UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref IconManager.folderEmptyDict);
            //UtilityFunctions.UpdateFolderEmptyDict(assetName, ref IconManager.folderEmptyDict);

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.RepaintProjectWindow();
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();
            foreach (var i in IconManager.folderEmptyDict)
            {
                Debug.Log($"Key: {i.Key} Value: {i.Value}");
            }
        }
    }

}
