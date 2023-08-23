using UnityEditor;
using UnityEngine;

namespace UnityEditorTools.FolderIcons
{

    [CustomEditor(typeof(DefaultAsset))]
    internal class FolderAsset : Editor
    {

        internal static string currentPath;
        internal static string selectedAssetGUID;
        
        private void OnEnable()
        {
            currentPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            selectedAssetGUID = AssetDatabase.AssetPathToGUID(currentPath);
        }


        // This is exist in base class
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        // Since we can't just only change inspector header icon, we need to make header from zero to hero öhmm, i mean zero to full
        // Fun Fact: there is no documentation about Protected Override void OnHeaderGUI, thanks unity, again
        protected override void OnHeaderGUI()
        {
            if (AssetDatabase.IsValidFolder(currentPath))
            {
                GUILayout.BeginHorizontal(HeaderFunctions.GetStyle("In BigTitle"));
                GUILayout.Space(38f);

                GUILayout.BeginVertical();
                GUILayout.Space(21f);
                GUILayout.BeginHorizontal();

                HeaderFunctions.DrawHeaderOpenButton(target.GetInstanceID(), IconManager.persistentData.headerContents.openButton,
                    HeaderFunctions.GetStyle("miniButton"));

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                HeaderFunctions.DrawFolderHeaderIcon(currentPath, selectedAssetGUID, IconManager.tempFolderIconDict, IconManager.persistentData.headerContents.headerIconGUIStyle,
                IconManager.persistentData.headerContents.folderPopupWindowContent);

                HeaderFunctions.DrawHeaderTitle(target, IconManager.persistentData.headerContents.headerStBuilder, "Default Asset");
                HeaderFunctions.DrawHeaderThreeDot(IconManager.persistentData.headerContents.resetButtonGUIContent);
            }
        }

    }
}
