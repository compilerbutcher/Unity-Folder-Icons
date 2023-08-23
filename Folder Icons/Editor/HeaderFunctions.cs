using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace UnityEditorTools.FolderIcons
{
    internal static class HeaderFunctions
    {

        // This is mostly for drawing default folder header in the inspector, we need custom header for clicking effect on the icon in the header
        internal static void CreateInspectorHeaderContents(ref FolderPopupWindowContent folderIconPopupContent,
            ref Texture2D buttonBackgroundTexture, ref Texture2D buttonHoverTexture,ref GUIStyle iconGUIStyle,
            ref GUIContent resetGUIContent, ref GUIContent openButton, Color buttonDefaultColor, Color buttonHoverColor)
        {
            folderIconPopupContent = new FolderPopupWindowContent();


            // Mini folder Icon in the inspector
            TextureFunctions.CreateTexture2DWithColor(ref buttonBackgroundTexture, buttonDefaultColor, 33, 33);
            TextureFunctions.CreateTexture2DWithColor(ref buttonHoverTexture, buttonHoverColor, 33, 33);

            TextureFunctions.CreateTexture(buttonBackgroundTexture, DynamicConstants.defaultButtonAbsolutePath);
            TextureFunctions.ImportTexture(DynamicConstants.defaultButtonPath);

            TextureFunctions.CreateTexture(buttonHoverTexture, DynamicConstants.hoverButtonAbsolutePath);
            TextureFunctions.ImportTexture(DynamicConstants.defaultButtonPath);

            buttonBackgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DynamicConstants.defaultButtonPath);
            buttonHoverTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DynamicConstants.hoverButtonPath);


            iconGUIStyle = new GUIStyle();
            iconGUIStyle.normal.background = buttonBackgroundTexture;
            iconGUIStyle.hover.background = buttonHoverTexture;
            // ------------------------------------------------------------------------------------------



            // Generic menu and reset button
            resetGUIContent = new GUIContent("Reset");
            // ------------------------------------------------------------------------------------------

            // Get open button GUIContent
            openButton = EditorGUIUtility.TrTextContent("Open");
            // ------------------------------------------------------------------------------------------
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

        // Draw folder icon in the inspector header as a clickable PopupWindow
        internal static void DrawFolderHeaderIcon(string currentPath, string selectedAssetGUID, Dictionary<string, TextureData> tempDict, GUIStyle headerIconGUIStyle, FolderPopupWindowContent folderIconPopup)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(lastRect.x + 0f, lastRect.y, lastRect.width - 0f, lastRect.height);
            Rect rect = new Rect(r.x + 6f, r.y + 6f, 32f, 32f);


            if (tempDict.ContainsKey(selectedAssetGUID))
            {
                
                if (tempDict[selectedAssetGUID].folderTexture != null)
                {
                    IconManager.folderEmptyDict.TryGetValue(currentPath, out bool outputBool);
                    if (outputBool)
                    {
                        if (GUI.Button(rect, tempDict[selectedAssetGUID].folderTexture, headerIconGUIStyle))
                        {
                            PopupWindow.Show(rect, folderIconPopup);
                        }
                    }
                    else
                    {
                        if (GUI.Button(rect, tempDict[selectedAssetGUID].emptyFolderTexture, headerIconGUIStyle))
                        {
                            PopupWindow.Show(rect, folderIconPopup);
                        }
                    }
                }
                else if (tempDict[selectedAssetGUID].customTexture != null)
                {
                    if (GUI.Button(rect, tempDict[selectedAssetGUID].customTexture, headerIconGUIStyle))
                    {
                        PopupWindow.Show(rect, folderIconPopup);
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
                        PopupWindow.Show(rect, folderIconPopup);
                    }
                }
                else
                {
                    if (GUI.Button(rect, DynamicConstants.emptyDefaultFolderIcon, headerIconGUIStyle))
                    {
                        PopupWindow.Show(rect, folderIconPopup);
                    }
                }
            }
        }


        // Draw inspector header title
        internal static void DrawHeaderTitle(Object target, StringBuilder headerStBuilder, string assetTypeName)
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
            GUI.Label(rect3, $"{target.name} ({assetTypeName})", EditorStyles.largeLabel);
        }

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

        internal static GUIStyle GetStyle(string styleName)
        {
            return GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }
    }
}
