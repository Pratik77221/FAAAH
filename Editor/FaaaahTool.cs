using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace FAAAH
{
    [InitializeOnLoad]
    public static class FaaaahTool
    {
        private const string PREFS_KEY = "FAAAH_Enabled";
        private const string AUDIO_FILENAME = "faaah";

        private static bool isEnabled;
        private static double nextPlayTime;
        private static AudioClip cachedAudioClip;
        private static bool shouldPlay;

        static FaaaahTool()
        {
            isEnabled = EditorPrefs.GetBool(PREFS_KEY, true);

            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked("Tools/FAAAH/Enabled", isEnabled);
            };
        }

        [MenuItem("Tools/FAAAH/Enabled")]
        public static void ToggleFAAAH()
        {
            isEnabled = !isEnabled;
            EditorPrefs.SetBool(PREFS_KEY, isEnabled);
            Menu.SetChecked("Tools/FAAAH/Enabled", isEnabled);

        }

        [MenuItem("Tools/FAAAH/Enabled", true)]
        public static bool ValidateToggleFAAAH()
        {
            Menu.SetChecked("Tools/FAAAH/Enabled", isEnabled);
            return true;
        }



        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!isEnabled) return;

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                shouldPlay = true;
            }
        }

        private static void EditorUpdate()
        {
            if (!shouldPlay) return;
            shouldPlay = false;

            double time = EditorApplication.timeSinceStartup;
            if (time < nextPlayTime) return;

            PlayAudio();

            if (cachedAudioClip != null)
            {
                nextPlayTime = time + cachedAudioClip.length;
            }
            else
            {
                nextPlayTime = time + 1.0;
            }
        }

        private static AudioClip FindAudioClip()
        {
            // Strategy 1: Search by asset name across entire project and packages
            string[] guids = AssetDatabase.FindAssets(AUDIO_FILENAME + " t:AudioClip");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null)
                {

                    return clip;
                }
            }

            // Strategy 2: Try known paths directly
            string[] knownPaths = new string[]
            {
                "Packages/com.faaah.erroraudio/Audio/faaah.mp3",
                "Assets/FAAAH/Audio/faaah.mp3",
                "Assets/Faaaah/Audio/faaah.mp3",
                "Packages/com.faaah.erroraudio/Audio/faaah.wav",
                "Packages/com.faaah.erroraudio/Audio/faaah.ogg",
            };

            foreach (string path in knownPaths)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null)
                {

                    return clip;
                }
            }

            Debug.LogWarning("[FAAAH] Could not find audio clip '" + AUDIO_FILENAME + "'. Make sure it exists in the Audio folder.");
            return null;
        }

        private static void PlayAudio()
        {
            if (cachedAudioClip == null)
            {
                cachedAudioClip = FindAudioClip();
            }

            if (cachedAudioClip == null) return;

            PlayClipInEditor(cachedAudioClip);
        }

        private static void PlayClipInEditor(AudioClip clip)
        {
            // Get the internal AudioUtil type
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilType = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilType == null)
            {
                Debug.LogWarning("[FAAAH] Could not find AudioUtil type.");
                return;
            }

            // Try all known method signatures across Unity versions
            // Unity 2020+: PlayPreviewClip(AudioClip, int, bool)
            // Unity 2019 and older: PlayClip(AudioClip, int, bool) or PlayClip(AudioClip)
            string[] methodNames = { "PlayPreviewClip", "PlayClip" };
            
            foreach (string methodName in methodNames)
            {
                // Try 3-parameter version first: (AudioClip, int startSample, bool loop)
                MethodInfo method = audioUtilType.GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                    null
                );

                if (method != null)
                {
                    try
                    {
                        method.Invoke(null, new object[] { clip, 0, false });
                        return;
                    }
                    catch (Exception) { }
                }

                // Try 1-parameter version: (AudioClip)
                method = audioUtilType.GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(AudioClip) },
                    null
                );

                if (method != null)
                {
                    try
                    {
                        method.Invoke(null, new object[] { clip });
                        return;
                    }
                    catch (Exception) { }
                }
            }

            Debug.LogWarning("[FAAAH] Could not find a compatible audio playback method for this Unity version.");
        }
    }
}
