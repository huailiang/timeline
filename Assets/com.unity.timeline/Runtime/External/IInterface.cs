using UnityEngine.Playables;

namespace UnityEngine.Timeline
{


    public delegate void NotifyDelegate(Playable origin, INotification notification);

    public interface IInterface
    {

        NotifyDelegate notify { get; set; }

        T Load<T>(string location) where T : Object;


       T Load<T>(string location, Transform parent, Vector3 pos, Quaternion rot) where T : Object;

    }
}
