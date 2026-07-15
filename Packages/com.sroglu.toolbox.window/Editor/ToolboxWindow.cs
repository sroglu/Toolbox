using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace Sroglu.Toolbox.Window.Editor
{
    /// <summary>
    /// Editor window that lists every sroglu Toolbox tool and installs any of them
    /// with one click via the Unity Package Manager (git URL add).
    /// </summary>
    public class ToolboxWindow : EditorWindow
    {
        const string IndexUrl = "https://raw.githubusercontent.com/sroglu/Toolbox/main/toolbox-index.json";

        [Serializable]
        class ToolEntry
        {
            public string id;
            public string name;
            public string desc;
            public string url;
        }

        [Serializable]
        class ToolList
        {
            public ToolEntry[] tools;
        }

        ToolEntry[] _tools;
        readonly HashSet<string> _installedIds = new HashSet<string>();
        readonly HashSet<string> _localIds = new HashSet<string>();

        UnityWebRequest _indexRequest;
        UnityWebRequestAsyncOperation _indexOp;
        ListRequest _listRequest;
        AddRequest _addRequest;

        string _status = "Loading…";
        Vector2 _scroll;

        [MenuItem("Tools/Toolbox")]
        static void Open()
        {
            GetWindow<ToolboxWindow>("Toolbox");
        }

        void OnEnable()
        {
            LoadEverything();
        }

        void OnDisable()
        {
            EditorApplication.update -= PollIndex;
            EditorApplication.update -= PollList;
            EditorApplication.update -= PollAdd;
            DisposeIndexRequest();
        }

        void LoadEverything()
        {
            _status = "Loading tool list…";
            ScanLocalPackages();
            BeginFetchIndex();
            BeginRefreshInstalled();
        }

        // ---- Index (remote, with offline fallback) ----------------------------------

        void BeginFetchIndex()
        {
            DisposeIndexRequest();
            EditorApplication.update -= PollIndex;
            try
            {
                _indexRequest = UnityWebRequest.Get(IndexUrl);
                _indexOp = _indexRequest.SendWebRequest();
                EditorApplication.update += PollIndex;
            }
            catch (Exception)
            {
                UseFallbackTools();
            }
        }

        void PollIndex()
        {
            if (_indexRequest == null || _indexOp == null)
            {
                EditorApplication.update -= PollIndex;
                return;
            }

            if (!_indexOp.isDone)
                return;

            EditorApplication.update -= PollIndex;

            bool ok = string.IsNullOrEmpty(_indexRequest.error);
#if UNITY_2020_2_OR_NEWER
            ok = ok && _indexRequest.result == UnityWebRequest.Result.Success;
#endif
            if (ok)
            {
                string json = _indexRequest.downloadHandler != null ? _indexRequest.downloadHandler.text : null;
                if (!TryParseIndex(json))
                    UseFallbackTools();
            }
            else
            {
                UseFallbackTools();
            }

            DisposeIndexRequest();
            Repaint();
        }

        bool TryParseIndex(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;
            try
            {
                // The file is a bare JSON array; JsonUtility needs a wrapping object.
                ToolList list = JsonUtility.FromJson<ToolList>("{\"tools\":" + json + "}");
                if (list != null && list.tools != null && list.tools.Length > 0)
                {
                    _tools = list.tools;
                    _status = "Loaded " + _tools.Length + " tools.";
                    return true;
                }
            }
            catch (Exception)
            {
                // fall through to fallback
            }
            return false;
        }

        void DisposeIndexRequest()
        {
            if (_indexRequest != null)
            {
                _indexRequest.Dispose();
                _indexRequest = null;
            }
            _indexOp = null;
        }

        void UseFallbackTools()
        {
            _tools = EmbeddedTools();
            _status = "Loaded " + _tools.Length + " tools (offline list).";
            Repaint();
        }

        // ---- Installed packages (PackageManager) ------------------------------------

        void BeginRefreshInstalled()
        {
            EditorApplication.update -= PollList;
            _listRequest = Client.List(true); // offlineMode = true
            EditorApplication.update += PollList;
        }

        void PollList()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
                return;

            EditorApplication.update -= PollList;

            _installedIds.Clear();
            if (_listRequest.Status == StatusCode.Success && _listRequest.Result != null)
            {
                foreach (var p in _listRequest.Result)
                {
                    if (p != null && !string.IsNullOrEmpty(p.name))
                        _installedIds.Add(p.name);
                }
            }

            _listRequest = null;
            Repaint();
        }

        void ScanLocalPackages()
        {
            _localIds.Clear();
            try
            {
                string packagesDir = Path.Combine(Directory.GetCurrentDirectory(), "Packages");
                if (!Directory.Exists(packagesDir))
                    return;
                foreach (var dir in Directory.GetDirectories(packagesDir))
                {
                    string id = Path.GetFileName(dir);
                    if (!string.IsNullOrEmpty(id))
                        _localIds.Add(id);
                }
            }
            catch (Exception)
            {
                // ignore — local scan is best-effort
            }
        }

        bool IsInstalled(string id)
        {
            return !string.IsNullOrEmpty(id) && _installedIds.Contains(id);
        }

        bool IsLocal(string id)
        {
            return !string.IsNullOrEmpty(id) && _localIds.Contains(id) && !IsInstalled(id);
        }

        // ---- Install (Client.Add) ---------------------------------------------------

        void BeginInstall(string url)
        {
            if (string.IsNullOrEmpty(url) || _addRequest != null)
                return;

            EditorApplication.update -= PollAdd;
            _status = "Installing…";
            _addRequest = Client.Add(url);
            EditorApplication.update += PollAdd;
            Repaint();
        }

        void PollAdd()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
                return;

            EditorApplication.update -= PollAdd;

            if (_addRequest.Status == StatusCode.Success && _addRequest.Result != null)
                _status = "Installed " + _addRequest.Result.displayName + ".";
            else if (_addRequest.Error != null)
                _status = "Install failed: " + _addRequest.Error.message;
            else
                _status = "Install finished.";

            _addRequest = null;
            BeginRefreshInstalled();
            Repaint();
        }

        // ---- GUI --------------------------------------------------------------------

        void OnGUI()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("sroglu Toolbox", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(_addRequest != null))
                {
                    if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                        LoadEverything();
                }
            }

            EditorGUILayout.LabelField(_status ?? string.Empty, EditorStyles.miniLabel);
            EditorGUILayout.Space();

            if (_tools == null || _tools.Length == 0)
            {
                EditorGUILayout.HelpBox("No tools to show yet.", MessageType.Info);
                return;
            }

            var wrap = new GUIStyle(EditorStyles.label) { wordWrap = true };

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _tools.Length; i++)
            {
                ToolEntry t = _tools[i];
                if (t == null)
                    continue;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField(string.IsNullOrEmpty(t.name) ? t.id : t.name, EditorStyles.boldLabel);
                            if (!string.IsNullOrEmpty(t.desc))
                                EditorGUILayout.LabelField(t.desc, wrap);
                        }

                        GUILayout.FlexibleSpace();

                        bool installed = IsInstalled(t.id);
                        bool local = IsLocal(t.id);

                        if (installed)
                        {
                            using (new EditorGUI.DisabledScope(true))
                                GUILayout.Button("Installed", GUILayout.Width(90), GUILayout.Height(24));
                        }
                        else if (local)
                        {
                            using (new EditorGUI.DisabledScope(true))
                                GUILayout.Button("Local", GUILayout.Width(90), GUILayout.Height(24));
                        }
                        else
                        {
                            using (new EditorGUI.DisabledScope(_addRequest != null || string.IsNullOrEmpty(t.url)))
                            {
                                if (GUILayout.Button("Install", GUILayout.Width(90), GUILayout.Height(24)))
                                    BeginInstall(t.url);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        // ---- Offline fallback list ---------------------------------------------------

        static ToolEntry Make(string id, string name, string desc)
        {
            return new ToolEntry
            {
                id = id,
                name = name,
                desc = desc,
                url = "https://github.com/sroglu/Toolbox.git?path=/Packages/" + id + "#main"
            };
        }

        static ToolEntry[] EmbeddedTools()
        {
            return new[]
            {
                Make("com.sroglu.toolbox.objectpool", "Object Pool", "Generic object pool + GameObject prefab pool (zero deps)"),
                Make("com.sroglu.toolbox.collections", "Collections (PriorityQueue)", "Double-ended priority queue (min/max heap), pure C#"),
                Make("com.sroglu.toolbox.statemachine", "State Machine", "Lightweight FSM: IState + StateMachine + keyed StateMachine<TId>"),
                Make("com.sroglu.toolbox.grid", "Grid", "Generic 2D grid: indexing, bounds, 4/8 neighbors, cell/world"),
                Make("com.sroglu.toolbox.mvp", "MVP", "Clean Model-View-Presenter (passive view + presenter)"),
                Make("com.sroglu.toolbox.random", "Random Utils", "Uniform/weighted pick, shuffle, range, chance (pure C#)"),
                Make("com.sroglu.toolbox.events", "Event Bus", "Type-keyed publish/subscribe hub, re-entrancy safe (pure C#)"),
                Make("com.sroglu.toolbox.services", "Service Locator", "Type-keyed registry of shared service instances (pure C#)"),
                Make("com.sroglu.toolbox.assets", "Asset Registry", "Lightweight id-to-asset catalog + typed lookup/instantiate"),
                Make("com.sroglu.toolbox.spawner", "Spawner", "Id-based GameObject spawning with per-id pooling"),
                Make("com.sroglu.toolbox.screens", "Screen Manager", "Screen/page navigation with a back-stack (ScreenManager + IScreen)"),
                Make("com.sroglu.toolbox.input", "Input", "Lightweight reader over Unity's Input System — no .inputactions asset"),
                Make("com.sroglu.toolbox.pathfinding", "Path Finding", "Grid A* pathfinding (self-contained, 4/8-neighbor)"),
                Make("com.sroglu.toolbox.datastore", "Data Store", "Typed key-value store with JSON save/load")
            };
        }
    }
}
