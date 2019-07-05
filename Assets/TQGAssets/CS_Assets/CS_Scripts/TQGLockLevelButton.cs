//Version 1.90 (29.07.2017)

using UnityEngine;
using UnityEngine.UI;

namespace TriviaQuizGame
{
    /// <summary>
    /// Locks a level based on a value in a PlayerPrefs record, such as the HighScore of a level. You can only attach this component to a button with Load Level script
    /// </summary>
    [RequireComponent(typeof(TQGLoadLevel))]
    [RequireComponent(typeof(Button))]
    public class TQGLockLevelButton : MonoBehaviour
    {
        [Tooltip("The PlayerPrefs record we check to see if this level is locked")]
        public string playerPrefsRecord = "CS_GameHighScore";

        [Tooltip("The value needed in order to unlock this level button, such as a HighScore of 10000")]
        public float valueToUnlock = 1000;

        [Tooltip("The message that appears when we don't have the needed value to unlock this level button")]
        public string lockedMessage = "Get 1000 points in Single Player";

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Awake()
        {
            CheckLockState();
        }

        /// <summary>
        /// Checks if this level is unlocked, based on a PlayerPrefs value we choose from the level
        /// </summary>
        public void CheckLockState()
        {
            // If the value needed is too low, lock the level button
            if (PlayerPrefs.GetFloat(playerPrefsRecord, 0) < valueToUnlock)
            {
                LockLevel();
            }
        }

        /// <summary>
        /// Locks a level button and shows a relevant message if needed
        /// </summary>
        public void LockLevel()
        {
            // Lock the level button
            GetComponent<Button>().interactable = false;

            // If there is a lock message, show it
            if ( lockedMessage != string.Empty ) transform.Find("Text").GetComponent<Text>().text = lockedMessage;
        }
    }
}