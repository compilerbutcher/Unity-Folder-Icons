using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;



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

            if (GUI.changed)
            {
                IconManager.ExchangeIconSetData(IconManager.persistentData.iconSetDataList, IconManager.tempIconSetDict, DataExchangeType.ListToDict);
            }
        }


        #region Icon Set Region
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

                IconManager.LoadIconSetsFromJson(selectedFile);

            }

            IconManager.iconSetNames = new string[IconManager.persistentData.iconSetDataList.Count];

            for (int i = 0; i < IconManager.persistentData.iconSetDataList.Count; i++)
            {
                IconManager.iconSetNames[i] = IconManager.persistentData.iconSetDataList[i].iconSetName;
            }
        }
        private void SaveIconSetButton()
        {
            if (GUILayout.Button("Save Icon Sets!"))
            {
                string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", "", "Icon Set Data.json", "json");

                IconManager.SaveIconSetsFromJson(selectedFile);
             
            }
        }
        private static void LoadIconsButton()
        {
            if (GUILayout.Button("Load Icons!"))
            {
                string selectedFile = EditorUtility.OpenFilePanel("Select a .json file to load!", "", "json");

                IconManager.LoadIconsFromJson(selectedFile);
            }

        }
        private static void SaveIconsButton()
        {
            if (GUILayout.Button("Save Icons!"))
            {
                string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", "", "Folder Icons Data.json", "json");

                IconManager.SaveIconsToJson(selectedFile);
            }
        }
        private void ResetAllIconsButton()
        {

            if (GUILayout.Button("Reset All Icons!", GUILayout.ExpandWidth(false), GUILayout.Width(100)))
            {
                bool warningReset = EditorUtility.DisplayDialog("WARNING!", "This will delete all of current folder icon setup!", "Continue!", "Cancel!");

                if (warningReset)
                {
                    try
                    {
                        selectedOption = 0;
                        IconManager.persistentData.colorFolderNumber = 0;

                        IconManager.persistentData.guidTextureList.Clear();
                        EditorUtility.SetDirty(IconManager.persistentData);
                        IconManager.tempFolderIconDict.Clear();

                        EditorApplication.projectWindowItemOnGUI = null;
                        EditorApplication.RepaintProjectWindow();
                        Debug.Log("All Icons Are Reset!");
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }

                }

            }
        }
        #endregion



        [MenuItem("Tools/haha")]
        private static void Re()
        {
            selectedOption = 0;
            IconManager.persistentData.iconSetDataList.Clear();
            EditorUtility.SetDirty(IconManager.persistentData);
        }

        #region haa
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
            if (IconManager.persistentData.iconSetDataList[currentListIndex].iconSetData.Count == 0) return;

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
            EditorUtility.SetDirty(IconManager.persistentData);
            if (selectedOption == 0)
            {
                isIconSetADefaultIconSet = AreFirstAndSecondDefaultIconSet();

                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.RepaintProjectWindow();

                IconManager.tempFolderIconDict.Clear();
                UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);

                return;
            }
            isIconSetADefaultIconSet = AreFirstAndSecondDefaultIconSet();


            UpdateSerializedPropertyOfIconSetList(serializedObject, IconManager.persistentData.currentIconSetIndex);




            // Search for all assets (no filter)
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
            IconManager.tempFolderIconDict.Clear();
            for (int assetNameIndex = 0; assetNameIndex < assetNameList.Count; assetNameIndex++)
            {
                for (int iconSetIndex = 0; iconSetIndex < iconSetsProperty.FindPropertyRelative("iconSetData").arraySize; iconSetIndex++)
                {

                    if (assetNameList[assetNameIndex] == iconSetsProperty.FindPropertyRelative("iconSetData").GetArrayElementAtIndex(iconSetIndex).
                        FindPropertyRelative("folderName").stringValue)
                    {
                        if (!IconManager.tempFolderIconDict.ContainsKey(assetGUIDList[assetNameIndex]))
                        {
                            UtilityFunctions.CreateAndSaveDataToDict(assetGUIDList[assetNameIndex], IconManager.tempFolderIconDict, Color.clear,
                                null, null, iconSetsProperty.FindPropertyRelative("iconSetData").GetArrayElementAtIndex(iconSetIndex).
                                FindPropertyRelative("icon").objectReferenceValue as Texture2D);
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


