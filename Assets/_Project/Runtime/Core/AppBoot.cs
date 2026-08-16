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
        }

        void Start()
        {
            if (!loadMainOnStart)
                return;

            if (SceneManager.GetSceneByName(mainSceneName).isLoaded)
                return;

            SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
        }

        void OnDestroy()
        {
            GameEvents.Clear();
            ServiceRegistry.Current?.Clear();
        }
    }
}
