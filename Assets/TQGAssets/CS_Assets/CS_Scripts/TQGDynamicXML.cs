//Version 1.97 (11.11.2017)

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

// For XML
using System.Text;
using System.Xml;
using System.IO;

namespace TriviaQuizGame.Types
{
	/// <summary>
	/// Toggles a sound source when clicked on. It also records the sound state (on/off) in a PlayerPrefs. 
	/// In order to detect clicks you need to attach this script to a UI Button and set the proper OnClick() event.
	/// </summary>
	public class TQGDynamicXML:MonoBehaviour
	{
		// Address type, either online or local
		public enum AddressType{ Online, Local }

		[Tooltip("The type of the address can either be online, or local ( from the computer )")]
		public AddressType addressType;

		// Used to hold the gamecontroller or th ecategory object, if they exist
		internal TQGGameController gameController;
		internal Category category;

		[Tooltip("The address of the Xml you want to load from the web. This is used if you want to load a set of questions while the game is running online in a browser")]
		[HideInInspector]
		public string xmlAddress;
		
		[Tooltip("How many seconds to wait for the Xml to be loaded before giving up and showing the error message")]
		[Range(0.5f, 5.0f)]
		public float xmlLoadTimeout = 1;
        
		[Tooltip("The message we show when the Xml could not be loaded from the online address")]
		public Transform xmlLoadErrorCanvas;
		
		// This is used when parsing the Xml info
		internal XmlNodeList xmlRecords;

		// Holds the list of questions loaded from the address. If they were already loaded before, they are not loaded again
		static Question[] questionsFromAddress = new Question[0];

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
			gameController = (TQGGameController) FindObjectOfType(typeof(TQGGameController));

			category = GetComponent<Category>();
		}

		/// <summary>
		/// Gets the XML from an address, either online or local ( on the computer )
		/// </summary>
		/// <returns>The XML from address.</returns>
		/// <param name="addressType">Address type.</param>
		public IEnumerator GetXMLFromAddress( string addressType )
		{
			// If we have a	web address for the Xml, load it from the site. This is not executed in the unity editor, but only in the actual build
			if ( xmlAddress != String.Empty )
			{
				// Get the address, assuming it is an online address
				WWW address = new WWW(xmlAddress);

				// If the address is local ( on the computer ), apply the correct "file:///" prefix
				if ( addressType == "Local" )    address = new WWW("file:///" + xmlAddress);

				// Wait until it has been loaded
				yield return address;

                // Print the error to the console
                if (!String.IsNullOrEmpty(address.error))
                {
                    // Display the actual error in the console
                    Debug.Log(address.error);

                    //Show the xml load error screen, and display the relevant error
                    if (xmlLoadErrorCanvas)
                    {
                        xmlLoadErrorCanvas.gameObject.SetActive(true);

                        // If we have a "404" or "Failed" error, it means the address of the XML file is wrong. If we have a "Host" error, it means we have no internet connection. Otherwise, display the full error text
                        if (address.error.Contains("404")) xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Question list can't be found (404)";
                        else if (address.error.Contains("Host Not Found")) xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Internet connection problem";
                        else if (address.error.Contains("Failed")) xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Question list can't be found";
                        else xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += address.error;
                    }

                    // Try to see if there is a local copy of the address
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
                    address = new WWW("file:///" + Application.persistentDataPath + SceneManager.GetActiveScene().name + ".xml");

#else
                    address = new WWW("file:///" + Application.persistentDataPath + Application.loadedLevelName + ".xml");
#endif

                    // Wait until it has been loaded
                    yield return address;
                }   

                // Print the error to the console
                if ( !String.IsNullOrEmpty(address.error) )    
				{
                    // Display the actual error in the console
                    Debug.Log(address.error);
						
					//Show the xml load error screen, and display the relevant error
					if ( xmlLoadErrorCanvas )    
					{
						xmlLoadErrorCanvas.gameObject.SetActive(true);
							
						// If we have a "404" or "Failed" error, it means the address of the XML file is wrong. If we have a "Host" error, it means we have no internet connection. Otherwise, display the full error text
						if ( address.error.Contains("404") )    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Question list can't be found (404)";
						else if ( address.error.Contains("Host Not Found") )    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Internet connection problem";
						else if ( address.error.Contains("Failed") )    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Question list can't be found";
						else    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += address.error;
					}
				}
                    
                if (gameController)
                {
                    // Load the Xml info from the address file
                    gameController.LoadXml(address.text, false);

                    // Keep a copy of the quiz so that we don't keep loading the same quiz if it has already been loaded
                    questionsFromAddress = gameController.questions;
                }

                if (category)
                {
                    // Load the Xml info from the address file
                    category.LoadXml(address.text, false);

                    // Keep a copy of the quiz so that we don't keep loading the same quiz if it has already been loaded
                    questionsFromAddress = category.questions;

                    gameController.SendMessage("SetQuestions", questionsFromAddress);

                }
#if !UNITY_WEBPLAYER
                // Save a copy of the quiz on the device, so that if we play offline we can still access it
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
                string path = Application.persistentDataPath + SceneManager.GetActiveScene().name + ".xml";
#else
                string path = Application.persistentDataPath + Application.loadedLevelName + ".xml";
#endif


                if (path.Length != 0)
                {
                    // Save the XML file to disk
                    if (gameController.GetComponent<TQGGameController>()) File.WriteAllBytes(path, System.Text.Encoding.UTF8.GetBytes(gameController.GetComponent<TQGGameController>().SaveXml()));
                    if (gameController.GetComponent<Category>()) File.WriteAllBytes(path, System.Text.Encoding.UTF8.GetBytes(gameController.GetComponent<Category>().SaveXml()));
                }
#endif

                // Hide the error screen
                if (xmlLoadErrorCanvas) xmlLoadErrorCanvas.gameObject.SetActive(false);

            }
		}
	}
}