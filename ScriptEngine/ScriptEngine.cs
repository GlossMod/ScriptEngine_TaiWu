using System;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptEngine
{

    [PluginConfig("ScriptEngine", "小莫", "1.0.0")]
    public class ScriptEngine : TaiwuRemakePlugin
    {
        private static GameObject gameObject = null;

        public override void Dispose()
        {
            Object.Destroy(gameObject);
        }

        public override void Initialize()
        {
            //Harmony.CreateAndPatchAll(typeof(ScriptEngine), "aoe.taiwu.ScriptEngine");

            gameObject = ReloadPlugins.Create($"taiwu.ScriptEngine_{DateTime.Now.Ticks}");
            
        }

    }        
}