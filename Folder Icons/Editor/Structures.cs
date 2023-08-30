using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace UnityEditorTools.FolderIcons
{
    internal enum DataExchangeType
    {
        ListToDict,
        DictToList
    }


    [Serializable]
    internal struct TextureData
    {
        [SerializeField] internal Color color;
        [SerializeField] internal Texture2D emptyFolderTexture;
        [SerializeField] internal Texture2D folderTexture;

        [SerializeField] internal Texture2D customTexture;

        internal void Clear()
        {
            color = Color.clear;
            emptyFolderTexture = null;
            folderTexture = null;
            customTexture = null;
        }

    }


    [Serializable]
    internal struct GUIDTextureData
    {
        [SerializeField] internal string guid;
        [SerializeField] internal TextureData textureData;

        internal void Clear()
        {
            guid = "";
            textureData.Clear();
        }
    }






    [Serializable]
    internal class IconSetData
    {
        [SerializeField] internal string folderName;
        [SerializeField] internal Texture2D icon;
    }


    // This is for serializable list
    [Serializable]
    internal class IconSetDataListWrapper
    {
        [SerializeField] internal string iconSetName;
        [SerializeField] internal List<IconSetData> iconSetData;
    }



    [Serializable]
    internal class HeaderContents
    {
        // Unity inspector header variables
        [SerializeField] internal FolderPopupWindowContent folderPopupWindowContent;
        [SerializeField] internal Texture2D buttonBackgroundTexture;
        [SerializeField] internal Texture2D buttonHoverTexture;
        [SerializeField] internal GUIStyle headerIconGUIStyle;
        [SerializeField] internal StringBuilder headerStBuilder;
        [SerializeField] internal GenericMenu threeDotGenericMenu;
        [SerializeField] internal GUIContent resetButtonGUIContent;
        [SerializeField] internal GUIContent openButton;
    }


    [Serializable]
    internal struct JsonTextureData
    {
        [SerializeField] internal string folderName;
        [SerializeField] internal Vector4 color;

        [SerializeField] internal string emptyFolderTextureName;
        [SerializeField] internal string folderTextureName;
        [SerializeField] internal string customTextureName;

        [SerializeField] internal string emptyFolderTextureBase64;
        [SerializeField] internal string folderTextureBase64;
        [SerializeField] internal string customTextureBase64;

    }






    [Serializable]
    internal struct MainIconSetData
    {
        [SerializeField] internal string iconSetName;
        [SerializeField] internal List<Base64IconSetData> iconSetData;
    }

    [Serializable]
    internal struct Base64IconSetData
    {
        [SerializeField] internal string folderName;
        [SerializeField] internal string iconName;
        [SerializeField] internal string iconBase64;
    }


}




