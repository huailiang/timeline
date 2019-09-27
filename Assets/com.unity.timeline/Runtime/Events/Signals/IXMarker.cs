namespace UnityEngine.Timeline
{

    public enum MarkType
    {
        SLOW = 0,
        JUMP,
        ANCHOR,
        ACTIVE,
        NONE = 100,
    }

    public interface IXMarker
    {
        MarkType markType { get; }
    }


}
