using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AnchorAsset : PlayableAsset
{
    
    [SerializeField] AnimationCurve[] m_Clip_Pos;
    [SerializeField] AnimationCurve[] m_Clip_Rot;
    [SerializeField] TrackAsset m_Track;

    public AnimationCurve[] clip_pos
    {
        get { return m_Clip_Pos; }
        set { m_Clip_Pos = value; }
    }


    public AnimationCurve[] clip_rot
    {
        get { return m_Clip_Rot; }
        set { m_Clip_Rot = value; }
    }

    public TrackAsset track
    {
        get { return m_Track; }
        set { m_Track = value; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var director = owner.GetComponent<PlayableDirector>();
        var binding = director.GetGenericBinding(track);
        GameObject go = null;
        if (binding is Animator)
        {
            go = (binding as Animator).gameObject;
        }
        else if (binding is Animation)
        {
            go = (binding as Animation).gameObject;
        }
        AnchorBehaviour beha = new AnchorBehaviour();
        beha.Set(clip_pos, clip_rot, go);
        return ScriptPlayable<AnchorBehaviour>.Create(graph, beha);
    }

}