using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEditorTools.FolderIcons
{
    // We need any type of persistent data that will be remain same between unity sessions
    // Alternatively we can use PlayerPrefs, JsonUtility
    [Serializable]
    internal class PersistentData : ScriptableObject
    {
        [SerializeField] internal int colorFolderNumber;
        [SerializeField] internal List<GUIDTextureData> guidTextureList = new List<GUIDTextureData>();

        // Inspector header contents
        [SerializeField, HideInInspector] internal HeaderContents headerContents;
        [SerializeField, HideInInspector] internal bool isHeaderContentsCreated;


        // Icon sets
        [SerializeField] internal List<IconSetDataListWrapper> iconSetDataList = new();
        [SerializeField] internal int currentIconSetIndex;

    }
}
