using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Mono.Cecil;

namespace ScriptEngine
{
    public class ReloadPlugins: MonoBehaviour
    {
        public static GameObject obj = null;
        private GameObject scriptManager;

        internal static GameObject Create(string name)
        {
            obj = new GameObject(name);
            DontDestroyOnLoad(obj);
            obj.AddComponent<ReloadPlugins>();
            return obj;
        }

        public void Start()
        {
            ReLoadMods();
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                ReLoadMods();

                #region[旧方法]

                // UnloadAllMods 卸载所有Mod
                //Traverse.Create(typeof(ModManager)).Method("UnloadAllMods").GetValue();

                //// _loadedPlugins 清空已加载的Mod列表 (上面的卸载方法中已经清空过了
                ////var _loadedPlugins = Traverse.Create(typeof(ModManager)).Field<Dictionary<ModId, List<TaiwuRemakePlugin>>>("_loadedPlugins").Value;
                ////Debug.Log("_loadedPlugins :" + _loadedPlugins.Count);

                //foreach (ModId modId in ModManager.EnabledMods)
                //{
                //    ModInfo modInfo;
                //    var _localMods = Traverse.Create(typeof(ModManager)).Field<Dictionary<string, ModInfo>>("_localMods").Value;
                //    if (_localMods.TryGetValue(modId.ToString(), out modInfo))
                //    {
                //        #region[经过测试,修改ID并不能实现 重新加载]
                //        // CreateTempModId  
                //        //modInfo.ModId = (ModId)Traverse.Create(typeof(ModManager)).Method("CreateTempModId", $"{modInfo.DirectoryName}_{DateTime.Now.Ticks}").GetValue();  // 重新生成一个ID
                //        ////modInfo.ModId = new ModId((ulong)DateTime.Now.Ticks, 1, 0);
                //        //Debug.Log($"modInfo.ModId : {modInfo.ModId}");
                //        #endregion
                //        for (int i = 0; i < modInfo.FrontendPlugins.Count; i++)
                //        {
                //            string modPath = ModManager.GetModPath(modInfo);
                //            string pluginDirPath = Path.Combine(modPath, "Plugins");                            
                //            string dllName = modInfo.FrontendPlugins[i];
                //            Debug.Log(dllName);
                //            // 将文件重命名
                //            string dllPath = Path.Combine(pluginDirPath, dllName);
                //            string randomStr = $"{DateTime.Now.Ticks}";
                //            File.Move(dllPath, $"{dllPath}.{randomStr}");
                //            modInfo.FrontendPlugins[i] = $"{dllName}.{randomStr}";
                //        }
                //        Traverse.Create(typeof(ModManager)).Method("LoadMod", modInfo).GetValue();
                //        Debug.Log($"加载 {modInfo.DirectoryName} 完毕");
                //    }
                //}
                //Debug.Log("_loadedPlugins 2:" + _loadedPlugins.Count);
                // 清空旧的mods
                //ModManager.EnabledMods.Clear();
                //ModManager.PlatformMods.Clear();
                //// ReadLocalMods 从本地加载Mod
                //Traverse.Create(typeof(ModManager)).Method("ReadLocalMods").GetValue();
                //// 重新读取Mod配置
                //ModManager.RestoreModConfig();
                //// 加载所有启用的Mod
                //ModManager.LoadAllEnabledMods();
                //UnloadMods();
                //LoadMods(mods);

                #endregion
            }
        }

        public void ReLoadMods()
        {
            Debug.Log("重新加载插件");
            Destroy(scriptManager);
            scriptManager = new GameObject($"ScriptEngine_scirpts_{DateTime.Now.Ticks}");
            DontDestroyOnLoad(scriptManager);

            Debug.Log($"scriptManager {scriptManager.name}");

            string scirpts = Path.Combine(ModManager.GetModRootFolder(), "scirpts");

            // 判断scirpts 文件夹是否存在
            if (!Directory.Exists(scirpts))
            {
                Debug.Log($"scirpts 文件夹不存在");
                return;
            }


            // 遍历 scirpts 文件夹里面的所有 *.dll 文件
            foreach (var item in Directory.GetFiles(scirpts, "*.dll"))
            {
                LoadMods(item);
            }
        }
        
        public void LoadMods(string path)
        {
            Debug.Log($"开始加载 {path}");

            var defaultResolver = new DefaultAssemblyResolver();
            defaultResolver.AddSearchDirectory(Path.Combine(ModManager.GetModRootFolder(), "scirpts"));
            
            var dll = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = defaultResolver });
            dll.Name.Name = $"{dll.Name.Name}-{DateTime.Now.Ticks}";

            Debug.Log($"dll.Name.Name : {dll.Name.Name}");

            var ms = new MemoryStream();
            dll.Write(ms);
            var ass = Assembly.Load(ms.ToArray());
            foreach (Type type in GetTypesSafe(ass))
            {
                // 判断类名是否是 main
                if (type.Name == "main")
                {
                    // 判断是否继承自 MonoBehaviour
                    if (type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        scriptManager.AddComponent(type);
                        Debug.Log($"加载 {path} 成功");
                    }
                }
            }
            dll.Dispose();
        }

        private IEnumerable<Type> GetTypesSafe(Assembly ass)
        {
            try
            {
                return ass.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sbMessage = new StringBuilder();
                sbMessage.AppendLine("\r\n-- 加载程序异常 --");
                foreach (var l in ex.LoaderExceptions)
                    sbMessage.AppendLine(l.ToString());
                sbMessage.AppendLine("\r\n-- 堆栈跟踪 --");
                sbMessage.AppendLine(ex.StackTrace);
                Debug.Log(sbMessage.ToString());
                return ex.Types.Where(x => x != null);
            }
        }
    }

}
