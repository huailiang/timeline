using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AnchorAsset : PlayableAsset, IDirectorIO
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

    public bool IsValid()
    {
        return m_Track != null &&
            m_Clip_Pos != null &&
            m_Clip_Rot != null;
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var director = owner.GetComponent<PlayableDirector>();
        Transform tf = ExternalHelp.FetchAttachOfTrack(director, track);
        AnchorBehaviour beha = new AnchorBehaviour();
        beha.Set(clip_pos, clip_rot, tf, director);
        return ScriptPlayable<AnchorBehaviour>.Create(graph, beha);
    }

    public void Load(BinaryReader reader)
    {
    }


    public void Write(BinaryWriter writer)
    {
    }

}