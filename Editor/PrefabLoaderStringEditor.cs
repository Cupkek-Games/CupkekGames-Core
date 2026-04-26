#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CupkekGames.Systems.Editor
{
    [CustomEditor(typeof(PrefabLoaderString), true)]
    public class PrefabLoaderStringEditor : PrefabLoaderEditor<string, GameObject>
    {
        public override string GetKeyFromFileName(string name)
        {
            return name;
        }
    }
}
#endif