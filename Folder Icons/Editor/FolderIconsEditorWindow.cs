using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace UnityEditorTools.FolderIcons
{
    public class FolderIconsEditorWindow : EditorWindow
    {
        private static int selectedOption = 0;
        private SerializedObject serializedObject;
        private SerializedProperty iconSetsProperty;

        private GUIStyle iconSettingsGUIStyle;
        private GUIStyle headerGUIStyle;

        private string newIconSetNameText;
        private bool isIconSetADefaultIconSet;
        private Vector2 scrollPosition;
        private int previousIconSetSize;

        [MenuItem("Tools/Folder Icon Settings")]
        static void FolderIconsWindow()
        {
            GetWindow<FolderIconsEditorWindow>();
        }

        private void OnEnable()
        {
            HandleOnEnable();
        }
      

        private void OnGUI()    
        {

            HandleOnEnable();

            if (IconManager.persistentData == null || serializedObject == null) return;
            if (IconManager.persistentData.iconSetDataList.Count > 0) serializedObject.Update();


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Icon Sets", headerGUIStyle, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            TextField();
            AddButton();
            EditorGUI.BeginDisabledGroup(isIconSetADefaultIconSet);
            RemoveButton();
            EditorGUI.EndDisabledGroup();
            DropdownButton();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
            IconSetLabelAndPropertField();


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Settings", headerGUIStyle, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Icon Set Settings", iconSettingsGUIStyle, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            LoadIconSetButton();
            SaveIconSetButton();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Icon Settings", iconSettingsGUIStyle, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            LoadIconsButton();
            SaveIconsButton();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            ResetAllIconsButton();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
            if (IconManager.persistentData.iconSetDataList.Count > 0) serializedObject.ApplyModifiedProperties();

            if (previousIconSetSize != IconManager.persistentData.currentIconSetIndex)
            {
                UpdateAllFolderIcons();
                previousIconSetSize = IconManager.persistentData.currentIconSetIndex;
            }

        }


        #region GUI Functions
        private void TextField()
        {
            newIconSetNameText = EditorGUILayout.TextField(newIconSetNameText);
        }
        private void AddButton()
        {
            if (GUILayout.Button("Add Icon Set"))
            {
                if (!IconManager.iconSetNames.Contains(newIconSetNameText) && newIconSetNameText != null && newIconSetNameText.Trim() != "")
                {
                    ArrayUtility.Add(ref IconManager.iconSetNames, newIconSetNameText);

                    IconSetDataListWrapper iconSetData = new IconSetDataListWrapper();

                    iconSetData.iconSetName = newIconSetNameText;
                    iconSetData.iconSetData = new List<IconSetData>();

                    IconManager.persistentData.iconSetDataList.Add(iconSetData);

                    if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

                    newIconSetNameText = "";

                    UpdateAllFolderIcons();

                    selectedOption = IconManager.persistentData.iconSetDataList.Count - 1;
                    UpdateSerializedPropertyOfIconSetList(serializedObject, selectedOption);

                    Repaint();

                    isIconSetADefaultIconSet = false;

                }
            }   
        }
        private void RemoveButton()
        {
            if (GUILayout.Button("Remove Icon Set"))
            {

                if (selectedOption != 0 && selectedOption != 1)
                {

                    ArrayUtility.Remove(ref IconManager.iconSetNames, IconManager.iconSetNames[selectedOption]);

                    IconManager.persistentData.iconSetDataList.RemoveAt(selectedOption);
                    IconManager.persistentData.currentIconSetIndex = 0;
                    EditorUtility.SetDirty(IconManager.persistentData);
                    UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);
                    selectedOption = 0;

                }
                else
                {
                    Debug.LogWarning("You can't delete default icon sets!");
                }
                isIconSetADefaultIconSet = true;

            }
        }
        private void DropdownButton()
        {
            EditorGUI.BeginChangeCheck();
            selectedOption = EditorGUILayout.Popup(selectedOption, IconManager.iconSetNames);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateAllFolderIcons();
            }
        }
        private void IconSetLabelAndPropertField()
        {
            EditorGUI.BeginDisabledGroup(isIconSetADefaultIconSet);
            serializedObject.Update();
            if (IconManager.persistentData.iconSetDataList.Count > 0)
                EditorGUILayout.PropertyField(iconSetsProperty);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
        }

        private void LoadIconSetButton()
        {
            if (GUILayout.Button("Load Icon Sets!"))
            {

                string selectedFile = EditorUtility.OpenFilePanel("Select a .json file to load!", "", "json");

                if (selectedFile.Length > 0)
                {
                    IconManager.LoadIconSetsFromJson(selectedFile);

                    IconManager.iconSetNames = new string[IconManager.persistentData.iconSetDataList.Count];

                    for (int i = 0; i < IconManager.persistentData.iconSetDataList.Count; i++)
                    {
                        IconManager.iconSetNames[i] = IconManager.persistentData.iconSetDataList[i].iconSetName;
                    }

                    Debug.Log($"Loaded icon sets from: {selectedFile}");
                }
                else
                {
                    Debug.LogWarning("You didn't selected a path!");
                }
            }
        }
        private void SaveIconSetButton()
        {
            if (GUILayout.Button("Save Icon Sets!"))
            {
                string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", "", "Icon Set Data.json", "json");

                if (selectedFile.Length > 0)
                {
                    IconManager.SaveIconSetsFromJson(selectedFile);

                    Debug.Log($"Saved icon sets to: {selectedFile}");
                }
                else
                {
                    Debug.LogWarning("You didn't selected a path!");
                }

            }
        }
        private static void LoadIconsButton()
        {
            if (GUILayout.Button("Load Icons!"))
            {
                string selectedFile = EditorUtility.OpenFilePanel("Select a .json file to load!", "", "json");

                if (selectedFile.Length > 0)
                {
                    IconManager.LoadIconsFromJson(selectedFile);

                    Debug.Log($"Loaded icons from: {selectedFile}");
                }
                else
                {
                    Debug.LogWarning("You didn't selected a path!");
                }
            }

        }
        private static void SaveIconsButton()
        {
            if (GUILayout.Button("Save Icons!"))
            {
                string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", "", "Folder Icons Data.json", "json");

                if (selectedFile.Length > 0)
                {
                    IconManager.SaveIconsToJson(selectedFile);

                    Debug.Log($"Saved icons to: {selectedFile}");
                }
                else
                {
                    Debug.LogWarning("You didn't select a path!");
                }
            }
        }
        private void ResetAllIconsButton()
        {

            if (GUILayout.Button("Reset All Icons!", GUILayout.ExpandWidth(false), GUILayout.Width(100)))
            {
                bool warningReset = EditorUtility.DisplayDialog("WARNING!", "THIS WILL DELETE ALL CURRENT ICONS/ICON SETS AND ALL LOADED" +
                    "ICON SETS/LOADED ICONS!", "Continue!", "Cancel!");

                if (warningReset)
                {
                    UtilityFunctions.CheckAndCreateFolderStorage();

                    try
                    {
                        selectedOption = 0;
                        IconManager.persistentData.currentIconSetIndex = 0;
                        if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);


                        string[] emptyFolderPaths = Directory.GetFiles(DynamicConstants.emptyIconFolderPath);
                        string[] folderPaths = Directory.GetFiles(DynamicConstants.iconFolderPath);

                        for (int i = 0; i < emptyFolderPaths.Length; i++)
                        {
                            string emptyPath = emptyFolderPaths[i];
                            string folderPath = folderPaths[i];

                            File.Delete(emptyPath);
                            File.Delete(folderPath);
                        }

                        string[] loadedIconSetPaths = Directory.GetDirectories(DynamicConstants.loadedIconSetPath);
                        string[] loadedIconPaths = Directory.GetFiles(DynamicConstants.loadedIconsPath);

                        for (int i = 0; i < loadedIconSetPaths.Length; i++)
                        {
                            string loadedIconSetPath = loadedIconSetPaths[i];

                            if (Directory.Exists(loadedIconSetPath))
                            {
                                Directory.Delete(loadedIconSetPath, true);
                                File.Delete($"{loadedIconSetPath}.meta");

                            }
                            else
                            {
                                Debug.Log($"Folder {loadedIconSetPath} does not exist.");
                            }
                        }

                        for (int i = 0; i < loadedIconPaths.Length; i++)
                        {
                            string loadedIconPath = loadedIconPaths[i];

                            File.Delete(loadedIconPath);
                        }

                        int iconSetDataIndex = IconManager.persistentData.iconSetDataList.Count - 2;

                        var iconSetList = IconManager.iconSetNames.ToList();
                        iconSetList.RemoveRange(2, iconSetDataIndex);
                        IconManager.iconSetNames = iconSetList.ToArray();

                        IconManager.persistentData.iconSetDataList.RemoveRange(2, iconSetDataIndex);


                        isIconSetADefaultIconSet = AreFirstAndSecondDefaultIconSet();
                        UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);


                        if (IconManager.persistentData != null) IconManager.persistentData.guidTextureList.Clear();
                        if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

                        AssetDatabase.Refresh();

                        EditorApplication.projectWindowItemOnGUI = null;
                        EditorApplication.RepaintProjectWindow();
                        Debug.Log("All icons have been reset!");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                }

            }
        }
        #endregion


        #region General Region
        private bool AreFirstAndSecondDefaultIconSet()
        {
            if (selectedOption == 0 || selectedOption == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void UpdateSerializedPropertyOfIconSetList(SerializedObject serializedObject, int currentListIndex)
        {

            if (IconManager.persistentData.iconSetDataList.Count == 0) return;
            //if (IconManager.persistentData.iconSetDataList[currentListIndex].iconSetData.Count == 0) return;

            serializedObject.Update();
            iconSetsProperty = serializedObject.FindProperty("iconSetDataList").GetArrayElementAtIndex(currentListIndex);
            SerializedProperty iconSetDataProperty = iconSetsProperty.FindPropertyRelative("iconSetData");

            iconSetDataProperty.ClearArray();

            // Get the array of icon set data from IconManager.persistentData
            var iconSetDataList = IconManager.persistentData.iconSetDataList[currentListIndex].iconSetData;

            for (int iconDataIndex = 0; iconDataIndex < iconSetDataList.Count; iconDataIndex++)
            {
                // Insert a new element to the iconSetsProperty array
                iconSetDataProperty.InsertArrayElementAtIndex(iconDataIndex);

                // Get the newly added element
                SerializedProperty newProperty = iconSetDataProperty.GetArrayElementAtIndex(iconDataIndex);
                // Set the properties of the newly added element using data from iconSetDataList
                newProperty.FindPropertyRelative("folderName").stringValue = iconSetDataList[iconDataIndex].folderName;
                newProperty.FindPropertyRelative("icon").objectReferenceValue = iconSetDataList[iconDataIndex].icon;
            }


            serializedObject.ApplyModifiedProperties();
        }
        private void HandleOnEnable()
        {
            if (IconManager.persistentData == null)
            {
                return;
            }
            else if (serializedObject == null)
            {
                serializedObject = new SerializedObject(IconManager.persistentData);


                if (IconManager.persistentData.iconSetDataList.Count > 0)
                {
                    int listIndex = IconManager.persistentData.currentIconSetIndex;
                    iconSetsProperty = serializedObject.FindProperty("iconSetDataList").GetArrayElementAtIndex(listIndex);
                }
                else
                {
                    iconSetsProperty = serializedObject.FindProperty("iconSetDataList");
                }



                selectedOption = IconManager.persistentData.currentIconSetIndex;

                isIconSetADefaultIconSet = AreFirstAndSecondDefaultIconSet();

                iconSettingsGUIStyle = new GUIStyle();
                iconSettingsGUIStyle.fontSize = 13;
                iconSettingsGUIStyle.fontStyle = FontStyle.Normal;
                iconSettingsGUIStyle.normal.textColor = Color.gray;

                headerGUIStyle = new GUIStyle();
                headerGUIStyle.fontSize = 25;
                headerGUIStyle.fontStyle = FontStyle.Bold;
                headerGUIStyle.normal.textColor = Color.white;


                if (IconManager.persistentData.iconSetDataList.Count == 0) return;


                UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);

                Repaint();
            }
        }
        private void UpdateAllFolderIcons()
        {
            if (IconManager.persistentData.iconSetDataList.Count == 0) return;

            IconManager.persistentData.currentIconSetIndex = selectedOption;
            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
            if (selectedOption == 0)
            {
                isIconSetADefaultIconSet = AreFirstAndSecondDefaultIconSet();

                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.RepaintProjectWindow();

                UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);

                return;
            }

            IconManager.persistentData.guidTextureList.Clear();
            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

            isIconSetADefaultIconSet = AreFirstAndSecondDefaultIconSet();

            UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);





            string[] guids = AssetDatabase.FindAssets("");
            List<string> assetGUIDList = new List<string>();
            List<string> assetNameList = new List<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (assetPath.StartsWith("Assets/") && !assetPath.StartsWith("Packages/"))
                {
                    assetGUIDList.Add(guids[i]);
                    assetNameList.Add(Path.GetFileNameWithoutExtension(assetPath));

                }
            }


            serializedObject.Update();
            for (int assetNameIndex = 0; assetNameIndex < assetNameList.Count; assetNameIndex++)
            {
                for (int iconSetIndex = 0; iconSetIndex < iconSetsProperty.FindPropertyRelative("iconSetData").arraySize; iconSetIndex++)
                {

                    if (assetNameList[assetNameIndex] == iconSetsProperty.FindPropertyRelative("iconSetData").GetArrayElementAtIndex(iconSetIndex).
                        FindPropertyRelative("folderName").stringValue)
                    {
                        if (IconManager.persistentData.guidTextureList.Any(x => x.guid != assetGUIDList[assetNameIndex]))
                        {
                            TextureData textureData = new TextureData();
                            textureData.color = Color.clear;
                            textureData.customTexture = iconSetsProperty.FindPropertyRelative("iconSetData").GetArrayElementAtIndex(iconSetIndex).
                                FindPropertyRelative("icon").objectReferenceValue as Texture2D;

                            GUIDTextureData guidTextureData = new GUIDTextureData();
                            guidTextureData.guid = assetGUIDList[assetNameIndex];
                            guidTextureData.textureData = textureData;

                            IconManager.persistentData.guidTextureList.Add(guidTextureData);

                            if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);
                        }    
                    }
                }
            }


            EditorApplication.projectWindowItemOnGUI = null;
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();


            IconManager.persistentData.currentIconSetIndex = selectedOption;
            EditorUtility.SetDirty(IconManager.persistentData);

        }
        #endregion


    }
}


