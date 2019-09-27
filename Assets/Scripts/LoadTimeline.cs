using System.IO;
using UnityEngine;
using UnityEngine.Playables;

public class LoadTimeline
{


    public void Load(string path, PlayableDirector director)
    {
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
        BinaryReader br = new BinaryReader(stream);
        XPlayableAsset asset = Load(br);
        director.playableAsset = asset;
        br.Close();
        stream.Close();
    }


    private XPlayableAsset Load(BinaryReader reader)
    {
        var asset = ScriptableObject.CreateInstance<XPlayableAsset>();
        var dur = reader.ReadDouble();
        asset.SetDuration(dur);
        return asset;
    }


}
