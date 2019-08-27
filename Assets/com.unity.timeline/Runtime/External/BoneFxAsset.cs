using UnityEngine;
using UnityEngine.Playables;

public class BoneFxAsset : PlayableAsset
{

    public string prefab;
    public string fxPath;
    public int bindTrackIndex;
    public Vector3 pos;


    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        PlayableDirector dir = graph.GetResolver() as PlayableDirector;
        BoneFxBehaviour beha = new BoneFxBehaviour();
        beha.Set(dir, prefab, fxPath,pos);
        return ScriptPlayable<BoneFxBehaviour>.Create(graph, beha);
    }

}