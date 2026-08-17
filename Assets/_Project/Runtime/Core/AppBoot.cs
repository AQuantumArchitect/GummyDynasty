using UnityEngine;
using UnityEngine.SceneManagement;

namespace GummyDynasty.Core
{
    /// <summary>Lives in Boot scene. Builds the service graph and loads Main.</summary>
    public sealed class AppBoot : MonoBehaviour
    {
        public const string MainSceneName = "Main";

        [SerializeField] string mainSceneName = MainSceneName;
        [SerializeField] bool loadMainOnStart = true;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var registry = new ServiceRegistry();
            ServiceRegistry.Install(registry);
            GameEvents.RaiseStatus("GummyDynasty boot");
            EnsureBootCamera();
        }

        void Start()
        {
            if (!loadMainOnStart)
                return;

            if (SceneManager.GetSceneByName(mainSceneName).isLoaded)
                return;

            if (!Application.CanStreamedLevelBeLoaded(mainSceneName))
            {
                GameEvents.RaiseStatus("Main scene is not in File > Build Profiles. Open Assets/_Project/Scenes/Main.unity");
                Debug.LogError("GummyDynasty: cannot load scene '" + mainSceneName + "'. Open Main.unity and press Play.");
                return;
            }

            SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
        }

        static void EnsureBootCamera()
        {
            if (Camera.main != null)
                return;
            var go = new GameObject("BootCamera");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.95f, 0.32f, 0.48f);
            go.tag = "MainCamera";
        }

        void OnDestroy()
        {
            GameEvents.Clear();
            ServiceRegistry.Current?.Clear();
        }
    }
}
