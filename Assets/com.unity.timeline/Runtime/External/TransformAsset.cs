using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TransformAsset : PlayableAsset
{


    [SerializeField] AnimationCurve[] m_Clip_Pos;
    [SerializeField] AnimationCurve[] m_Clip_Rot;
    [SerializeField] TrackAsset m_Parent;

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

    public TrackAsset parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var director = owner.GetComponent<PlayableDirector>();
        var binding = director.GetGenericBinding(parent);
        GameObject go = null;
        if (binding is Animator)
        {
            go = (binding as Animator).gameObject;
        }
        else if (binding is Animation)
        {
            go = (binding as Animation).gameObject;
        }
        TransformBehaviour beha = new TransformBehaviour();
        beha.Set(clip_pos, clip_rot, go);
        return ScriptPlayable<TransformBehaviour>.Create(graph, beha);
    }

}