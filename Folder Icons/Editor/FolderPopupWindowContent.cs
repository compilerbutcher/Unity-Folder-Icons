using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditorTools.FolderIcons
{
    [Serializable]
    internal class FolderPopupWindowContent : PopupWindowContent
    {

        private string currentAssetPath;
        private static string currentAssetGUID;

        public override Vector2 GetWindowSize() => new Vector2(300, 65);
        public override void OnGUI(Rect rect)
        {
            if (IconManager.persistentData.currentIconSetIndex == 0)
                HandleOnGUI();
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Icon Sets Are Enabled!");
                GUILayout.EndHorizontal();
                GUILayout.Label("You Can Disable it from Tools > Dropdown Menu!");

            }
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
                if (File.Exists($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{currentAssetGUID}.png") &&
                    File.Exists($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{currentAssetGUID}.png"))
                {
                    File.Delete($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{currentAssetGUID}.png");
                    File.Delete($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{currentAssetGUID}.png");
                    File.Delete($"{Path.GetFullPath(DynamicConstants.emptyIconFolderPath)}\\{currentAssetGUID}.png.meta");
                    File.Delete($"{Path.GetFullPath(DynamicConstants.iconFolderPath)}\\{currentAssetGUID}.png.meta");
                    AssetDatabase.Refresh();
                }
                IconManager.persistentData.guidTextureList.RemoveAll(x => x.guid == currentAssetGUID);
                if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

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


            GUIDTextureData guidTextureData = IconManager.persistentData.guidTextureList.Find(x => x.guid == currentAssetGUID);

            if (guidTextureData.guid != null)
            {
                IconManager.projectCurrentColor = guidTextureData.textureData.color;
                IconManager.projectCurrentCustomTexture = guidTextureData.textureData.customTexture;
            }
        }
        private void HandleOnClose()
        {

            if (IconManager.projectCurrentEmptyFolderTexture != null)
            {
                PopupWindowContentFunctions.HandleColorFoldersTexture(currentAssetGUID);
            }
            else if (IconManager.projectCurrentFolderTexture != null)
            {
                PopupWindowContentFunctions.HandleColorFoldersTexture(currentAssetGUID);
            }
            if (IconManager.projectCurrentCustomTexture != null)
            {
                PopupWindowContentFunctions.HandleCustomTexture(currentAssetGUID);
            }


            IconManager.projectCurrentColor = Color.clear;
            IconManager.projectCurrentEmptyFolderTexture = null;
            IconManager.projectCurrentFolderTexture = null;
            IconManager.projectCurrentCustomTexture = null;

            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();

            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
        }






        internal static void DrawFolderColor(string guid, Rect selectionRect)
        {
            if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID))) return;

            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                IconManager.folderEmptyDict.TryGetValue(folderPath, out bool isFolderFilled);
                if (isFolderFilled)
                {
                    UtilityFunctions.DrawTextures(selectionRect, IconManager.projectCurrentFolderTexture);
                }
                else
                {
                    UtilityFunctions.DrawTextures(selectionRect, IconManager.projectCurrentEmptyFolderTexture);
                }

            }
            else
            {
                return;
            }
        }



        private static void PrivateCreateDefaultFolderWithColor(Color currentColor, ref Texture2D emptyFolderTexture, ref Texture2D defaultFolderTexture)
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                IconManager.folderEmptyDict.TryGetValue(folderPath, out bool isFolderFilled);
                if (isFolderFilled)
                {
                    defaultFolderTexture = new Texture2D(DynamicConstants.defaultFolderIcon.width, DynamicConstants.defaultFolderIcon.height);

                    for (int x = 0; x < DynamicConstants.defaultFolderIcon.width; x++)
                    {
                        for (int y = 0; y < DynamicConstants.defaultFolderIcon.height; y++)
                        {
                            // Set default folder
                            Color defaultOldColor = DynamicConstants.defaultFolderIcon.GetPixel(x, y);
                            Color defaultNewCol = currentColor;

                            defaultNewCol.a = defaultOldColor.a;
                            defaultFolderTexture.SetPixel(x, y, defaultNewCol);
                        }
                    }

                    defaultFolderTexture.Apply();

                }
                else
                {

                    emptyFolderTexture = new Texture2D(DynamicConstants.emptyDefaultFolderIcon.width, DynamicConstants.emptyDefaultFolderIcon.height);
                    for (int x = 0; x < DynamicConstants.defaultFolderIcon.width; x++)
                    {
                        for (int y = 0; y < DynamicConstants.defaultFolderIcon.height; y++)
                        {
                            // Set empty folder 
                            Color emptyOldColor = DynamicConstants.emptyDefaultFolderIcon.GetPixel(x, y);
                            Color emptyNewCol = currentColor;

                            emptyNewCol.a = emptyOldColor.a;
                            emptyFolderTexture.SetPixel(x, y, emptyNewCol);
                        }
                    }

                    emptyFolderTexture.Apply();
                }

            }
        }


        // For temp
        internal static void ChangeFolderColor()
        {

            PrivateCreateDefaultFolderWithColor(IconManager.projectCurrentColor, ref IconManager.projectCurrentEmptyFolderTexture,
                ref IconManager.projectCurrentFolderTexture);
            IconManager.projectCurrentCustomTexture = null;
            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += DrawFolderColor;
            EditorApplication.RepaintProjectWindow();
            //string selectedGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID));

            //UtilityFunctions.CreateAndSaveDataToDict(selectedGUID, IconManager.tempFolderIconDict, IconManager.projectCurrentColor,
            //    IconManager.projectCurrentEmptyFolderTexture, IconManager.projectCurrentFolderTexture, null);

            //int index = IconManager.persistentData.guidTextureList.FindIndex(x => x.guid == selectedGUID);
            //if (index != -1)
            //{
            //    GUIDTextureData guidTextureData = new GUIDTextureData();
            //    TextureData textureData = new TextureData();
            //    textureData.color = IconManager.projectCurrentColor;
            //    textureData.emptyFolderTexture = IconManager.projectCurrentEmptyFolderTexture;
            //    textureData.folderTexture = IconManager.projectCurrentFolderTexture;
            //    textureData.customTexture = IconManager.projectCurrentCustomTexture;

            //    guidTextureData = new GUIDTextureData();
            //    guidTextureData.guid = selectedGUID;
            //    guidTextureData.textureData = textureData;

            //    IconManager.persistentData.guidTextureList[index] = guidTextureData;
            //}
            //else
            //{
            //    GUIDTextureData guidTextureData = new GUIDTextureData();
            //    TextureData textureData = new TextureData();
            //    textureData.color = IconManager.projectCurrentColor;
            //    textureData.emptyFolderTexture = IconManager.projectCurrentEmptyFolderTexture;
            //    textureData.folderTexture = IconManager.projectCurrentFolderTexture;
            //    textureData.customTexture = IconManager.projectCurrentCustomTexture;

            //    guidTextureData = new GUIDTextureData();
            //    guidTextureData.guid = selectedGUID;
            //    guidTextureData.textureData = textureData;

            //    IconManager.persistentData.guidTextureList.Add(guidTextureData);
            //}



        }




        // For temp
        internal static void DrawFolderTexture(string guid, Rect selectionRect)
        {
            if (guid != AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeInstanceID))) return;
            UtilityFunctions.DrawTextures(selectionRect, IconManager.projectCurrentCustomTexture);
        }
        internal static void ChangeFolderTexture()
        {
            IconManager.projectCurrentFolderTexture = null;

            EditorApplication.projectWindowItemOnGUI += DrawFolderTexture;
            EditorApplication.RepaintProjectWindow();
        }

    }



}
