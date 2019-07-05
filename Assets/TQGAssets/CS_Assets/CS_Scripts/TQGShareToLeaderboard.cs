//Version 1.99 (26.02.2018)

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TriviaQuizGame.Types;
using System.Collections.Generic;

namespace TriviaQuizGame
{
    /// <summary>
    /// This script shares a score to a leaderboard using the free dreamlo plugin.
    /// </summary>
    public class TQGShareToLeaderboard : MonoBehaviour
    {
        // Holds some objects for quicker access
        internal TQGGameController gameController;
        internal Text textMessage;
        internal Text inputFieldName;
        internal Button buttonSendScore;
        internal dreamloLeaderBoard dreamloLeaderboard;
        internal List<dreamloLeaderBoard.Score> scoreList;
        public GameObject leaderboard;
        internal RectTransform leaderboardPlayers;
        internal bool showLeaderboard = false;

        internal int listCount = 0;
        internal int index;

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Start()
        {
            // Look for the game controller in this level
            gameController = (TQGGameController)FindObjectOfType(typeof(TQGGameController));

            // Assign the text message object, which shows if the message was sent or failed
            textMessage = GameObject.Find("TextMessage").GetComponent<Text>();

            // Clear the text message
            if (textMessage) textMessage.text = "";

            // Holds the objects for quicker access
            leaderboardPlayers = leaderboard.transform.Find("ScoreTexts/Players").GetComponent<RectTransform>();
            inputFieldName = GameObject.Find("InputFieldName/Text").GetComponent<Text>();
            buttonSendScore = GameObject.Find("ButtonSendScore").GetComponent<Button>();

            // Hide the leaderboard object
            leaderboard.SetActive(false);

            // get the reference to dearmlo leaderboard object in the scene
            this.dreamloLeaderboard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();

            // Go through all the score texts and update them for each player
            for (index = 0; index < leaderboardPlayers.childCount; index++)
            {
                // Display the name of the player
                leaderboardPlayers.GetChild(index).GetComponent<Text>().text = "Loading...";

                // Display the score of the player
                leaderboardPlayers.GetChild(index).Find("ScoreText").GetComponent<Text>().text = "";
            }
        }

        public void Update()
        {
            // Display the score we got
            if (transform.Find("Base/TextScore").GetComponent<Text>().text != gameController.players[gameController.currentPlayer].score.ToString()) transform.Find("Base/TextScore").GetComponent<Text>().text = gameController.players[gameController.currentPlayer].score.ToString();

            if (showLeaderboard == true)
            {
                // Get the list of scores from the leaderboard
                List<dreamloLeaderBoard.Score> scoreList = dreamloLeaderboard.ToListHighToLow();

                // If the list is not empty, show it
                if (scoreList == null)
                {

                }
                else
                {
                    listCount = 0;

                    // Go through each item in the scores list and display the name and score of each player. If this is the score we just submitted, animate it so we can notice it
                    foreach (dreamloLeaderBoard.Score currentScore in scoreList)
                    {
                        // Only fill up names up to the number of text items we have in the list
                        if (listCount < leaderboardPlayers.childCount)
                        {
                            // Display the name of the player
                            leaderboardPlayers.GetChild(listCount).GetComponent<Text>().text = currentScore.playerName;

                            // Display the score of the player
                            leaderboardPlayers.GetChild(listCount).Find("ScoreText").GetComponent<Text>().text = currentScore.score.ToString();

                            // If this is the currently submitted score, animate it so we can notice it form the rest
                            if (leaderboardPlayers.GetChild(listCount).GetComponent<Text>().text == inputFieldName.text)
                            {
                                leaderboardPlayers.GetChild(listCount).GetComponent<Animation>().Play();
                            }

                            showLeaderboard = false;
                        }

                        listCount++;
                    }

                    // Fill the rest of the list with empty "-" marks
                    if (listCount > 0)
                    {
                        for (index = listCount; index < leaderboardPlayers.childCount; index++)
                        {
                            // Empty mark on the player name
                            leaderboardPlayers.GetChild(index).GetComponent<Text>().text = "-";

                            // Empty mark on the player score
                            leaderboardPlayers.GetChild(index).Find("ScoreText").GetComponent<Text>().text = "-";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the current leaderboard without adding a score to it
        /// </summary>
        public void ShowLeaderboard()
        {
            // Show the score to the dreamlo leaderboard
            dreamloLeaderboard.LoadScores();

            // Show the leaderboard object
            leaderboard.SetActive(true);

            // Show the leaderboard results
            showLeaderboard = true;

            // Show the leaderboard object
            GameObject.Find("Base/Leaderboard").SetActive(true);
        }

        /// <summary>
        /// Tries to send score to the dreamlo leaderboar in the scene.
        /// </summary>
        public void TryToSendScore()
        {
            if (inputFieldName.text == "")
            {
                // If we forgot to enter a name, remind us
                if (textMessage) textMessage.text = "You must enter your name";
            }
            else
            {
                // Sending score in progress...
                if (textMessage) textMessage.text = "Sending score...";

                // We can't press the send button anymore
                buttonSendScore.interactable = false;

                // Make sure we have the relevant codes assigned to the dreamlo object
                if (dreamloLeaderboard.publicCode == "") Debug.LogError("You forgot to set the publicCode variable");
                if (dreamloLeaderboard.privateCode == "") Debug.LogError("You forgot to set the privateCode variable");

                // Add the score to the dreamlo leaderboard
                dreamloLeaderboard.AddScore(inputFieldName.text, Mathf.RoundToInt(gameController.players[gameController.currentPlayer].score));

                // Show the leaderboard object
                leaderboard.SetActive(true);

                // Show the leaderboard results
                showLeaderboard = true;

                // Show the leaderboard object
                GameObject.Find("Base/Leaderboard").SetActive(true);
            }
        }
    }
}