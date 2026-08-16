#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GummyDynasty.Editor
{
    [InitializeOnLoad]
    public static class GummyDynastyBootstrap
    {
        const string MarkerPath = "Assets/_Project/Content/Settings/GummyDynasty.bootstrapped";
        const string UrpAssetPath = "Assets/_Project/Content/Settings/GummyDynastyURP.asset";
        const string RendererPath = "Assets/_Project/Content/Settings/GummyDynastyURP_Renderer.asset";
        const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";
        const string MainScenePath = "Assets/_Project/Scenes/Main.unity";

        static GummyDynastyBootstrap()
        {
            EditorApplication.delayCall += EnsureOnce;
        }

        [MenuItem("GummyDynasty/Bootstrap Project")]
        public static void EnsureOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (File.Exists(MarkerPath))
                return;

            EnsureFolders();
            var urp = EnsureUrp();
            EnsureScenes();
            EnsurePlayerSettings();
            File.WriteAllText(MarkerPath, "ok\n");
            AssetDatabase.Refresh();
            Debug.Log("GummyDynasty bootstrap complete. URP + Boot/Main scenes ready.");
            if (urp != null)
                GraphicsSettings.defaultRenderPipeline = urp;
        }

        static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory("Assets/_Project/Content/Settings");
        }

        static UniversalRenderPipelineAsset EnsureUrp()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (urp == null)
            {
                urp = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(urp, UrpAssetPath);
            }

            QualitySettings.renderPipeline = urp;
            GraphicsSettings.defaultRenderPipeline = urp;
            EditorUtility.SetDirty(urp);
            return urp;
        }

        static void EnsureScenes()
        {
            if (!File.Exists(BootScenePath))
            {
                var boot = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var bootGo = new GameObject("AppBoot", typeof(GummyDynasty.Core.AppBoot));
                Object.DontDestroyOnLoad(bootGo);
                EditorSceneManager.SaveScene(boot, BootScenePath);
            }

            if (!File.Exists(MainScenePath))
            {
                var main = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                var cam = Object.FindFirstObjectByType<Camera>();
                if (cam != null && cam.GetComponent<GummyDynasty.Presentation.MainCameraRig>() == null)
                    cam.gameObject.AddComponent<GummyDynasty.Presentation.MainCameraRig>();
                if (cam != null && cam.GetComponent<UniversalAdditionalCameraData>() == null)
                    cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

                var sim = new GameObject("SessionDirector", typeof(GummyDynasty.Simulation.SessionDirector));
                var arena = new GameObject("ToyArena", typeof(GummyDynasty.Simulation.ToyArena), typeof(GummyDynasty.Simulation.ToySandboxDirector));
                var hud = new GameObject("Hud", typeof(GummyDynasty.UI.HudController));
                var input = new GameObject("Input", typeof(GummyDynasty.Input.PlayerInputRouter), typeof(GummyDynasty.Input.ToyHostInput));
                if (cam != null)
                    cam.transform.position = new Vector3(0f, 8f, -14f);
                _ = sim;
                _ = arena;
                _ = hud;
                _ = input;
                EditorSceneManager.SaveScene(main, MainScenePath);
            }

            var bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
            var mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(MainScenePath, true)
            };
            _ = bootScene;
            _ = mainScene;

            if (File.Exists(BootScenePath))
                EditorSceneManager.OpenScene(BootScenePath);
        }

        static void EnsurePlayerSettings()
        {
            PlayerSettings.companyName = "Somapptic";
            PlayerSettings.productName = "GummyDynasty";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Unity_4_8);
            try
            {
                // 1 = Input System Package, 2 = Both
                var so = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null)
                {
                    prop.intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            catch
            {
                // First import may not have serialized ProjectSettings yet.
            }
        }
    }
}
#endif
