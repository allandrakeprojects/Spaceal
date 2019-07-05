//Version 1.58 (17.07.2016)

using System;
using UnityEngine;

namespace TriviaQuizGame.Types
{
	[Serializable]
	public class Answer
	{
		[Tooltip("The answer to a question")]
		public string answer;

		[Tooltip("An image that accompanies the answer. You can leave this empty if you don't want an image")]
		public Sprite image;

        [Tooltip("The address of an image that is loaded through a URL. You can leave this empty if you don't want an image")]
        public string imageURL;

#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_BLACKBERRY && !UNITY_WP8 && !UNITY_WEBGL
        [Tooltip("A video that accompanies the answer. You can leave this empty if you don't want a video")]
		internal MovieTexture video;
		#endif

		[Tooltip("A sound that accompanies the answer. You can leave this empty if you don't want a sound")]
		public AudioClip sound;

        [Tooltip("The address of a sound that accompanies the answer. You can leave this empty if you don't want a sound")]
        public string soundURL;

        [Tooltip("This answer is correct")]
		public bool isCorrect = false;
	}
}
