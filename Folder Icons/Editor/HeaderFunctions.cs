using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UnityEditorTools.FolderIcons
{
    internal static class HeaderFunctions
    {

        // This is mostly for drawing default folder header in the inspector, we need custom header for clicking effect on the icon and to have a customized
        // Icon in the header
        internal static void CreateInspectorHeaderContents(ref Texture2D buttonBackgroundTexture, ref Texture2D buttonHoverTexture,ref GUIStyle iconGUIStyle,
            ref GUIContent resetGUIContent, ref GUIContent openButton)
        {
            buttonBackgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DynamicConstants.defaultButtonPath);
            buttonHoverTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DynamicConstants.hoverButtonPath);


            iconGUIStyle = new GUIStyle();
            iconGUIStyle.normal.background = buttonBackgroundTexture;
            iconGUIStyle.hover.background = buttonHoverTexture;


            resetGUIContent = new GUIContent("Reset");

            openButton = EditorGUIUtility.TrTextContent("Open");
        }




        // Draw open button in the inspector header
        internal static void DrawHeaderOpenButton(int currentAssetInstanceId, GUIContent openButtonGUIContent, GUIStyle openButtonStyle)
        {
            GUILayoutUtility.GetRect(10f, 10f, 16f, 16f, EditorStyles.layerMaskField);
            GUILayout.FlexibleSpace();
            GUI.enabled = true;

            if (GUILayout.Button(openButtonGUIContent, openButtonStyle))
            {
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(currentAssetInstanceId)))
                {
                    AssetDatabase.OpenAsset(currentAssetInstanceId);
                    GUIUtility.ExitGUI();
                }
            }
        }

        // Draw folder icon in the inspector header as a clickable PopupWindow and with the custom icon we created
        internal static void DrawFolderHeaderIcon(string currentPath, string selectedAssetGUID, GUIStyle headerIconGUIStyle)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(lastRect.x + 0f, lastRect.y, lastRect.width - 0f, lastRect.height);
            Rect rect = new Rect(r.x + 6f, r.y + 6f, 32f, 32f);

            
            if (IconManager.persistentData.guidTextureList.Any(x => x.guid == selectedAssetGUID))
            {
                GUIDTextureData guidTextureData = IconManager.persistentData.guidTextureList.Find(x => x.guid == selectedAssetGUID);
                if (guidTextureData.textureData.folderTexture != null)
                {
                    IconManager.folderEmptyDict.TryGetValue(currentPath, out bool outputBool);
                    if (outputBool)
                    {
                        if (GUI.Button(rect, guidTextureData.textureData.folderTexture, headerIconGUIStyle))
                        {
                            FolderPopupWindowContent customFolderPopupWindow = new FolderPopupWindowContent();

                            PopupWindow.Show(rect, customFolderPopupWindow);
                        }
                    }
                    else
                    {
                        if (GUI.Button(rect, guidTextureData.textureData.emptyFolderTexture, headerIconGUIStyle))
                        {
                            FolderPopupWindowContent customFolderPopupWindow = new FolderPopupWindowContent();

                            PopupWindow.Show(rect, customFolderPopupWindow);
                        }
                    }
                }
                else if (guidTextureData.textureData.customTexture != null)
                {
                    if (GUI.Button(rect, guidTextureData.textureData.customTexture, headerIconGUIStyle))
                    {
                        FolderPopupWindowContent customFolderPopupWindow = new FolderPopupWindowContent();

                        PopupWindow.Show(rect, customFolderPopupWindow);
                    }
                }
            }
            else
            {

                IconManager.folderEmptyDict.TryGetValue(currentPath, out bool outputBool);
                if (outputBool)
                {
                    if (GUI.Button(rect, DynamicConstants.defaultFolderIcon, headerIconGUIStyle))
                    {
                        FolderPopupWindowContent customFolderPopupWindow = new FolderPopupWindowContent();

                        PopupWindow.Show(rect, customFolderPopupWindow);
                    }
                }
                else
                {
                    if (GUI.Button(rect, DynamicConstants.emptyDefaultFolderIcon, headerIconGUIStyle))
                    {
                        FolderPopupWindowContent customFolderPopupWindow = new FolderPopupWindowContent();

                        PopupWindow.Show(rect, customFolderPopupWindow);
                    }
                }
            }
        }


        // Draw inspector header title
        internal static void DrawHeaderTitle(Object target)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(lastRect.x + 0f, lastRect.y, lastRect.width - 0f, lastRect.height);

            Vector2 vector = EditorStyles.iconButton.CalcSize(GUIContent.none);
            float x = vector.x;
            bool enabled = GUI.enabled;
            GUI.enabled = true;
            GUI.enabled = enabled;
            x += vector.x;
            Rect rect2 = new Rect(r.xMax - x, r.y + 5f, vector.x, vector.y);

            float num = r.x + 44f;
            Rect rect3 = new Rect(num, r.y + 6f, rect2.x - num - 4f, 18f);

            rect3.yMin -= 2f;
            rect3.yMax += 2f;

            GUI.Label(rect3, $"{target.name} (Default Asset)", EditorStyles.largeLabel);
        }

        // Draw Three Dot in the header (Three little dot that opens a popup when clicking)
        internal static void DrawHeaderThreeDot(GUIContent resetButtonGUIContent)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(lastRect.x + 0f, lastRect.y, lastRect.width - 0f, lastRect.height);
            Vector2 vector = EditorStyles.iconButton.CalcSize(GUIContent.none);
            float x = vector.x;
            Rect position = new Rect(r.xMax - x, r.y + 5f, vector.x, vector.y);
            GUI.enabled = true;

            if (EditorGUI.DropdownButton(position, GUIContent.none, FocusType.Passive, GetStyle("PaneOptions")))
            {
                GenericMenu genericMenu = new GenericMenu();
                genericMenu.AddDisabledItem(resetButtonGUIContent);
                genericMenu.DropDown(position);
            }
        }

        // Stolen from unity editor codes, finds style if can't get built-in
        internal static GUIStyle GetStyle(string styleName)
        {
            return GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }
    }
}
