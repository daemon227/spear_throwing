using UnityEngine;

namespace Inwave.Production.Utils
{
    /// <summary>
    /// FastSingleton MonoBehaviour ---> For fasted reference purpose
    /// Update:2020-04-21 by hungtx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FastSingleton<T> : MonoBehaviour where T : FastSingleton<T>
    {
        /// <summary>
        /// The public static reference to the Instance
        /// Call if you sure the reference is available
        /// </summary>
        public static T Instance;

        private static bool _instantiated; //checking bool is faster than checking null

        /// <summary>
        /// This is safe reference
        /// Call to get reference in Awake Function
        /// </summary>
        public static T instanceSafe
        {
            get
            {
                if (_instantiated)
                {
                    return Instance;
                }
                Instance = (T)FindObjectOfType(typeof(T));
                if (Instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).ToString());
                    Instance = gameObject.AddComponent<T>();
                }
                if (Instance != null)
                {
                    _instantiated = true;
                }
                return Instance;
            }
        }
        protected virtual void Awake()
        {
            // Make Instance in Awake to make reference performance uniformly.
            if (Instance == null)
            {
                Instance = (T)this;
                _instantiated = true;
            }
            // If there is an Instance already in the same scene, destroy this script.
            else if (Instance != this)
            {
                Debug.LogWarning("Singleton " + typeof(T) + " is already exists.");
                Destroy(gameObject);
            }
        }
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                //Instance = null; //2021-03-29 hungtx removed: do not need to set null, this can cause null exception OnDisable function of other MonoBehaviours call this singleton
                _instantiated = false;
            }
        }
    }
}
