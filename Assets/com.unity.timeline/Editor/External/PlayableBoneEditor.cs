using UnityEditor;

namespace UnityEngine.Timeline
{

    [CustomEditor(typeof(BoneFxAsset))]

    public class PlayableBoneEditor : Editor
    {
        private GameObject boneGo;
        private GameObject prefabGo;
        private GameObject go;

        string GetRootFullPath(Transform tf)
        {
            string path = "";
            while (true)
            {
                if (tf.parent)
                {
                    if (!string.IsNullOrEmpty(path)) path = "/" + path;
                    path = tf.name + path;
                    tf = tf.parent;
                }
                else break;
            }
            return path;
        }


        public override void OnInspectorGUI()
        {
            BoneFxAsset asset = target as BoneFxAsset;
            EditorGUILayout.Space();
            prefabGo = EditorGUILayout.ObjectField("Fx Prefab", prefabGo, typeof(GameObject), true) as GameObject;
            if (prefabGo != null)
            {
                string path = AssetDatabase.GetAssetPath(prefabGo);
                asset.prefab = path.Replace("Assets/Resources/", string.Empty).Replace(".prefab", string.Empty);
                if (!path.EndsWith(".prefab"))
                {
                    EditorGUILayout.HelpBox("The fx that you selected is invalid", MessageType.Error);
                }
            }
            if (boneGo == null && !string.IsNullOrEmpty(asset.prefab))
            {
                prefabGo = Resources.Load<GameObject>(asset.prefab);
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
                if (string.IsNullOrEmpty(asset.fxPath))
                {
                    EditorGUILayout.HelpBox("Avatar bone transform is invalid", MessageType.Error);
                }
            }
            else if (!string.IsNullOrEmpty(asset.fxPath))
            {
                GameObject go = asset.GetBind();
                if (go)
                {
                    var tf = go.transform.GetChild(0).Find(asset.fxPath);
                    if (tf) boneGo = tf.gameObject;
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