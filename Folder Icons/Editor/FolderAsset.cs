using UnityEditor;
using UnityEngine;

namespace UnityEditorTools.FolderIcons
{

    [CustomEditor(typeof(DefaultAsset))]
    internal sealed class FolderAsset : Editor
    {

        internal string currentPath;
        internal string selectedAssetGUID;
        
        private void OnEnable()
        {
            currentPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            selectedAssetGUID = AssetDatabase.AssetPathToGUID(currentPath);
        }


        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        // Since we can't just only change inspector header icon, we need to make header from scratch
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

                HeaderFunctions.DrawFolderHeaderIcon(currentPath, selectedAssetGUID, IconManager.persistentData.headerContents.headerIconGUIStyle);
                HeaderFunctions.DrawHeaderTitle(target);
                HeaderFunctions.DrawHeaderThreeDot(IconManager.persistentData.headerContents.resetButtonGUIContent);
            }
        }

    }
}
