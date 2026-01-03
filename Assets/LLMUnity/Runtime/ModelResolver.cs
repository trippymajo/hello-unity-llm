using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LLMUnity
{
    /// <summary>
    /// Resolves LLM model paths based on a configuration file in StreamingAssets and caches the selection.
    /// </summary>
    public static class ModelResolver
    {
        const string ConfigFileName = "model-config.json";
        const string PlayerPrefsKey = "LLMUnity.ModelResolver.Paths";

        class ModelConfig
        {
            public string stupid;
            public string genious;
            public string classStudent;
            public string classTeacher;
            public List<string> goals;
        }

        [Serializable]
        class PersistedPaths
        {
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();
        }

        static readonly Dictionary<string, string> cachedPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static bool initialized = false;
        static ModelConfig config;
        static string firstModel = "";

        /// <summary>
        /// Applies the resolved model to the provided LLM instance.
        /// </summary>
        /// <param name="llm">LLM instance to configure.</param>
        public static void ApplyModel(LLM llm)
        {
            if (llm == null) return;
            if (!Application.isPlaying) return;
            if (llm.remote) return;

            Initialize();

            string key = ResolveKey(llm);
            string modelPath = ResolvePathForKey(key);
            if (string.IsNullOrEmpty(modelPath))
            {
                if (!string.IsNullOrEmpty(key)) LLMUnitySetup.LogWarning($"ModelResolver could not find a model for key '{key}'");
                return;
            }

            llm.SetModel(modelPath);
            LLMUnitySetup.Log($"ModelResolver applied model '{modelPath}' to '{llm.gameObject.name}'");
        }

        static void Initialize()
        {
            if (initialized) return;
            LoadCachedPaths();
            LoadConfig();
            firstModel = FindFirstModel();
            initialized = true;
        }

        static void LoadCachedPaths()
        {
            string pref = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (string.IsNullOrEmpty(pref)) return;

            try
            {
                PersistedPaths persisted = JsonUtility.FromJson<PersistedPaths>(pref);
                if (persisted?.keys == null || persisted.values == null) return;

                int count = Math.Min(persisted.keys.Count, persisted.values.Count);
                for (int i = 0; i < count; i++)
                {
                    string key = persisted.keys[i];
                    string value = persisted.values[i];
                    if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;
                    cachedPaths[key] = value;
                }
            }
            catch (Exception e)
            {
                LLMUnitySetup.LogWarning($"ModelResolver failed to load cached paths: {e.Message}");
            }
        }

        static void SaveCachedPaths()
        {
            PersistedPaths persisted = new PersistedPaths();
            foreach (KeyValuePair<string, string> pair in cachedPaths)
            {
                persisted.keys.Add(pair.Key);
                persisted.values.Add(pair.Value);
            }
            string json = JsonUtility.ToJson(persisted);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        static void LoadConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            if (!File.Exists(path))
            {
                LLMUnitySetup.LogWarning($"ModelResolver could not find config file at {path}");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                // Support a bare array by wrapping it.
                if (json.TrimStart().StartsWith("["))
                {
                    json = "{\"goals\":" + json + "}";
                }
                config = JsonUtility.FromJson<ModelConfig>(json);
            }
            catch (Exception e)
            {
                LLMUnitySetup.LogWarning($"ModelResolver failed to parse config: {e.Message}");
            }
        }

        static string ResolveKey(LLM llm)
        {
            if (!string.IsNullOrEmpty(llm.modelKey)) return llm.modelKey;

            string scene = SceneManager.GetActiveScene().name.ToLowerInvariant();
            string name = llm.gameObject.name.ToLowerInvariant();

            if (name.Contains("student")) return "classStudent";
            if (name.Contains("teacher")) return "classTeacher";
            if (name.Contains("stupid")) return scene.Contains("class") ? "classStudent" : "stupid";
            if (name.Contains("genious")) return "genious";

            if (scene.Contains("genious")) return "genious";
            if (scene.Contains("stupid")) return "stupid";
            if (scene.Contains("class")) return "classStudent";

            return "";
        }

        static string ResolvePathForKey(string key)
        {
            Initialize();

            if (!string.IsNullOrEmpty(key))
            {
                if (cachedPaths.TryGetValue(key, out string cachedPath) && File.Exists(ToFullPath(cachedPath)))
                {
                    return cachedPath;
                }

                string configured = GetConfiguredPath(key);
                string validated = ValidateRelativePath(configured);
                if (!string.IsNullOrEmpty(validated))
                {
                    cachedPaths[key] = validated;
                    SaveCachedPaths();
                    return validated;
                }
            }

            string fallback = GetFallbackModel();
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(fallback))
            {
                cachedPaths[key] = fallback;
                SaveCachedPaths();
            }
            return fallback;
        }

        static string GetConfiguredPath(string key)
        {
            if (config == null) return "";
            switch (key)
            {
                case "stupid":
                    return config.stupid;
                case "genious":
                    return config.genious;
                case "classStudent":
                    return !string.IsNullOrEmpty(config.classStudent) ? config.classStudent : config.stupid;
                case "classTeacher":
                    return !string.IsNullOrEmpty(config.classTeacher) ? config.classTeacher : config.genious;
                default:
                    break;
            }

            if (config.goals != null && config.goals.Count > 0)
            {
                return config.goals[0];
            }
            return "";
        }

        static string ValidateRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";
            string fullPath = ToFullPath(relativePath);
            if (File.Exists(fullPath)) return relativePath;

            LLMUnitySetup.LogWarning($"ModelResolver expected model at '{fullPath}' but it was not found.");
            return "";
        }

        static string GetFallbackModel()
        {
            if (!string.IsNullOrEmpty(firstModel)) return firstModel;
            LLMUnitySetup.LogWarning("ModelResolver could not locate any .gguf files in StreamingAssets to use as fallback.");
            return "";
        }

        static string FindFirstModel()
        {
            try
            {
                string basePath = Application.streamingAssetsPath;
                if (!Directory.Exists(basePath)) return "";
                string[] files = Directory.GetFiles(basePath, "*.gguf", SearchOption.AllDirectories);
                if (files.Length == 0) return "";
                return LLMUnitySetup.RelativePath(files[0], LLMUnitySetup.GetAssetPath());
            }
            catch (Exception e)
            {
                LLMUnitySetup.LogWarning($"ModelResolver failed while searching for fallback models: {e.Message}");
                return "";
            }
        }

        static string ToFullPath(string relativePath)
        {
            return LLMUnitySetup.GetAssetPath(relativePath);
        }
    }
}