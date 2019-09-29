using UnityEngine;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BoneFxAsset : PlayableAsset, IDirectorIO
{

    [SerializeField] public string prefab;
    [SerializeField] public string fxPath;
    [SerializeField] public Vector3 pos, rot, scale;
    [SerializeField] TrackAsset m_Track;


    public TrackAsset track
    {
        get { return m_Track; }
        set { m_Track = value; }
    }


    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        PlayableDirector dir = graph.GetResolver() as PlayableDirector;
        BoneFxBehaviour beha = new BoneFxBehaviour();
        beha.Set(dir, prefab, fxPath, pos, rot, scale);
        return ScriptPlayable<BoneFxBehaviour>.Create(graph, beha);
    }


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