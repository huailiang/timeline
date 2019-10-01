# Unity2019 timeline 




### timeline支持自定义signal

![](/img/t1.jpg)

原生的timeline的signal事件，通过signal的在track上添加emmiter发射事件，然后在track的binding 的gameobject上添加signal receiver组件来接收事件。

对于静态的cutscene, 这样做非常方便。 然而有时候我们的需求是动态绑定，动态生成的。这时候signal receiver就不好序列化了（prefab的方式并不推荐，建议emmiter和receive都在内存里计算）。

timeline所有推送事件都是通过PlayableOutput.PushNotification 来发射的， 修改TimeNotificationBehaviour里的PushNotification， 直接调用我们自定义的receiver, 这里也省去了signal receiver的mono开销。



### 支持自定义序列化方式

使用.bytes文件代替timeline的.playable文件， 使用自己的序列化方式，更加方便版本升级。编辑器时使用timeline自己的工具，方便编辑； 运行时走bytes流程，为了提高效率。

我们自己实现的XDirectorAsset（继承PlayableAsset）和XTrackAsset（继承TrackAsset），然后赋值给director.timelineAsset和对应的sceneBingding, 如下图所示。 


![](/img/t3.jpg)

XDirectorAsset和XTrackAsset使用我们自己的bytes文件填充，所以编辑好timeline之后需要记得save对应的bytes。 这个流程可以扩展一下timeline的编辑器很快的完成。


### 预览Graph

强烈建议使用Unity官方的一个Plugin叫做graph-visualizer，来预览playable之间的结构关系，也是为了排错。比如说如果一个节点没有output， 对应的behavior的prepare是不会触发的。

注意： graph-visualizer 目前只是支持实时预览， 暂不能edit和debug。

![](/img/t2.jpg)


### 其他

我们知道PlayableAsset和PlayableBehaviour都是成对出现的， 我们这里封装了一套架构，在实现自定义的playable的时候， behaviour能很方便的获取playableAsset里的变量, 也很方便的获取track bingding。

在实现自定义的Playable的实现，需要这样写:

```csharp
class BoneFxAsset : XPlayableAsset<BoneFxBehaviour>, IDirectorIO
{
		[SerializeField] public string prefab;

        public void Load(BinaryReader reader)
        {
        	// load from .bytes file
        	prefab = reader.ReadString();
        }


        public void Write(BinaryWriter writer)
        {
            // save to .bytes file
            writer.Write(prefab);
        }
 }
```

这样Behaviour获取Asset里的变量和Track binding Gameobject方法如下:

```csharp
public class BoneFxBehaviour : XPlayableBehaviour
{

    string _prefab;


    protected override void OnInitial()
    {
        base.OnInitial();
        BoneFxAsset bAsset = asset as BoneFxAsset;
        _prefab = bAsset.prefab; // fetch asset variable
        if (bindObj)  //fetch track bind
        {
            m_Transf = bindObj.transform;
        }
    }
}
```
