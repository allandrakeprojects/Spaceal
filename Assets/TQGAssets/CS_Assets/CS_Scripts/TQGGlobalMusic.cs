//Version 1.99 (26.02.2018)

using UnityEngine;
using System.Collections;

namespace TriviaQuizGame
{
    /// <summary>
    /// handles a global music source which carries over from scene to scene without resetting the music track.
    /// You can have this script attached to a music object and include that object in each scene, and the script will keep
    /// only the oldest music source in the scene. If there is new music, it will crossfade and the replace the old one.
    /// </summary>
    public class TQGGlobalMusic : MonoBehaviour
    {
        [Tooltip("The tag of the music source")]
        public string musicTag = "Music";

        [Tooltip("The time this instance of the music source has been in the game")]
        internal float instanceTime = 0;
        internal bool destroyThis = false;

        internal GameObject[] musicObjects;
        internal AudioSource firstMusicObject;
        internal AudioSource secondMusicObject;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Awake is used to initialize any variables or game state before the game starts. Awake is called only once during the 
        /// lifetime of the script instance. Awake is called after all objects are initialized so you can safely speak to other 
        /// objects or query them using eg. GameObject.FindWithTag. Each GameObject's Awake is called in a random order between objects. 
        /// Because of this, you should use Awake to set up references between scripts, and use Start to pass any information back and forth. 
        /// Awake is always called before any Start functions. This allows you to order initialization of scripts. Awake can not act as a coroutine.
        /// </summary>
        void Awake()
        {
            //Find all the music objects in the scene
            musicObjects = GameObject.FindGameObjectsWithTag(musicTag);

            //Keep only the music object which has been in the game for more than 0 seconds
            if (musicObjects.Length > 1)
            {
                // Go through all the music objects in the scene, and check if there is new music playing
                foreach (var musicObject in musicObjects)
                {
                    // Organize the music objects from old to new
                    if (musicObject.GetComponent<TQGGlobalMusic>().instanceTime <= 0)    secondMusicObject = musicObject.GetComponent<AudioSource>();
                    else    firstMusicObject = musicObject.GetComponent<AudioSource>();

                    if (firstMusicObject && secondMusicObject)
                    {
                        // If the music didn't change, remove the new music object and keep playing the old one. Otherwise, if we have new music, cross-fade from the old music to the new one
                        if (firstMusicObject.clip == secondMusicObject.clip) Destroy(secondMusicObject.gameObject);
                        else StartCoroutine("CrossFade");

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Cross fades two music objects, lowering the volume of the old one, and increasing the volume of the new one
        /// </summary>
        /// <returns></returns>
        IEnumerator CrossFade()
        {
            float timeOut = 1;

            // Gradually cross-fade the music objects
            while (timeOut > 0)
            {
                // Reduce the volume of the first music object
                firstMusicObject.volume = timeOut;

                // Increase the volume of the second music object
                secondMusicObject.volume = 1 - timeOut;

                // Wait for update. This is used to allow animation
                yield return new WaitForSeconds(Time.deltaTime);

                timeOut -= Time.deltaTime;
            }

            // Remove the first music object as we don't need it anymore
            Destroy(firstMusicObject.gameObject);
        }

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Start()
        {
            //Don't destroy this object when loading a new scene
            DontDestroyOnLoad(transform.gameObject);
        }

        private void Update()
        {
            instanceTime += Time.deltaTime;
        }

    }
}
