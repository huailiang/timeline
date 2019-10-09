using UnityEngine.Playables;

namespace UnityEngine.Timeline
{


    public delegate void NotifyDelegate(Playable origin, INotification notification);

    public interface IInterface
    {

        NotifyDelegate notify { get; set; }
        
    }
    
}
