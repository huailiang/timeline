using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline.Signals
{
    class SignalManager : IDisposable
    {
        static SignalManager m_Instance;
        readonly List<SignalAsset> m_assets = new List<SignalAsset>();

        internal static SignalManager instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new SignalManager();
                    m_Instance.Refresh();
                }

                return m_Instance;
            }

            set { m_Instance = value; }
        }

        internal SignalManager()
        {
            SignalAsset.OnEnableCallback += Register;
        }

        public static IEnumerable<SignalAsset> assets
        {
            get
            {
                foreach (var asset in instance.m_assets)
                {
                    if (asset != null)
                        yield return asset;
                }
            }
        }

        public static SignalAsset CreateSignalAssetInstance(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            foreach (var signalAsset in assets)
            {
                if (signalAsset.name == name)
                    return signalAsset;
            }

            var newSignal = ScriptableObject.CreateInstance<SignalAsset>();
            newSignal.name = name;
            return newSignal;
        }

        public void Dispose()
        {
            SignalAsset.OnEnableCallback -= Register;
        }

        void Register(SignalAsset a)
        {
            m_assets.Add(a);
        }

        void Refresh()
        {
            var guids = AssetDatabase.FindAssets("t:SignalAsset");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<SignalAsset>(path);
                m_assets.Add(asset);
            }
        }
    }
}
