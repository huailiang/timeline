using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BoneFxBehaviour : XPlayableBehaviour
{
    PlayableBinding _bindPb, _currPb;
    ParticleSystem[] _particles;
    string _prefab;
    string _fx_path;
    Transform _fx_root;
    GameObject _fx_obj;
    Vector3 _pos, _rot, _scale;

    protected override void OnInitial()
    {
        base.OnInitial();
        BoneFxAsset bAsset = asset as BoneFxAsset;
        _prefab = bAsset.prefab;
        _pos = bAsset.pos;
        _rot = bAsset.rot;
        _scale = bAsset.scale;
        _fx_path = bAsset.fxPath;
    }



    public override void OnGraphStart(Playable playable) { }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (bindObj != null && _fx_obj == null)
        {
            _fx_root = bindObj.transform.Find(_fx_path);
            if (_fx_root != null)
            {
#if UNITY_EDITOR
                Transform tf = _fx_root.Find(_prefab+ "(Clone)");
                if (tf != null)
                {
                    if (Application.isPlaying)
                        GameObject.Destroy(tf.gameObject);
                    else
                        GameObject.DestroyImmediate(tf.gameObject);
                }
#endif
                var obj = Resources.Load<GameObject>(_prefab);
                _fx_obj = GameObject.Instantiate(obj) as GameObject;
                _particles = _fx_obj.GetComponentsInChildren<ParticleSystem>();
                _fx_obj.transform.parent = _fx_root;
                _fx_obj.transform.localPosition = _pos;
                _fx_obj.transform.localRotation = Quaternion.Euler(_rot);
                _fx_obj.transform.localScale = _scale;
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

}