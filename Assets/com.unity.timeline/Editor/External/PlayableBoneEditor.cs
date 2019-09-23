using UnityEditor;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    [CustomEditor(typeof(BoneFxAsset))]

    public class PlayableBoneEditor : Editor
    {
        private GameObject boneGo;
        private GameObject prefabGo;
        private GameObject go;
        private PlayableDirector director;

        string GetRootFullPath(Transform tf)
        {
            string path = tf.name;
            while (tf.parent != null && tf.name != "root")
            {
                tf = tf.parent;
                path = tf.name + "/" + path;
            }
            return path;
        }



        public override void OnInspectorGUI()
        {
            BoneFxAsset asset = target as BoneFxAsset;
            if (director == null)
            {
                director = GameObject.FindObjectOfType<PlayableDirector>();
            }
            EditorGUILayout.Space();
            prefabGo = EditorGUILayout.ObjectField("Fx Prefab", prefabGo, typeof(GameObject), true) as GameObject;
            if (prefabGo != null)
            {
                string path = AssetDatabase.GetAssetPath(prefabGo);
                asset.prefab = path.Replace("Assets/BundleRes/", string.Empty).Replace(".prefab", string.Empty);
                if (!path.Contains("BundleRes") || !path.EndsWith(".prefab"))
                {
                    EditorGUILayout.HelpBox("The fx that you selected is invalid", MessageType.Error);
                }
            }
            if (boneGo == null && !string.IsNullOrEmpty(asset.prefab))
            {
                string path = "Assets/BundleRes/" + asset.prefab + ".prefab";
                prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabGo == null)
                {
                    EditorGUILayout.HelpBox("AssetPath is invalid", MessageType.Error);
                }
            }
            EditorGUILayout.LabelField(asset.prefab);
            boneGo = EditorGUILayout.ObjectField("Select Bone", boneGo, typeof(GameObject), true) as GameObject;
            EditorGUILayout.LabelField(asset.fxPath);
            if (boneGo != null)
            {
                asset.fxPath = GetRootFullPath(boneGo.transform);
                if (string.IsNullOrEmpty(asset.fxPath) || !asset.fxPath.StartsWith("root"))
                {
                    EditorGUILayout.HelpBox("Avatar bone transform is invalid", MessageType.Error);
                }
            }
            else if (!string.IsNullOrEmpty(asset.fxPath))
            {
                if (asset.track != null)
                {
                    var binding = director.GetGenericBinding(asset.track);
                    Transform tf = ExternalHelp.FetchAttachOfTrack(director, asset.track);
                    if (tf)
                    {
                        tf = tf.GetChild(0).Find(asset.fxPath);
                        if (tf) boneGo = tf.gameObject;
                    }
                }
            }
            DrawTransform();
            PreviewFx();
        }


        void PreviewFx()
        {
            if (prefabGo != null)
            {
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Texture2D icon128 = AssetPreview.GetAssetPreview(prefabGo);
                EditorGUILayout.ObjectField(icon128, typeof(Texture2D), false, GUILayout.Width(128 + 8), GUILayout.Height(128));
                GUILayout.Space(4);
                GUILayout.EndHorizontal();
            }
        }

        void DrawTransform()
        {
            BoneFxAsset asset = target as BoneFxAsset;
            EditorGUILayout.LabelField("Fx Transform:");
            asset.pos = EditorGUILayout.Vector3Field(" Position", asset.pos);
            asset.rot = EditorGUILayout.Vector3Field(" Rotation", asset.rot);
            asset.scale = EditorGUILayout.Vector3Field(" Scale", asset.scale);
        }

    }

}