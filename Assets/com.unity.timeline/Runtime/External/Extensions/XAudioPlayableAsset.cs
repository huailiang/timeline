using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Timeline
{
    partial class AudioPlayableAsset : IDirectorIO
    {

        public void Load(BinaryReader reader, XTrackAsset track)
        {
            string path = reader.ReadString();
            m_Clip = Resources.Load<AudioClip>(path);
            loop = reader.ReadBoolean();
            m_bufferingTime = reader.ReadSingle();
        }


        public void Write(BinaryWriter writer)
        {

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(m_Clip);
            path = path.Replace("Assets/Resources/", "");
            path = path.Substring(0, path.LastIndexOf('.'));
            
            writer.Write(path);
            writer.Write(loop);
            writer.Write(m_bufferingTime);
#endif
        }

    }

}
