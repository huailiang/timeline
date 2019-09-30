using System.IO;

namespace UnityEngine.Timeline
{

    public class BoneFxAsset : XPlayableAsset<BoneFxBehaviour>, IDirectorIO
    {

        [SerializeField] public string prefab;
        [SerializeField] public string fxPath;
        [SerializeField] public Vector3 pos, rot, scale;
   


        public void Load(BinaryReader reader)
        {
            pos = reader.ReadVector3();
            rot = reader.ReadVector3();
            scale = reader.ReadVector3();
            prefab = reader.ReadString();
            fxPath = reader.ReadString();
        }


        public void Write(BinaryWriter writer)
        {
            writer.Write(pos);
            writer.Write(rot);
            writer.Write(scale);
            writer.Write(prefab);
            writer.Write(fxPath);
        }

    }
}