using UnityEngine;
using UnityEngine.Playables;

public class BoneFxBehaviour : PlayableBehaviour
{
    PlayableDirector _director;
    PlayableBinding _bindPb, _currPb;
    ParticleSystem[] _particles;
    string _prefab;
    string _fx_path;
    Transform _target;
    Transform _fx_root;
    GameObject _fx_obj;
    Vector3 _pos;
    int _bindTrack;

    public void Set(PlayableDirector dir, string prefab, string fxpath, Vector3 pos)
    {
        _director = dir;
        _prefab = prefab;
        _pos = pos;
        var list = dir.playableAsset.outputs;
        int i = 0;
        foreach (var item in list)
        {
            if (i == _bindTrack)
            {
                _bindPb = item;
            }
            i++;
        }
        _fx_path = fxpath;

    }

    public override void OnGraphStart(Playable playable) { }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        var bind = _director.GetGenericBinding(_bindPb.sourceObject);
        if(bind is Animator)
        {
            _target = (bind as Animator).transform;
        }
        else if(bind is GameObject)
        {
            _target = (bind as GameObject).transform;
        }
        else if(bind is Animation)
        {
            _target = (bind as Animation).transform;
        }
        if (_target != null && _fx_obj == null)
        {
            _fx_root = _target.transform.Find(_fx_path);
            if (_fx_root != null)
            {
                string hash = "fx" + _prefab.GetHashCode().ToString();
#if UNITY_EDITOR
                Transform tf = _fx_root.Find(hash);
                if (tf != null)
                {
                    if (Application.isPlaying)
                        GameObject.Destroy(tf.gameObject);
                    else
                        GameObject.DestroyImmediate(tf.gameObject);
                }
#endif
                 //_fx_obj;
                _particles = _fx_obj.GetComponentsInChildren<ParticleSystem>();
                _fx_obj.transform.parent = _fx_root;
                _fx_obj.transform.localPosition = _pos;
                _fx_obj.transform.localRotation = Quaternion.identity;
                _fx_obj.transform.localScale = Vector3.one;
                _fx_obj.name = hash.ToString();
            }
        }
        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                _particles[i].Play();
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        ClearFx();
        base.OnBehaviourPause(playable, info);
    }

    public override void OnGraphStop(Playable playable)
    {
        ClearFx();
        base.OnGraphStop(playable);
    }

    private void ClearFx()
    {
        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] != null) _particles[i].Stop();
            }
        }
        if (_fx_obj != null)
        {
            _fx_obj = null;
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
    }

}