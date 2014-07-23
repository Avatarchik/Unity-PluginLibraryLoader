/* PluginLibraryLoader
* 24-07-14
* https://github.com/SlateNeon/
*
*/
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PluginLibraryLoader : MonoBehaviour
{
    private static bool done = false;
    public static readonly Dictionary<string, PluginLibrary> loadedPlugins = new Dictionary<string, PluginLibrary>();

    static string dllPrefix = "PluginLibrary.";
    static string dllSuffix = ".dll";
    static string noloadFile = "noload";
    static string loadorderFile = "order";
    static string pluginFolder = "Plugins";
    static string entryClassName = "PluginEntryPoint";
    static string entryMethodName = "Init";
    static string entryInfoName = "info";

    public bool loadPlugins = true;
    private string pluginRootFolder = "";



    /// <summary> Gets relevant dlls and special files from path </summary>
    List<string> GetRelevantFiles(string rootPath) {
        List<string> files = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories).Where(x => {
            string s = Path.GetFileName(x);
            return s.StartsWith(dllPrefix) && s.EndsWith(dllSuffix);
        }).ToList();
        return files;
    }
    bool FolderContainsNoload(string rootPath) {
        return Directory.GetFiles(rootPath).Any(x => Path.GetFileName(x) == noloadFile);
    }

    #if UNITY_EDITOR
    public string editorPluginPath = Environment.CurrentDirectory + "/Build/build_Data/Plugins/";
    #endif
    /// <summary> Gets plugin root folder, returns "" if unsupported </summary>
    string GetPluginRootPath() {
        string path = "";

    #if UNITY_EDITOR
        path = editorPluginPath;
    #elif UNITY_STANDALONE
        path = Application.dataPath + "/" + pluginFolder + "/";
    #endif
        return path;
    }

    void LoadLibraries() {
        if (!loadPlugins || FolderContainsNoload(pluginRootFolder)) return;
        List<string> files = GetRelevantFiles(pluginRootFolder);
        if (files.Count == 0) return;

        files.ForEach(x => LoadLibrary(x));
        Debug.Log(string.Format("Loaded {0} plugin/s",loadedPlugins.Count));
    }

    void LoadLibrary(string path) {
        Assembly a = Assembly.LoadFile(path);
        if (a == null) return;
        loadedPlugins.Add(a.GetName().Name, new PluginLibrary(a));
    }

    void Awake() {
        //GetRelevantFiles(@"E:\Projects\Unity\PluginLibraryLoader\Build\build_Data\Plugins").ForEach(x=>Debug.Log(x));
        if (done) return;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        DontDestroyOnLoad(gameObject);
        pluginRootFolder = GetPluginRootPath();
        LoadLibraries();
        done = true;
        sw.Stop();
        Debug.Log(sw.Elapsed);
    }

    public class PluginLibrary {
        public Assembly plAssembly;
        public string plName = "??";
        public string plAbout = "??";
        public string plVersion = "??";
        public string plFileName;
        public string plFilePath;

        public PluginLibrary(Assembly a) {
            plAssembly = a;
            Prepare();
        }

        void Prepare() {
            Type t = GetEntryClass();
            GetData(t);
            CallEntryMethod(t);
        }

        Type GetEntryClass() {
            if (plAssembly == null) return null;
            return plAssembly.GetType(string.Format("{0}.{1}", plAssembly.GetName().Name, entryClassName), false);
        }

        void CallEntryMethod(Type entryClass) {
            if (entryClass == null) return;
            MethodInfo m = entryClass.GetMethod(entryMethodName); if (m == null) return;
            m.Invoke(null, null);
        }

        void GetData(Type entryClass) {
            if (entryClass == null) return;
            FieldInfo f = entryClass.GetField(entryInfoName);
            if (f == null) return;
            Dictionary<string, string> info = f.GetValue(null) as Dictionary<string,string>;
            info.TryGetValue("name", out plName);
            info.TryGetValue("about", out plAbout);
            info.TryGetValue("version", out plVersion);
            plFilePath = plAssembly.Location;
            plFileName = Path.GetFileName(plFilePath);
        }
    }

    #region GUI THINGS
    public KeyCode userInterfaceKey = KeyCode.F8;
    bool showUI = false;

    void Update() {
        if (Input.GetKeyDown(userInterfaceKey)) showUI = !showUI;
    }

    void OnGUI() {
        if (!showUI) return;
        GUI.BeginGroup(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
        //GUI.Box(new Rect(0, 0, Screen.width - 40, Screen.height - 40), "");
        int i = 0;
        foreach (PluginLibrary l in loadedPlugins.Values) {
            GUI.BeginGroup(new Rect(0, (50+5) * i, Screen.width - 20, 50));
                GUI.Box(new Rect(0, 0, Screen.width - 40, 50), "");
                GUI.Label(new Rect(5, 5, Screen.width - 40, 50), string.Format("name: {0} || version: {2} || about: {1}\n{3}",l.plName,l.plAbout,l.plVersion,l.plFilePath));
            GUI.EndGroup();
            i++;
        }
        GUI.EndGroup();
    }

    #endregion
}
