using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditorTools.FolderIcons
{
    public static class Constants
    {
        // Package paths from "Packages/com.compilerbutcher.foldericons/Folder Icons"
        internal const string packageIconsPath = "Packages/com.compilerbutcher.foldericons/Folder Icons";
        internal const string PersistentDataName = "/FolderIconsData.asset";
        internal const string defaultIconSetName = "/Default Icon Sets.json";
        internal const string dataFolderName = "/Data";
        internal const string defaultIconsName = "/Default Icons";
        internal const string iconSetsName = "/Icon Sets";
        internal const string iconsFolderName = "/Icons";
        // -----------------------------------------------------------------



        // Icon paths from "/Assets/Plugins/Unity-Folder-Icons"
        internal const string pluginsName = "Plugins";
        internal const string mainFolderIconName = "Unity-Folder-Icons";

        // Names for colorful folders
        internal const string emptyFolderIconsName = "Empty Folder Icons";
        internal const string folderIconsName = "Folder Icons";

        // Names for loaded icons from Tools > Folder Icon Settings
        internal const string loadedIconSetsName = "Loaded Icon Sets";
        internal const string loadedIconsFolderName = "Loaded Icons";
        // -----------------------------------------------------------------


        // Default icon names
        internal const string darkEmptyFolderName = "/d_DarkEmptyFolder Icon.png";
        internal const string darkFolderName = "/d_DarkFolder Icon.png";
        internal const string lightEmptyFolderName = "/d_LightEmptyFolder Icon.png";
        internal const string lightFolderName = "/d_LightFolder Icon.png";

        internal const string defaultButtonName = "/defaultbutton.png";
        internal const string hoverButtonName = "/hoverButtonName.png";






        // Button Default and Hover Color
        internal static readonly Color32 darkDefaultSkin = new Color32(60, 60, 60, 255);
        internal static readonly Color32 darkHoverSkin = new Color32(103, 103, 103, 255);
        internal static readonly Color32 lightDefaultSkin = new Color32(203, 203, 203, 255);
        internal static readonly Color32 lightHoverSkin = new Color32(239, 239, 239, 255);


        // Project Background Color
        internal static readonly Color32 darkBackgroundSkin = new Color32(51, 51, 51, 255);
        internal static readonly Color32 lightBackgroundSkin = new Color32(190, 190, 190, 255);


        // Folder Color
        internal static readonly Color32 darkFolderSkin = new Color32(194, 194, 194, 255);
        internal static readonly Color32 lightFolderSkin = new Color32(86, 86, 86, 255);

    }

    internal static class DynamicConstants
    {
        // Default unity icons path
        internal static string darkEmptyFolderPath;
        internal static string darkFolderPath;
        internal static string lightEmptyFolderPath;
        internal static string lightFolderPath;
        internal static string defaultButtonPath;
        internal static string hoverButtonPath;
        internal static string defaultButtonAbsolutePath;
        internal static string hoverButtonAbsolutePath;
        internal static string dynamicDefaultEmptyFolderPath;
        internal static string dynamicDefaultFolderPath;


        // Package paths from "Packages/com.compilerbutcher.foldericons/Folder Icons
        internal static string persistentDataPath;
        internal static string absolutePackagePath;
        internal static string defaultIconsPath;


        // Icon paths from "/Assets/Plugins/Unity-Folder-Icons"
        internal static string pluginsPath;
        internal static string mainFolderPath;
        internal static string emptyIconFolderPath;
        internal static string iconFolderPath;
        internal static string loadedIconSetPath;
        internal static string loadedIconsPath;


        // Dynamically updated dark theme or light theme variables (Pro skin or not)
        internal static Color32 buttonDefaultColor;
        internal static Color32 buttonHoverColor;
        internal static Color32 projectBackgroundColor;
        internal static Color32 folderColor;

        // Default folder path and textures
        internal static Texture2D emptyDefaultFolderIcon;
        internal static Texture2D defaultFolderIcon;

        internal static void UpdateDynamicConstants()
        {
            // Immutable package paths
            persistentDataPath = Constants.packageIconsPath + Constants.dataFolderName + Constants.PersistentDataName;
            absolutePackagePath = Path.GetFullPath(Constants.packageIconsPath);
            defaultIconsPath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.defaultIconsName;


            // Mutable paths
            pluginsPath = $"Assets/{Constants.pluginsName}";
            mainFolderPath = $"{pluginsPath}/{Constants.mainFolderIconName}";
            emptyIconFolderPath = $"{mainFolderPath}/{Constants.emptyFolderIconsName}";
            iconFolderPath = $"{mainFolderPath}/{Constants.folderIconsName}";
            loadedIconSetPath = $"{mainFolderPath}/{Constants.loadedIconSetsName}";
            loadedIconsPath = $"{mainFolderPath}/{Constants.loadedIconsFolderName}";




            // Immutable texture paths
            darkEmptyFolderPath = defaultIconsPath + Constants.darkEmptyFolderName;
            darkFolderPath = defaultIconsPath + Constants.darkFolderName;
            lightEmptyFolderPath = defaultIconsPath + Constants.lightEmptyFolderName;
            lightFolderPath = defaultIconsPath + Constants.lightFolderName;
            defaultButtonPath = defaultIconsPath + Constants.defaultButtonName;
            hoverButtonPath = defaultIconsPath + Constants.hoverButtonName;
            defaultButtonAbsolutePath = Path.GetFullPath(defaultButtonPath);
            hoverButtonAbsolutePath = Path.GetFullPath(hoverButtonPath);

            dynamicDefaultEmptyFolderPath = EditorGUIUtility.isProSkin ? darkEmptyFolderPath : lightEmptyFolderPath;
            dynamicDefaultFolderPath = EditorGUIUtility.isProSkin ? darkFolderPath : lightFolderPath;



            // Textures
            defaultFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(dynamicDefaultFolderPath);
            emptyDefaultFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(dynamicDefaultEmptyFolderPath);

            // Colors
            buttonDefaultColor = EditorGUIUtility.isProSkin ? Constants.darkDefaultSkin : Constants.lightDefaultSkin;
            buttonHoverColor = EditorGUIUtility.isProSkin ? Constants.darkHoverSkin : Constants.lightHoverSkin;

            projectBackgroundColor = EditorGUIUtility.isProSkin ? Constants.darkBackgroundSkin : Constants.lightBackgroundSkin;
            folderColor = EditorGUIUtility.isProSkin ? Constants.darkFolderSkin : Constants.lightFolderSkin;
        }

    }
}
