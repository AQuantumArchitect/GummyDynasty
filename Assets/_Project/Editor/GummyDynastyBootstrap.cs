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
            EditorApplication.delayCall += EnsureEditorPlayPath;
            EditorApplication.delayCall += EnsurePersonalities;
            EditorApplication.delayCall += EnsureCreatorAssets;
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
            Directory.CreateDirectory("Assets/_Project/Content/Resources/Personalities");
            Directory.CreateDirectory("Assets/_Project/Content/Resources/Projectiles");
            Directory.CreateDirectory("Assets/_Project/Content/Resources/Factions");
            Directory.CreateDirectory("Assets/_Project/Content/Resources/Units");
            Directory.CreateDirectory("Assets/_Project/Content/Resources/Objectives");
            Directory.CreateDirectory("Assets/_Project/Content/Resources/Modes");
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
                new GameObject("AppBoot", typeof(GummyDynasty.Core.AppBoot));
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

            EnsureEditorPlayPath();
        }

        [MenuItem("GummyDynasty/Create Personality")]
        public static void CreatePersonality()
        {
            SaveNew(
                "Personality",
                "NewPersonality",
                "Assets/_Project/Content/Resources/Personalities",
                () =>
                {
                    var p = GummyDynasty.Simulation.PhysicalPersonality.GummyPreset();
                    p.DisplayName = "NewPersonality";
                    return p;
                });
        }

        [MenuItem("GummyDynasty/Create Projectile")]
        public static void CreateProjectile()
        {
            SaveNew(
                "Projectile",
                "NewProjectile",
                "Assets/_Project/Content/Resources/Projectiles",
                GummyDynasty.Simulation.ProjectilePersonality.CandyPreset);
        }

        [MenuItem("GummyDynasty/Create Faction")]
        public static void CreateFaction()
        {
            SaveNew(
                "Faction",
                "NewFaction",
                "Assets/_Project/Content/Resources/Factions",
                () => ScriptableObject.CreateInstance<GummyDynasty.Simulation.FactionDefinition>());
        }

        [MenuItem("GummyDynasty/Create Objective")]
        public static void CreateObjective()
        {
            SaveNew(
                "Objective",
                "NewObjective",
                "Assets/_Project/Content/Resources/Objectives",
                () => ScriptableObject.CreateInstance<GummyDynasty.Simulation.ObjectiveDefinition>());
        }

        [MenuItem("GummyDynasty/Create Mode")]
        public static void CreateMode()
        {
            SaveNew(
                "Mode",
                "NewMode",
                "Assets/_Project/Content/Resources/Modes",
                () =>
                {
                    var m = ScriptableObject.CreateInstance<GummyDynasty.Simulation.GameModeDefinition>();
                    m.Objective = AssetDatabase.LoadAssetAtPath<GummyDynasty.Simulation.ObjectiveDefinition>(
                        "Assets/_Project/Content/Resources/Objectives/West.asset");
                    return m;
                });
        }

        [MenuItem("GummyDynasty/Create Unit")]
        public static void CreateUnit()
        {
            SaveNew(
                "Unit",
                "NewUnit",
                "Assets/_Project/Content/Resources/Units",
                () =>
                {
                    var u = ScriptableObject.CreateInstance<GummyDynasty.Simulation.GummyUnit>();
                    u.Personality = AssetDatabase.LoadAssetAtPath<GummyDynasty.Simulation.PhysicalPersonality>(
                        "Assets/_Project/Content/Resources/Personalities/Gummy.asset");
                    u.Faction = AssetDatabase.LoadAssetAtPath<GummyDynasty.Simulation.FactionDefinition>(
                        "Assets/_Project/Content/Resources/Factions/Red.asset");
                    return u;
                });
        }

        static void SaveNew(string title, string fileName, string folder, System.Func<ScriptableObject> make)
        {
            EnsureFolders();
            var path = EditorUtility.SaveFilePanelInProject(
                title,
                fileName,
                "asset",
                "Edit this and press Play. You do not need GummyFactory.",
                folder);
            if (string.IsNullOrEmpty(path))
                return;
            var asset = make();
            if (asset is GummyDynasty.Simulation.PhysicalPersonality p)
                p.DisplayName = Path.GetFileNameWithoutExtension(path);
            if (asset is GummyDynasty.Simulation.ProjectilePersonality proj)
                proj.DisplayName = Path.GetFileNameWithoutExtension(path);
            if (asset is GummyDynasty.Simulation.GummyUnit unit)
                unit.DisplayName = Path.GetFileNameWithoutExtension(path);
            if (asset is GummyDynasty.Simulation.ObjectiveDefinition obj)
                obj.DisplayName = Path.GetFileNameWithoutExtension(path);
            if (asset is GummyDynasty.Simulation.GameModeDefinition mode)
                mode.DisplayName = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);
            EditorGUIUtility.PingObject(asset);
        }

        static void EnsurePersonalities()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            EnsureFolders();
            WritePreset("Gummy", GummyDynasty.Simulation.PhysicalPersonality.GummyPreset);
            WritePreset("Knight", GummyDynasty.Simulation.PhysicalPersonality.KnightPreset);
            WritePreset("Scout", GummyDynasty.Simulation.PhysicalPersonality.ScoutPreset);
            WritePreset("Marcher", GummyDynasty.Simulation.PhysicalPersonality.MarcherPreset);
        }

        static void WritePreset(string name, System.Func<GummyDynasty.Simulation.PhysicalPersonality> make)
        {
            var path = "Assets/_Project/Content/Resources/Personalities/" + name + ".asset";
            if (File.Exists(path))
                return;
            var p = make();
            AssetDatabase.CreateAsset(p, path);
        }

        static void EnsureCreatorAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            EnsureFolders();
            WriteProjectile("Candy", GummyDynasty.Simulation.ProjectilePersonality.CandyPreset);
            WriteProjectile("Gumdrop", GummyDynasty.Simulation.ProjectilePersonality.GumdropPreset);
            WriteProjectile("Jawbreaker", GummyDynasty.Simulation.ProjectilePersonality.JawbreakerPreset);
            WriteFaction();
            WriteLevy();
            WriteWestMode();
        }

        static void WriteProjectile(string name, System.Func<GummyDynasty.Simulation.ProjectilePersonality> make)
        {
            var path = "Assets/_Project/Content/Resources/Projectiles/" + name + ".asset";
            if (File.Exists(path))
                return;
            AssetDatabase.CreateAsset(make(), path);
        }

        static void WriteFaction()
        {
            var path = "Assets/_Project/Content/Resources/Factions/Red.asset";
            if (File.Exists(path))
                return;
            var f = ScriptableObject.CreateInstance<GummyDynasty.Simulation.FactionDefinition>();
            f.DisplayName = "Red";
            AssetDatabase.CreateAsset(f, path);
        }

        static void WriteLevy()
        {
            var path = "Assets/_Project/Content/Resources/Units/Levy.asset";
            if (File.Exists(path))
                return;
            var u = ScriptableObject.CreateInstance<GummyDynasty.Simulation.GummyUnit>();
            u.DisplayName = "Levy";
            u.DefaultIntent = "idle";
            u.Personality = AssetDatabase.LoadAssetAtPath<GummyDynasty.Simulation.PhysicalPersonality>(
                "Assets/_Project/Content/Resources/Personalities/Gummy.asset");
            u.Faction = AssetDatabase.LoadAssetAtPath<GummyDynasty.Simulation.FactionDefinition>(
                "Assets/_Project/Content/Resources/Factions/Red.asset");
            AssetDatabase.CreateAsset(u, path);
        }

        static void WriteWestMode()
        {
            var objPath = "Assets/_Project/Content/Resources/Objectives/West.asset";
            if (!File.Exists(objPath))
            {
                var o = ScriptableObject.CreateInstance<GummyDynasty.Simulation.ObjectiveDefinition>();
                o.DisplayName = "WEST";
                o.Label = "WEST";
                AssetDatabase.CreateAsset(o, objPath);
            }

            var modePath = "Assets/_Project/Content/Resources/Modes/HoldWest.asset";
            if (File.Exists(modePath))
                return;
            var m = ScriptableObject.CreateInstance<GummyDynasty.Simulation.GameModeDefinition>();
            m.DisplayName = "Hold WEST";
            m.Victory = GummyDynasty.Simulation.GameModeRules.HoldFlag;
            m.Objective = AssetDatabase.LoadAssetAtPath<GummyDynasty.Simulation.ObjectiveDefinition>(objPath);
            AssetDatabase.CreateAsset(m, modePath);
        }

        [MenuItem("GummyDynasty/Open Toy (Main)")]
        public static void EnsureEditorPlayPath()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var main = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
            if (main != null)
                EditorSceneManager.playModeStartScene = main;

            if (!File.Exists(MainScenePath))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            var untitledOrBoot = string.IsNullOrEmpty(scene.path) || scene.path == BootScenePath;
            if (untitledOrBoot && scene.path != MainScenePath)
                EditorSceneManager.OpenScene(MainScenePath);
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
