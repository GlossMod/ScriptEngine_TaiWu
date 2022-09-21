# 太吾绘卷脚本调试引擎

### 主要功能
太吾绘卷已经官方支持Mod, 我觉得美中不足的地方就是脚本调试不太方便, 所以我写了这个调试引擎, 用来调试脚本.

以方便Mod开发者来调试自己的脚本


### 使用方法
- 将 "ScriptEngine" 解压到游戏目录的 "mods"文件夹中
- 进入游戏后找到"模组选择"
- 启用"太吾绘卷 脚本调试器",然后应用
- 将你的脚本放入 "mods/Script" 文件夹中
- 按F6会自动重新加载文件夹中的脚本

### main
若要使用 ScriptEngine 来调试脚本,你需要在你的脚本中添加一个 `main` 类, 并继承 `MonoBehaviour` 类, 由于继承了 `MonoBehaviour` 类，所以 MonoBehaviour 的[所有事件函数](https://docs.unity3d.com/cn/2021.3/Manual/class-MonoBehaviour.html)都可以使用, 例如 `Start` `Update` `OnGUI` `OnDisable` 等等

### 例子

```csharp
using System;
using HarmonyLib;
using UnityEngine;

namespace ScriptTrainer
{
    public class main: MonoBehaviour
    {
        void Start()
        {
            // 在启动时做些事情
            Debug.Log("ScriptTrainer Start 启动");
        }

        void OnDisable()
        {
            // 脚本在卸载时做些事情
            Debug.Log("ScriptTrainer OnDisable 卸载");
        }

        void Update()
        {
            // 在每一帧做些事情
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("你按下了 F9 键");
            }
        }
    }
}

```

下面这个是游戏调用的方法
    
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace ScriptTrainer
{

    [PluginConfig("ScriptTrainer", "小莫", "1.0.0")]
    public class ScriptTrainer : TaiwuRemakePlugin
    {
        private static GameObject gameObject;

        public override void Dispose()
        {
            //// 销毁
            Object.Destroy(gameObject);
            Debug.Log("ScriptTrainer 销毁");
        }

        public override void Initialize()
        {
            // 加载时调用

            // 创建一个空物体
            gameObject = new GameObject($"taiwu.ScriptTrainer{DateTime.Now.Ticks}");

            // 将 main 类挂载到 gameObject 上 
            // 游戏会自动调用 Start、Update 方法
            gameObject.AddComponent<main>();
            Debug.Log("ScriptTrainer 初始化完成");
        }
    }
}
```

这里的 `public class ScriptTrainer : TaiwuRemakePlugin` 是作为Mod时, 游戏调用的类型, 这个是游戏要用的, 记得加上, 不然游戏不会读取你的脚本

而 `public class main: MonoBehaviour` 是调试引擎 用到的类，至于为什么要这么做, 你翻一翻源码就明白了，我本身是想模拟游戏加载Mod一样来调试脚本，但不管怎么折腾，都无法达到我想要的效果，所以只能这么做了,稍微麻烦了一点, 如果你有更好的方法的话, 欢迎推送你修改的代码

### 原理

- 创建一个空的 `GameObject`, 
- 使用 `AddComponent` 就可以将继承了 `MonoBehaviour` 的类挂载到 `GameObject`上
- 挂载之后, 游戏会帮你调用它(MonoBehaviour)的所有事件函数
- 这个方法适用于所有Unity游戏, 