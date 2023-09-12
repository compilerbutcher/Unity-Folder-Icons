using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityEditorTools.FolderIcons
{

    [CustomEditor(typeof(DefaultAsset))]
    internal class FolderAsset : Editor
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

        // Since we can't just only change inspector header icon, we need to make header from zero to hero ohmm, i mean zero to full
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
                HeaderFunctions.DrawFolderHeaderIcon(currentPath, selectedAssetGUID, IconManager.persistentData.headerContents.headerIconGUIStyle,
                IconManager.persistentData.headerContents.folderPopupWindowContent);
                Profiler.BeginSample("Dict");

                HeaderFunctions.DrawHeaderTitle(target);
                Profiler.EndSample();

                HeaderFunctions.DrawHeaderThreeDot(IconManager.persistentData.headerContents.resetButtonGUIContent);
            }
        }

    }
}
