using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using UnityEngine.Assertions.Must;

namespace UnityEditorTools.FolderIcons
{
    public class FolderIconsEditorWindow : EditorWindow
    {
        private static string[] iconSetNames;
        private int selectedOption = 0;
        private SerializedObject serializedObject;
        private SerializedProperty iconSetsProperty;
        [SerializeField] private List<IconSetData> iconSets;

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

            selectedOption = IconManager.persistentData.currentIconSetIndex;

            isIconSetADefaultIconSet = IsDefaultIconSet();

            iconSettingsGUIStyle = new GUIStyle();
            iconSettingsGUIStyle.fontSize = 13;
            iconSettingsGUIStyle.fontStyle = FontStyle.Normal;
            iconSettingsGUIStyle.normal.textColor = Color.gray;

            headerGUIStyle = new GUIStyle();
            headerGUIStyle.fontSize = 25;
            headerGUIStyle.fontStyle = FontStyle.Bold;
            headerGUIStyle.normal.textColor = Color.white;


            if (IconManager.persistentData.iconSetDataList.Count == 0) return;

            serializedObject = new SerializedObject(IconManager.persistentData);
            int listIndex = IconManager.persistentData.currentIconSetIndex;
            iconSetsProperty = serializedObject.FindProperty("iconSetDataList").GetArrayElementAtIndex(listIndex);



            iconSetNames = new string[IconManager.persistentData.iconSetDataList.Count];

            for (int i = 0; i < IconManager.persistentData.iconSetDataList.Count; i++)
            {
                iconSetNames[i] = IconManager.persistentData.iconSetDataList[i].iconSetName;
            }



            UpdateSerializedProperty(serializedObject, IconManager.persistentData.currentIconSetIndex);
        }
        private void OnGUI()    
        {
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
            LoadIconSets();
            SaveIconSets();
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
            LoadIcons();
            SaveIcons();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            ResetAllIcons();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
            if (IconManager.persistentData.iconSetDataList.Count > 0) serializedObject.ApplyModifiedProperties();

            if (iconSetsProperty.FindPropertyRelative("iconSetData").arraySize != previousIconSetSize)
            {
                UpdateIconSets();
                previousIconSetSize = iconSetsProperty.FindPropertyRelative("iconSetData").arraySize;
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
                if (!iconSetNames.Contains(newIconSetNameText) && newIconSetNameText != null && newIconSetNameText.Trim() != "")
                {
                    ArrayUtility.Add(ref iconSetNames, newIconSetNameText);
                    IconSetDataListWrapper iconSetData = new IconSetDataListWrapper();
                    iconSetData.iconSetName = newIconSetNameText;
                    iconSetData.iconSetData = new List<IconSetData>();
                    IconManager.persistentData.iconSetDataList.Add(iconSetData);
                    EditorUtility.SetDirty(IconManager.persistentData);
                    newIconSetNameText = "";
                    UpdateIconSets();
                    selectedOption = IconManager.persistentData.iconSetDataList.Count - 1;
                    UpdateSerializedProperty(serializedObject, selectedOption);
                    this.Repaint();

                    isIconSetADefaultIconSet = false;

                }
            }   
        }

        private void RemoveButton()
        {
            if (GUILayout.Button("Remove Icon Set"))
            {

                if (selectedOption != 0 && selectedOption != 1 && selectedOption != 2)
                {

                    ArrayUtility.Remove(ref iconSetNames, iconSetNames[selectedOption]);

                    IconManager.persistentData.iconSetDataList.RemoveAt(selectedOption);
                    IconManager.persistentData.currentIconSetIndex = 0;
                    EditorUtility.SetDirty(IconManager.persistentData);
                    UpdateSerializedProperty(serializedObject, IconManager.persistentData.currentIconSetIndex);
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
            selectedOption = EditorGUILayout.Popup(selectedOption, iconSetNames);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateIconSets();
            }
        }

        private void IconSetLabelAndPropertField()
        {
            EditorGUI.BeginDisabledGroup(isIconSetADefaultIconSet);
            serializedObject.Update();
            EditorGUILayout.PropertyField(iconSetsProperty);
            EditorGUI.EndDisabledGroup();
        }

        private void LoadIconSets()
        {
            if (GUILayout.Button("Load Icon Sets!"))
            {

                string selectedFile = EditorUtility.OpenFilePanel("Select a .json file to load!", "", "json");

                List<MainIconSetData> jsonDataList;
                JsonHelper.LoadJson<MainIconSetData>(selectedFile, out jsonDataList);


                for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
                {
                    MainIconSetData data = jsonDataList[loadedJsonDataIndex];

                    IconSetDataListWrapper newIconSetDataListWrapper = new IconSetDataListWrapper();
                    newIconSetDataListWrapper.iconSetData = new List<IconSetData>();
                    newIconSetDataListWrapper.iconSetName = data.iconSetName;

                    IconSetData newIconSetData;

                    for (int iconSetIndex = 0; iconSetIndex < data.iconSetData.Count; iconSetIndex++)
                    {
                        newIconSetData = new();
                        string folderName = data.iconSetData[iconSetIndex].folderName;
                        string iconName = data.iconSetData[iconSetIndex].iconName;
                        string base64Texture = data.iconSetData[iconSetIndex].iconBase64;


                        string fullPackagePath = DynamicConstants.absolutePackagePath + Constants.iconsFolderName + Constants.loadedIconSetsName;
                        string unityRelativePackagePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconSetsName;

                        TextureFunctions.Base64ToTexture2D(base64Texture, fullPackagePath + $"/{iconName}.png");
                        TextureFunctions.ImportTexture(unityRelativePackagePath + $"/{iconName}.png");


                        newIconSetData.folderName = folderName;
                        newIconSetData.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(unityRelativePackagePath + $"/{iconName}.png");
                        newIconSetData.icon.name = iconName;

                        newIconSetDataListWrapper.iconSetData.Add(newIconSetData);
                    }


                    IconManager.persistentData.iconSetDataList.Add(newIconSetDataListWrapper);
                    if (IconManager.persistentData != null) EditorUtility.SetDirty(IconManager.persistentData);

                }

            }
        }
        private void SaveIconSets()
        {
            if (GUILayout.Button("Save Icon Sets!"))
            {
                string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", "", "Icon Set Data.json", "json");

                List<MainIconSetData> packedIconSetData = new List<MainIconSetData>();

                for (int i = 3; i < IconManager.tempIconSetDict.Count; i++)
                {
                    KeyValuePair<string, List<IconSetData>> keyValue = IconManager.tempIconSetDict.ElementAt(i);


                    MainIconSetData newMainIconSetData = new();
                    newMainIconSetData.iconSetName = keyValue.Key;
                    newMainIconSetData.iconSetData = new List<Base64IconSetData>();

                    for (int IconSetDataIndex = 0; IconSetDataIndex < keyValue.Value.Count; IconSetDataIndex++)
                    {
                        Base64IconSetData newIconSetData = new Base64IconSetData();
                        newIconSetData.folderName = keyValue.Value[IconSetDataIndex].folderName;
                        newIconSetData.iconName = keyValue.Value[IconSetDataIndex].icon.name;
                        newIconSetData.iconBase64 = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value[IconSetDataIndex].icon));

                        newMainIconSetData.iconSetData.Add(newIconSetData);
                    }
                    packedIconSetData.Add(newMainIconSetData);
                }

                JsonHelper.SaveJson<MainIconSetData>(selectedFile, packedIconSetData);
            }
        }




        private void ResetAllIcons()
        {

            if (GUILayout.Button("Reset All Icons!", GUILayout.ExpandWidth(false), GUILayout.Width(100)))
            {
                bool warningReset = EditorUtility.DisplayDialog("WARNING!", "This will delete all of current folder icon setup!", "Continue!", "Cancel!");

                if (warningReset)
                {
                    try
                    {
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










        // -------------------------------------------------------------------------------------------------------------------------------

        private static void LoadIcons()
        {
            if (GUILayout.Button("Load Icons!"))
            {
                string selectedFile = EditorUtility.OpenFilePanel("Select a .json file to load!", "", "json");

                string folderName = "";
                Color color;
                string emptyFolderBase64String;
                string folderTextureBase64String;
                string customTextureBase64String;

                List<JsonTextureData> jsonDataList;
                JsonHelper.LoadJson<JsonTextureData>(selectedFile, out jsonDataList);

                for (int loadedJsonDataIndex = 0; loadedJsonDataIndex < jsonDataList.Count; loadedJsonDataIndex++)
                {
                    JsonTextureData data = jsonDataList[loadedJsonDataIndex];


                    folderName = data.folderName;
                    color = new Color(data.color.x, data.color.y, data.color.z, data.color.w);


                    emptyFolderBase64String = data.emptyFolderTextureBase64;
                    folderTextureBase64String = data.folderTextureBase64;
                    customTextureBase64String = data.customTextureBase64;


                    string emptyFolderTextureFullPath = $"{DynamicConstants.absolutePackagePath}\\{Constants.iconsFolderName}\\{Constants.loadedIconsFolderName}\\{data.emptyFolderTextureName}.png";
                    string folderTextureFullPath = $"{DynamicConstants.absolutePackagePath}\\{Constants.iconsFolderName}\\{Constants.loadedIconsFolderName}\\{data.folderTextureName}.png";
                    string customFolderTextureFullPath = $"{DynamicConstants.absolutePackagePath}\\{Constants.iconsFolderName}\\{Constants.loadedIconsFolderName}\\{data.customTextureName}.png";

                    TextureFunctions.Base64ToTexture2D(emptyFolderBase64String, emptyFolderTextureFullPath);
                    TextureFunctions.Base64ToTexture2D(folderTextureBase64String, folderTextureFullPath);
                    TextureFunctions.Base64ToTexture2D(customTextureBase64String, customFolderTextureFullPath);

                    string emptyFolderTexturePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconsFolderName + $"/{data.emptyFolderTextureName}.png";
                    string folderTexturePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconsFolderName + $"/{data.folderTextureName}.png";
                    string customFolderTexturePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.loadedIconsFolderName + $"/{data.customTextureName}.png";

                    if (emptyFolderBase64String.Length > 0)
                        TextureFunctions.ImportTexture(emptyFolderTexturePath);
                    if (folderTextureBase64String.Length > 0)
                        TextureFunctions.ImportTexture(folderTexturePath);
                    if (customTextureBase64String.Length > 0)
                        TextureFunctions.ImportTexture(customFolderTexturePath);


                    string newlyCreatedFolderGUID = AssetDatabase.CreateFolder("Assets", data.folderName);

                    Color currentColor = new Color(data.color.x, data.color.y, data.color.z, data.color.w);
                    Texture2D emptyFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyFolderTexturePath);
                    Texture2D colorFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(folderTexturePath);
                    Texture2D customFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(customFolderTexturePath);
                    UtilityFunctions.CreateAndSaveDataToDict(newlyCreatedFolderGUID, IconManager.tempFolderIconDict, currentColor, emptyFolder, colorFolder, customFolder);
                }


                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
                EditorApplication.RepaintProjectWindow();
                UtilityFunctions.CheckAllFoldersCurrentEmptiness(ref IconManager.folderEmptyDict);
            }

        }
        private static void SaveIcons()
        {
            if (GUILayout.Button("Save Icons!"))
            {
                string selectedFile = EditorUtility.SaveFilePanel("Select a folder to save!", "", "Folder Icons Data.json", "json");


                List<JsonTextureData> packedJsonList = new List<JsonTextureData>();
                foreach (KeyValuePair<string, TextureData> keyValue in IconManager.tempFolderIconDict)
                {
                    string emptyFolderBase64String = "";
                    string folderTextureBase64String = "";
                    string customTextureBase64String = "";


                    // Guid
                    string folderName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(keyValue.Key));


                    if (keyValue.Value.emptyFolderTexture != null)
                    {
                        emptyFolderBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value.emptyFolderTexture));
                    }
                    if (keyValue.Value.folderTexture != null)
                    {
                        folderTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value.folderTexture));
                    }
                    if (keyValue.Value.customTexture != null)
                    {
                        customTextureBase64String = Convert.ToBase64String(ImageConversion.EncodeToPNG(keyValue.Value.customTexture));
                    }

                    JsonTextureData packedJsonData = new JsonTextureData();

                    if (keyValue.Value.emptyFolderTexture != null)
                    {
                        packedJsonData.emptyFolderTextureName = keyValue.Value.emptyFolderTexture.name;
                        packedJsonData.folderTextureName = keyValue.Value.folderTexture.name;
                    }
                    if (keyValue.Value.customTexture != null)
                        packedJsonData.customTextureName = keyValue.Value.customTexture.name;




                    packedJsonData.folderName = folderName;
                    packedJsonData.color = new Vector4(keyValue.Value.color.r, keyValue.Value.color.g, keyValue.Value.color.b, keyValue.Value.color.a);

                    packedJsonData.emptyFolderTextureBase64 = emptyFolderBase64String;
                    packedJsonData.folderTextureBase64 = folderTextureBase64String;
                    packedJsonData.customTextureBase64 = customTextureBase64String;

                    packedJsonList.Add(packedJsonData);
                }

                JsonHelper.SaveJson<JsonTextureData>(selectedFile, packedJsonList);
                AssetDatabase.Refresh();

            }
        }
        // -------------------------------------------------------------------------------------------------------------------------------




        private void UpdateSerializedProperty(SerializedObject serializedObject, int currentListIndex)
        {


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


        private bool IsDefaultIconSet()
        {
            if (selectedOption == 0 || selectedOption == 1 || selectedOption == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateIconSets()
        {
            if (IconManager.persistentData.iconSetDataList.Count == 0) return;


            IconManager.persistentData.currentIconSetIndex = selectedOption;
            EditorUtility.SetDirty(IconManager.persistentData);
            if (selectedOption == 0)
            {
                isIconSetADefaultIconSet = IsDefaultIconSet();

                EditorApplication.projectWindowItemOnGUI = null;
                EditorApplication.RepaintProjectWindow();

                IconManager.tempFolderIconDict.Clear();
                UpdateSerializedProperty(serializedObject, IconManager.persistentData.currentIconSetIndex);

                return;
            }
            isIconSetADefaultIconSet = IsDefaultIconSet();


            UpdateSerializedProperty(serializedObject, IconManager.persistentData.currentIconSetIndex);




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
            EditorApplication.RepaintProjectWindow();
            EditorApplication.projectWindowItemOnGUI += UtilityFunctions.DrawFolders;
            EditorApplication.RepaintProjectWindow();


            IconManager.persistentData.currentIconSetIndex = selectedOption;
            EditorUtility.SetDirty(IconManager.persistentData);
        }



    }
}


