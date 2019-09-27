using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class XTrackAsset : PlayableAsset
{
    static event Action<TimelineClip, GameObject, Playable> OnClipPlayableCreate;

    private DiscreteTime m_Start;
    private DiscreteTime m_End;
    private PlayableAsset m_Parent;

    public Playable playable;
    public PlayableOutput playableOutput;

    protected internal List<TimelineClip> m_Clips = new List<TimelineClip>();

    MarkerList m_Markers = new MarkerList(0);

    public sealed override double duration
    {
        get { return (double)(m_End - m_Start); }
    }

    public PlayableAsset parent
    {
        get { return m_Parent; }
        internal set { m_Parent = value; }
    }

    public virtual Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return Playable.Create(graph, inputCount);
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return Playable.Null;
    }

    protected virtual Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        var asset = clip.asset as IPlayableAsset;
        if (asset != null)
        {
            var handle = asset.CreatePlayable(graph, gameObject);
            if (handle.IsValid())
            {
                handle.SetAnimatedProperties(clip.curves);
                handle.SetSpeed(clip.timeScale);
                if (OnClipPlayableCreate != null)
                    OnClipPlayableCreate(clip, gameObject, handle);
            }
            return handle;
        }
        return Playable.Null;
    }

    internal virtual Playable OnCreateClipPlayableGraph(PlayableGraph graph, GameObject go,
            IntervalTree<RuntimeElement> tree)
    {
        int clipCount = m_Clips.Count;
        Playable blend = CreateTrackMixer(graph, go, clipCount);
        for (var c = 0; c < clipCount; c++)
        {
            Playable source = CreatePlayable(graph, go, m_Clips[c]);
            if (source.IsValid())
            {
                source.SetDuration(m_Clips[c].duration);
                var clip = new RuntimeClip(m_Clips[c], source, blend);
                tree.Add(clip);
                graph.Connect(source, 0, blend, c);
                blend.SetInputWeight(c, 0.0f);
            }
        }
        return blend;
    }


    public void Load(BinaryReader reader)
    {
        m_Start = (DiscreteTime)reader.ReadDouble();
        m_End = (DiscreteTime)reader.ReadDouble();
    }


    public void AddClip(TimelineClip newClip)
    {
        m_Clips.Add(newClip);
    }

    public void AddMarker(ScriptableObject e)
    {
        m_Markers.Add(e);
    }
    

}