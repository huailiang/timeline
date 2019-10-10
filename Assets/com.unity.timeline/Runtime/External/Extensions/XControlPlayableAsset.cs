using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Timeline
{
    public partial class ControlPlayableAsset : IDirectorIO
    {

        public void Load(BinaryReader reader, XTrackAsset track)
        {
            string path = reader.ReadString();
            prefabGameObject = Resources.Load<GameObject>(path);
            active = reader.ReadBoolean();
            short pack = reader.ReadInt16();
            postPlayback = (ActivationControlPlayable.PostPlaybackState)pack;
            controllingDirectors = reader.ReadBoolean();
            particleRandomSeed = reader.ReadUInt32();
            updateITimeControl = reader.ReadBoolean();
            updateDirector = reader.ReadBoolean();
            updateParticle = reader.ReadBoolean();
        }


        public void Write(BinaryWriter writer)
        {
#if UNITY_EDITOR 
            string path = AssetDatabase.GetAssetPath(prefabGameObject);
            path = path.Replace("Assets/Resources/", "").Replace(".prefab", "");
            
            writer.Write(path);

            writer.Write(active);

            writer.Write((short)postPlayback);

            writer.Write(controllingDirectors);

            writer.Write(particleRandomSeed);

            writer.Write(updateITimeControl);

            writer.Write(updateDirector);

            writer.Write(updateParticle);
#endif
        }
    }

}
