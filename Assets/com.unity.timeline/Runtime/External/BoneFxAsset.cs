using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BoneFxAsset : PlayableAsset
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
        beha.Set(dir, prefab, fxPath, pos, rot,scale);
        return ScriptPlayable<BoneFxBehaviour>.Create(graph, beha);
    }

}