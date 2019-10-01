# Unity2019 timeline 


### timeline支持自定义signal

![](/img/t1.jpg)

原生的timeline的signal事件，通过signal的在track上添加emmiter发射事件，然后在track的binding 的gameobject上添加signal receiver组件来接收事件。

对于静态的cutscene, 这样做非常方便。 然而有时候我们的需求是动态绑定，动态生成的。这时候signal receiver就不好序列化了（prefab的方式并不推荐，建议emmiter和receive都在内存里计算）。

timeline所有推送事件都是通过PlayableOutput.PushNotification 来发射的， 我们直接修改TimeNotificationBehaviour里的PushNotification， 直接调用我们自定义的receiver, 这里也省去了signal receiver的mono开销。



### 支持自定义序列化方式

使用.bytes文件代替timeline的.playable文件， 使用自己的序列化方式，更加方便版本升级。

我们自己实现的XDirectorAsset（继承PlayableAsset）和XTrackAsset（继承TrackAsset），然后赋值给director.timelineAsset和对应的sceneBingding, 如下图所示。 

XDirectorAsset和XTrackAsset使用我们自己的bytes文件填充，所以编辑好timeline之后需要记得save对应的bytes。 这个流程可以扩展一下timeline的编辑器很快的完成。


![](/img/t3.jpg)


### 预览Graph

强烈建议使用Unity官方的一个Plugin叫做graph-visualizer，来预览playable之间的结构关系，也是为了排错。比如说如果一个节点没有output， 对应的behavior的prepare是不会触发的。

注意： graph-visualizer 目前支持实时预览， 暂还不能编辑和debug。

![](/img/t2.jpg)