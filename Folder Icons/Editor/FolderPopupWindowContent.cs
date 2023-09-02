using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEditorTools.FolderIcons
{
    [Serializable]
    internal class FolderPopupWindowContent : PopupWindowContent
    {

        protected string currentAssetPath;
        protected string currentAssetGUID;

        public override Vector2 GetWindowSize() => new Vector2(300, 65);
        public override void OnGUI(Rect rect)
        {
            HandleOnGUI();
        }

        public override void OnOpen()
        {
            HandleOnOpen();
        }

        public override void OnClose()
        {
            HandleOnClose();
        }







        private void HandleOnGUI()
        {
            // Folder Color Section
            GUILayout.BeginHorizontal();
            GUILayout.Label("Folder Color", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            IconManager.projectCurrentColor = EditorGUILayout.ColorField(IconManager.projectCurrentColor, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                // Alternatively we could just ignore change and apply changes in the on close of the popup window
                // But its worth to tradeoff because this way we can see folder color change immediately.
                ChangeFolderColor();
            }


            // Folder Texture Section
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom Texture", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            IconManager.projectCurrentCustomTexture = EditorGUILayout.ObjectField(IconManager.projectCurrentCustomTexture, typeof(Texture2D), false, GUILayout.ExpandWidth(false)) as Texture2D;
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {

                ChangeFolderTexture();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool removeButton = GUILayout.Button("Delete!", GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (removeButton)
            {
                IconManager.persistentData.guidTextureList.RemoveAll(x => x.guid == currentAssetGUID);
                EditorUtility.SetDirty(IconManager.persistentData);


                IconManager.tempFolderIconDict.Remove(currentAssetGUID);

                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();

                IconManager.projectCurrentFolderTexture = null;
                IconManager.projectCurrentEmptyFolderTexture = null;
                IconManager.projectCurrentCustomTexture = null;

                editorWindow.Close();
            }
        }

        private void HandleOnOpen()
        {
            currentAssetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            currentAssetGUID = AssetDatabase.AssetPathToGUID(currentAssetPath);
            

            if (IconManager.tempFolderIconDict.ContainsKey(currentAssetGUID))
            {
                TextureData textureData = IconManager.tempFolderIconDict[currentAssetGUID];

                IconManager.projectCurrentColor = textureData.color;
                IconManager.projectCurrentCustomTexture = textureData.customTexture;
            }
        }
        private void HandleOnClose()
        {

            if (IconManager.projectCurrentFolderTexture != null)
            {
                PopupWindowContentFunctions.HandleCreateAndDeleteFoldersOnClose(currentAssetGUID, IconManager.tempFolderIconDict);
            }
            else if (IconManager.projectCurrentCustomTexture != null)
            {
                PopupWindowContentFunctions.HandleCreateCustomTexture(currentAssetGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentCustomTexture);
            }


            IconManager.projectCurrentColor = Color.clear;
            IconManager.projectCurrentEmptyFolderTexture = null;
            IconManager.projectCurrentFolderTexture = null;
            IconManager.projectCurrentCustomTexture = null;

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();

            IconManager.ExchangeFolderIconData(IconManager.persistentData.guidTextureList, IconManager.tempFolderIconDict, DataExchangeType.DictToList);
            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
        }















        internal static void DrawFolderColor(string guid, Rect selectionRect)
        {
            if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID))) return;

            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                IconManager.folderEmptyDict.TryGetValue(folderPath, out bool outputBool);
                if (outputBool)
                    UtilityFunctions.DrawTextures(guid, selectionRect, IconManager.projectCurrentFolderTexture);
                else
                    UtilityFunctions.DrawTextures(guid, selectionRect, IconManager.projectCurrentEmptyFolderTexture);
            }
            else
            {
                return;
            }
        }


        // For temp
        internal static void ChangeFolderColor()
        {
            TextureFunctions.CreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture,
                ref IconManager.projectCurrentFolderTexture);
            IconManager.projectCurrentCustomTexture = null;

            //string selectedGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID));

            //UtilityFunctions.CreateAndSaveDataToDict(selectedGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentColor,
            //    IconManager.projectCurrentEmptyFolderTexture, IconManager.projectCurrentFolderTexture, null);

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += DrawFolderColor;
            EditorApplication.RepaintProjectWindow();
        }




        // For temp
        internal static void DrawFolderTexture(string guid, Rect selectionRect)
        {
            if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID))) return;
            UtilityFunctions.DrawTextures(guid, selectionRect, IconManager.projectCurrentCustomTexture);
        }
        internal static void ChangeFolderTexture()
        {
            IconManager.projectCurrentFolderTexture = null;

            EditorApplication.projectWindowItemOnGUI += DrawFolderTexture;
            EditorApplication.RepaintProjectWindow();
        }

    }



}