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

    protected virtual void OnCreateClip(TimelineClip clip) { }
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


    internal TimelineClip CreateAndAddNewClipOfType(Type requestedType)
    {
        var newClip = CreateClipOfType(requestedType);
        AddClip(newClip);
        return newClip;
    }

    internal TimelineClip CreateClipOfType(Type requestedType)
    {
        var playableAsset = CreateInstance(requestedType);
        if (playableAsset == null)
        {
            throw new System.InvalidOperationException("Could not create an instance of the ScriptableObject type " + requestedType.Name);
        }
        playableAsset.name = requestedType.Name;
        TimelineCreateUtilities.SaveAssetIntoObject(playableAsset, this);

        return CreateClipFromAsset(playableAsset);
    }

    private TimelineClip CreateClipFromAsset(ScriptableObject playableAsset)
    {
        var newClip = CreateNewClipContainerInternal();
        newClip.displayName = playableAsset.name;
        newClip.asset = playableAsset;

        IPlayableAsset iPlayableAsset = playableAsset as IPlayableAsset;
        if (iPlayableAsset != null)
        {
            var candidateDuration = iPlayableAsset.duration;

            if (!double.IsInfinity(candidateDuration) && candidateDuration > 0)
                newClip.duration = Math.Min(Math.Max(candidateDuration, TimelineClip.kMinDuration), TimelineClip.kMaxTimeValue);
        }

        try
        {
            OnCreateClip(newClip);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message, playableAsset);
            return null;
        }

        return newClip;
    }

    internal TimelineClip CreateNewClipContainerInternal()
    {
        var clipContainer = new TimelineClip(/*this*/null);
        clipContainer.asset = null;

        // position clip at end of sequence
        var newClipStart = 0.0;
        for (var a = 0; a < m_Clips.Count - 1; a++)
        {
            var clipDuration = m_Clips[a].duration;
            if (double.IsInfinity(clipDuration))
                clipDuration = TimelineClip.kDefaultClipDurationInSeconds;
            newClipStart = Math.Max(newClipStart, m_Clips[a].start + clipDuration);
        }

        clipContainer.mixInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        clipContainer.mixOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        return clipContainer;
    }

}