using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UnityEngine.Timeline
{
    public partial class AnimationPlayableAsset : IDirectorIO
    {

        public void Load(BinaryReader reader, XTrackAsset track)
        {
            bool ext = reader.ReadBoolean();
            if (ext)
            {
                string path = reader.ReadString();

                m_Clip = Resources.Load<AnimationClip>(path);
                short mode = reader.ReadInt16();
                loop = (LoopMode)mode;

                applyFootIK = reader.ReadBoolean();
                
                if(track.isRecordMode)
                {
                    AnimationTrack atrack = track as AnimationTrack;
                    atrack.infiniteClip = clip;
                }
            }
        }


        public void Write(BinaryWriter writer)
        {
#if UNITY_EDITOR
            bool ext = m_Clip != null;
            writer.Write(ext);
            if (ext)
            {
                string path = AssetDatabase.GetAssetPath(m_Clip);
                path = path.Replace("Assets/Resources/", "").Replace(".anim", "");
                writer.Write(path);
                writer.Write((short)loop);
                writer.Write(applyFootIK);
            }
#endif
        }

        public static void Write(BinaryWriter writer, string path)
        {
            writer.Write(true);
            writer.Write(path);
            writer.Write((short)LoopMode.Off);
            writer.Write(false);
        }

        

    }

}