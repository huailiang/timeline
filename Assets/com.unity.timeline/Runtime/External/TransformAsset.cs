using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TransformAsset : PlayableAsset
{


    [SerializeField] AnimationCurve[] m_Clip;
    [SerializeField] TrackAsset m_Parent;

    public AnimationCurve[] clip
    {
        get { return m_Clip; }
        set { m_Clip = value; }
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
        beha.Set(clip, go);
        return ScriptPlayable<TransformBehaviour>.Create(graph, beha);
    }

}