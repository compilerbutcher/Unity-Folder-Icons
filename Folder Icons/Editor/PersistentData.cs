using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEditorTools.FolderIcons
{

    // We need any type of persistent data that will be remain same between unity sessions Alternatively we can use PlayerPrefs, JsonUtility
    // but these would be horibble way to because we are using textures continually
    [Serializable]
    internal class PersistentData : ScriptableObject
    {
        // Main guidTexture Data
        [SerializeField, HideInInspector] internal List<GUIDTextureData> guidTextureList = new List<GUIDTextureData>();

        // Inspector header contents
        [SerializeField, HideInInspector] internal HeaderContents headerContents;

        // Icon sets
        [SerializeField] internal List<IconSetDataListWrapper> iconSetDataList = new();
        [SerializeField, HideInInspector] internal int currentIconSetIndex;

    }
}
