using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditorTools.FolderIcons
{
    public static class Constants
    {
        // PersistentData path
        internal const string PersistentDataName = "/FolderIconsData.asset";
        internal const string defaultIconSetName = "/Default Icon Sets.json";


        // Main package path
        internal const string packageIconsPath = "Packages/com.codedestroyer.foldericons/Folder Icons";

        internal const string dataName = "/Data";
        internal const string iconsFolderName = "/Icons";
        internal const string defaultIconsName = "/Default Icons";
        internal const string colorFolderStorageName = "/Folder With Color Icons";
        internal const string loadedIconsFolderName = "/Loaded Icons";
        internal const string loadedIconSetsName = "/Loaded Icon Sets";
        internal const string iconSetsName = "/Icon Sets";


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
        // Dynamic Paths
        internal static string persistentDataPath;
        
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
        internal static string absolutePackagePath;
        internal static string folderStoragePath;
        internal static string defaultIconsPath;


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
            // Paths
            persistentDataPath = Constants.packageIconsPath + Constants.dataName + Constants.PersistentDataName;
            defaultIconsPath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.defaultIconsName;

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
            folderStoragePath = Constants.packageIconsPath + Constants.iconsFolderName + Constants.colorFolderStorageName;
            absolutePackagePath = Path.GetFullPath(Constants.packageIconsPath);



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
