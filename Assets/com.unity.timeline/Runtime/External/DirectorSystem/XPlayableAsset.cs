using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    public class XPlayableBehaviour: PlayableBehaviour
    {
        protected PlayableAsset asset;

        protected GameObject bindObj;

        protected virtual void OnInitial() { }

        public void SetPlayableAsset(PlayableAsset asset, GameObject bind)
        {
            this.asset = asset;
            this.bindObj = bind;
            OnInitial();
        }
    }

    public class ClipPlayaleAsset : PlayableAsset
    {
        protected GameObject bindObj;

        public void SetBind(GameObject obj)
        {
            bindObj = obj;
        }

        public void SetBind(Transform tf)
        {
            if (tf)
            {
                bindObj = tf.gameObject;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return Playable.Null;
        }
    }


    public class XPlayableAsset<TBehaviour> : ClipPlayaleAsset
        where TBehaviour : XPlayableBehaviour, new()
    {

        protected TBehaviour behaviour;
        

        protected TBehaviour GetBehavior()
        {
            if (behaviour == null)
            {
                behaviour = new TBehaviour();
            }
            return behaviour as TBehaviour;
        }


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            if (behaviour == null)
            {
                behaviour = new TBehaviour();
                behaviour.SetPlayableAsset(this, bindObj);
            }
            return ScriptPlayable<TBehaviour>.Create(graph, behaviour);
        }


    }

}