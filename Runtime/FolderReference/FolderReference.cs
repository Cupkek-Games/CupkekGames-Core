#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CupkekGames.Core
{
    [System.Serializable]
    public class FolderReference
    {
        public string GUID;
#if UNITY_EDITOR
        public string Path => AssetDatabase.GUIDToAssetPath(GUID);
#else
        public string Path => "FolderReference is editor only.";
#endif
    }
}