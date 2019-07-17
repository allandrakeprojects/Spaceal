using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TriviaQuizGame;
using TriviaQuizGame.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slots : MonoBehaviour, IDropHandler
{
    [Header("<Player Options>")]
    [Tooltip("A list of the players in the game. Each player can be assigned a name, a score text, lives and lives bar. You must have at least one player in the list in order to play the game. You don't need to assign all fields. For example, a player may have a name with no lives bar and it will work fine.")]
    public Player[] players;

    //The current turn of the player. 0 means it's player 1's turn to play, 1 means it's player 2's turn, etc
    internal int currentPlayer = 0;

    // Is this game played in hot-seat mode? This mode lets each player answer a question in turn.
    internal bool playInTurns = true;

    [Tooltip("The number of lives each player has. You lose a life if time runs out, or you answer wrongly too many times")]
    public float lives = 3;

    // The width of a single life in the lives bar. This is calculated from the total width of a life bar divided by the number of lives
    internal float livesBarWidth = 128;

    [Tooltip("The number of players participating in the match. This number cannot be larger than the total number of players list ( The ones that you assign a scoreText/livesBar to )")]
    public int numberOfPlayers = 4;

    internal RectTransform playersObject;

    // ----------

    [Header("<Question Options>")]

    [Tooltip("The object that displays the current question")]
    public Transform questionObject;

    // ----------

    [Header("<Animation & Sounds>")]

    [Tooltip("The animation that plays when showing a new question")]
    public AnimationClip animationQuestion;

    // ----------

    [Header("<User Interface Options>")]

    [Tooltip("The bonus object that displays how much we can win if we answer correctly")]
    public Transform bonusObject;

    [Tooltip("The menu that appears after finishing all the questions in the game. Used for single player and hotseat")]
    public Transform victoryCanvas;

    [Tooltip("The menu that appears if we lose all lives in a single player game")]
    public Transform gameOverCanvas;


    // Questions
    [Tooltip("A list of all possible questions in the game. Each question has a number of correct/wrong answers, a followup text, a bonus value, time, and can also have an image/video as the background of the question")]
    public Question[] questions;
    internal Question[] questionsTemp;

    [Header("<Animation & Sounds>")]
    [Tooltip("The animation that plays when showing an answer")]
    public AnimationClip animationShow;


    // Is the game over?
    internal bool isGameOver = false;

    // Is a question being asked right now?
    internal bool askingQuestion;

    [Tooltip("How many questions from the current bonus group should be asked before moving on to the next group. If we dont sort the questions by bonus groups, then this value is ignored. If there are several players in the game, the value of Questions Per Group will be multiplied by the number of players, so that each one can have a chance to answer a question from the same group before moving on to the next group")]
    public int questionsPerGroup = 2;
    internal int defaultQuestionsPerGroup;
    internal int questionCount = 0;

    // Holds the name of the category loaded into this quiz, if it exists
    internal string currentCategory;

    // The index of the current question being asked. -1 is the index of the first question, 0 the index of the second, and so on
    internal int currentQuestion = -1;

    [Tooltip("Prevent a quiz from repeating questions. Once all questions in a quiz have been asked, they will repeat again.")]
    public bool dontRepeatQuestions = true;

    // How many seconds are left before game over
    internal float timeLeft = 10;

    // Is the timer running?
    internal bool timerRunning = false;

    [Tooltip("If we set this time higher than 0, it will override the individual times for each question. The global time does not reset between questions")]
    public float globalTime = 0;

    // A general use index
    internal int index = 0;
    internal int indexB = 0;

    [Tooltip("Randomize the list of questions. Use this if you don't want the questions to appear in the same order every time you play. Combine this with 'sortQuestions' if you want the questions to be randomized within the bonus groups.")]
    public bool randomizeQuestions = true;

    //The buttons that display the possible answers
    internal Transform[] answerObjects;

    [Tooltip("Randomize the display order of answers when a new question is presented")]
    public bool randomizeAnswers = true;

    [Tooltip("Sort the list of questions from lowest bonus to highest bonus and put them into groups. Use this if you want the questions to be displayed from the easiest to the hardest ( The difficulty of a question is decided by the bonus value you give to it )")]
    public bool sortQuestions = true;

    [Tooltip("Various sounds and their source")]
    public AudioClip soundQuestion;
    public AudioClip soundCorrect;
    public AudioClip soundWrong;
    public AudioClip soundTimeUp;
    public AudioClip soundGameOver;
    public AudioClip soundVictory;
    public string soundSourceTag = "Sound";
    internal GameObject soundSource;








    void Start()
    {
        //Assign the sound source for easier access
        if (GameObject.FindGameObjectWithTag(soundSourceTag)) soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);
    }






    public IEnumerator QuestionFlip(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Animate the question
        if (animationQuestion)
        {
            // If the animation clip doesn't exist in the animation component, add it
            if (questionObject.GetComponent<Animation>().GetClip(animationQuestion.name) == null) questionObject.GetComponent<Animation>().AddClip(animationQuestion, animationQuestion.name);

            // Play the animation
            questionObject.GetComponent<Animation>().Play(animationQuestion.name);

            // Wait for half the animation time, then display the next question. This will make the question appear while the question tab flips. Just a nice effect
            yield return new WaitForSeconds(questionObject.GetComponent<Animation>().clip.length * 0.5f);
        }

        // Set the bonus we can get for this question 
        if (bonusObject && bonusObject.GetComponent<Animation>())
        {
            // Animate the bonus object
            bonusObject.GetComponent<Animation>().Play();

            // Reset the bonus animation
            bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = -1;

            // Display the bonus text
            // adpd update
            bonusObject.Find("Text").GetComponent<Text>().text = "+1";
        }

        AskQuestion();
    }

    void AskQuestion()
    {
        currentCategory = PlayerPrefs.GetString("Category");

        if (PlayerPrefs.GetInt("DragAndDropCurrentCount") <= 2)
        {
            if (soundSource && soundQuestion) soundSource.GetComponent<AudioSource>().PlayOneShot(soundQuestion);

            GameObject.Find("QuestionsCount").GetComponent<Text>().text = PlayerPrefs.GetInt("DragAndDropCurrentCount") + " of " + PlayerPrefs.GetInt("DragAndDropLimit") + "\nQuestions";
            // Next question
            string questionAnswer = PlayerPrefs.GetString("DragAndDrop" + PlayerPrefs.GetInt("DragAndDropCurrentCount"));
            String[] questionAnswerArray = questionAnswer.ToString().Replace("Drag the answer to complete the question.", "").Split(new string[] { " --- " }, StringSplitOptions.None);

            string question = questionAnswerArray[0];
            string answer = questionAnswerArray[1];
            String[] questionArray = question.ToString().Split(new string[] { " || " }, StringSplitOptions.None);
            String[] answerArray = answer.ToString().Split(new string[] { " || " }, StringSplitOptions.None);
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                int number;

                do number = rand.Next(0, 4);
                while (randomNumbers.Contains(number));

                randomNumbers.Add(number);

                Sprite sprite = Resources.Load("New Folder/Level/DragAndDrop/" + questionArray[i].Trim(), typeof(Sprite)) as Sprite;
                if (sprite)
                {
                    GameObject.Find("DragAndDropObject/ButtonAnswer" + number).GetComponent<Image>().sprite = sprite;
                    GameObject.Find("DragAndDropObject/ButtonAnswer" + number).GetComponentInChildren<Text>().text = "\n\n\n\n\n\n\n\n\n" + questionArray[i].Trim();

                }
                else
                {
                    Sprite sprite_ = Resources.Load("New Folder/Buttons/buttonblue", typeof(Sprite)) as Sprite;
                    GameObject.Find("DragAndDropObject/ButtonAnswer" + number).GetComponent<Image>().sprite = sprite_;
                    GameObject.Find("DragAndDropObject/ButtonAnswer" + number).GetComponentInChildren<Text>().text = questionArray[i].Trim();
                }

                GameObject.Find("Answers/ButtonAnswer" + number).GetComponent<Image>().enabled = true;
                GameObject.Find("Answers/ButtonAnswer" + number + "/Text").GetComponent<Text>().text = answerArray[i].Trim();

                // Play the animation
                GameObject.Find("Answers/ButtonAnswer" + number).GetComponent<Animation>().AddClip(animationShow, animationShow.name);
                GameObject.Find("Answers/ButtonAnswer" + number).GetComponent<Animation>().Play(animationShow.name);
            }
        }
        else
        {
            if ((PlayerPrefs.GetInt("DragAndDropScore") >= 7 && currentCategory.ToString().ToLower().Contains("level 1")) ||
                (PlayerPrefs.GetInt("DragAndDropScore") >= 1 && currentCategory.ToString().ToLower().Contains("level 2")) ||
                (PlayerPrefs.GetInt("DragAndDropScore") >= 17))
            {
                StartCoroutine(Victory(0));
            }
            else
            {
                StartCoroutine(GameOver(0));
            }
        }

        PlayerPrefs.DeleteKey("IsDragCorrect");
        PlayerPrefs.SetInt("DragAndDropCount", 0);
    }

    private System.Random rand = new System.Random();



    IEnumerator Victory(float delay)
    {
        // Record the state of the category as completed
        if (currentCategory != null)
        {
            PlayerPrefs.SetInt(currentCategory + "Completed", 1);

            currentCategory = null;
        }
        yield return new WaitForSeconds(delay);

        //Show the game over screen
        if (victoryCanvas)
        {
            //Show the victory screen
            victoryCanvas.gameObject.SetActive(true);

            // If we have a TextScore and TextHighScore objects, then we are using the single player victory canvas
            if (victoryCanvas.Find("ScoreTexts/TextScore") && victoryCanvas.Find("ScoreTexts/TextHighScore"))
            {
                if ((PlayerPrefs.GetInt("DragAndDropLimit") == 10 && PlayerPrefs.GetInt("DragAndDropScore") == 10) ||
                    (PlayerPrefs.GetInt("DragAndDropLimit") == 20 && PlayerPrefs.GetInt("DragAndDropScore") == 20))
                {
                    // 3 stars
                    victoryCanvas.Find("TextTitle").GetComponent<Text>().text = "PERFECT!";
                }
                else if ((PlayerPrefs.GetInt("DragAndDropLimit") == 10 && (PlayerPrefs.GetInt("DragAndDropScore") <= 9 && PlayerPrefs.GetInt("DragAndDropScore") <= 7)) ||
                        (PlayerPrefs.GetInt("DragAndDropLimit") == 20 && (PlayerPrefs.GetInt("DragAndDropScore") <= 19 && PlayerPrefs.GetInt("DragAndDropScore") <= 17)))
                {
                    // 2 stars
                }
                else
                {
                    // 1 stars
                }

                //Write the score text, if it exists
                victoryCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text += "YOUR SCORE IS \n" + PlayerPrefs.GetInt("DragAndDropScore").ToString() + " of " + PlayerPrefs.GetInt("DragAndDropLimit");

//                //Check if we got a high score
//                if (PlayerPrefs.GetInt("DragAndDropScore") > highScore)
//                {
//                    highScore = PlayerPrefs.GetInt("DragAndDropScore");

//                    //Register the new high score
//#if UNITY_5_3 || UNITY_5_3_OR_NEWER
//                    PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", PlayerPrefs.GetInt("DragAndDropScore"));
//#else
//                        PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", PlayerPrefs.GetInt("DragAndDropScore"));
//#endif
//                }

                //Write the high sscore text
                //victoryCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text += " " + highScore.ToString();
            }

            // If we have a Players object, then we are using the hotseat results canvas
            if (victoryCanvas.Find("ScoreTexts/Players"))
            {
                // Sort the players by their score and then check the winners ( could be a draw with more than one winner )
                // The number of winners, could be more than one in case of a draw
                int winnerCount = 0;

                // Sort the players by the score they have, from highest to lowest
                Array.Sort(players, delegate (Player x, Player y) { return y.score.CompareTo(x.score); });

                // Go through all the players and check if we have more than one winner
                for (index = 0; index < numberOfPlayers; index++)
                {
                    // The first player in the list is always the winner. After that we check if there are other players with the same score ( a draw between several winners )
                    if (index == 0) winnerCount = 1;
                    else if (players[index].score == players[0].score)
                    {
                        winnerCount++;
                    }
                }

                // Go through all the players in the table and hide the winner icon from all the losers, or if everyone got 0 points.
                for (index = 0; index < numberOfPlayers; index++)
                {
                    if (players[index].score <= 0 || index >= winnerCount) victoryCanvas.Find("ScoreTexts/Players").GetChild(index).Find("WinnerIcon").gameObject.SetActive(false);
                }

                // Go through all the score texts and update them each player
                for (index = 0; index < numberOfPlayers; index++)
                {
                    // Display the name of the player
                    victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetComponent<Text>().text = players[index].name;

                    // Set the color of the player name
                    victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetComponent<Text>().color = players[index].color;

                    // Display the score of the player
                    victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetChild(0).GetComponent<Text>().text = players[index].score.ToString();
                }

                // If the value of numberOfPlayers is lower than the actual number of players, remove any excess players from the list
                if (numberOfPlayers < players.Length)
                {
                    // Go through all the extra players in the list, and remove their name, score, and lives objects
                    for (index = numberOfPlayers; index < victoryCanvas.Find("ScoreTexts/Players").childCount; index++)
                    {
                        victoryCanvas.Find("ScoreTexts/Players").GetChild(index).gameObject.SetActive(false);
                    }
                }

                // Display the list of winners
                // If we have one winner display a single name. Otherwise, display a "draw" message with the names of all the winners
                if (winnerCount == 1)
                {
                    // Display the single winner
                    victoryCanvas.Find("TextResult").GetComponent<Text>().text = players[0].name + " wins with " + players[0].score.ToString() + " points!";
                }
                else
                {
                    // Display the "draw" message between several winners
                    victoryCanvas.Find("TextResult").GetComponent<Text>().text = "It's a draw between ";

                    // Display the names of the winners
                    while (winnerCount > 0)
                    {
                        winnerCount--;

                        // Add to the text the name of the next winner
                        if (winnerCount == 0) victoryCanvas.Find("TextResult").GetComponent<Text>().text += "and " + players[winnerCount].name;
                        else victoryCanvas.Find("TextResult").GetComponent<Text>().text += players[winnerCount].name + ", ";
                    }

                    // Display the score they got
                    victoryCanvas.Find("TextResult").GetComponent<Text>().text += ", each with " + players[0].score.ToString() + " points!";
                }
            }

            // If we have a TextProgress object, then we can display how many questions we answered correctly
            //if (victoryCanvas.Find("ScoreTexts/TextProgress"))
            //{
            //    //Write the progress text
            //    victoryCanvas.Find("ScoreTexts/TextProgress").GetComponent<Text>().text = correctAnswers.ToString() + "/" + PlayerPrefs.GetInt("DragAndDropLimit").ToString();
            //}

            //If there is a source and a sound, play it from the source
            if (soundSource && soundVictory) soundSource.GetComponent<AudioSource>().PlayOneShot(soundVictory);
        }
    }

    IEnumerator GameOver(float delay)
    {
        isGameOver = true;

        // Calculate the quiz duration
        //playTime = DateTime.Now - startTime;

        yield return new WaitForSeconds(delay);

        //Show the game over screen
        if (gameOverCanvas)
        {
            //Show the game over screen
            gameOverCanvas.gameObject.SetActive(true);

            //Write the score text, if it exists
            if (gameOverCanvas.Find("ScoreTexts/TextScore")) gameOverCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text += "YOUR SCORE IS \n" + PlayerPrefs.GetInt("DragAndDropScore").ToString() + " of " + PlayerPrefs.GetInt("DragAndDropLimit");

            //Check if we got a high score
//            if (PlayerPrefs.GetInt("DragAndDropScore") > highScore)
//            {
//                highScore = PlayerPrefs.GetInt("DragAndDropScore");

//                //Register the new high score
//#if UNITY_5_3 || UNITY_5_3_OR_NEWER
//                PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", PlayerPrefs.GetInt("DragAndDropScore"));
//#else
//                    PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", PlayerPrefs.GetInt("DragAndDropScore"));
//#endif
//            }

            if ((PlayerPrefs.GetInt("DragAndDropLimit") == 10 && PlayerPrefs.GetInt("DragAndDropScore") == 6) ||
                (PlayerPrefs.GetInt("DragAndDropLimit") == 20 && PlayerPrefs.GetInt("DragAndDropScore") == 16))
            {
                gameOverCanvas.Find("TextTitle").GetComponent<Text>().text = "YOU CAN DO IT!";
            }

            //Write the high sscore text
            int passingScore = 0;
            if (PlayerPrefs.GetInt("DragAndDropLimit") == 10)
            {
                passingScore = 7;
            }
            else if (PlayerPrefs.GetInt("DragAndDropLimit") == 20)
            {
                passingScore = 17;
            }
            gameOverCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text += "Passing score is " + passingScore;

            //If there is a source and a sound, play it from the source
            if (soundSource && soundGameOver) soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
        }
    }






















































    void Update()
    {
        //// Make the score count up to its current value, for the current player
        //print(PlayerPrefs.GetInt("DragAndDropScore") + " --- " + PlayerPrefs.GetInt("DragAndDropScore")Count);
        //if (PlayerPrefs.GetInt("DragAndDropScore") < PlayerPrefs.GetInt("DragAndDropScore")Count)
        //{
        //    // Count up to the courrent value
        //    PlayerPrefs.GetInt("DragAndDropScore") = Mathf.Lerp(PlayerPrefs.GetInt("DragAndDropScore"), PlayerPrefs.GetInt("DragAndDropScore")Count, Time.deltaTime * 10);

        //    // Round up the score value
        //    PlayerPrefs.GetInt("DragAndDropScore") = Mathf.CeilToInt(PlayerPrefs.GetInt("DragAndDropScore"));

        //    // Update the score text
        //    UpdateScore();
        //}
    }

    void UpdateScore()
    {
        //Update the score text
        //if ( scoreText )    scoreText.GetComponent<Text>().text = score.ToString();

        //Update the score text for the current player
        GameObject.Find("ScoreText").GetComponent<Text>().text = "Score: " + PlayerPrefs.GetInt("DragAndDropScore").ToString();

        // If we reach the victory score we win the game
        // adpd update
        //if (scoreToVictory > 0 && PlayerPrefs.GetInt("DragAndDropScore") >= scoreToVictory) StartCoroutine(Victory(0));
    }

    public GameObject item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }

            return null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!item)
        {
            // Drag and Drop Correct or Wrong
            string questionGameObject = PlayerPrefs.GetString("OnBeginDrag");
            String[] questionGameObjectArray = questionGameObject.ToString().Split(new string[] { " || " }, StringSplitOptions.None);
            string answer_ = questionGameObjectArray[0];
            string gameObject_ = questionGameObjectArray[1];
            string answer = gameObject.GetComponentInChildren<Text>().text + " --- " + answer_;
            ReadString(answer.Trim(), gameObject.name);
            //DragHandeler.itemBeingDragged.transform.SetParent(transform);
            //ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
            GameObject.Find("Answers/" + gameObject_).GetComponent<Image>().enabled = false;
            GameObject.Find("Answers/" + gameObject_).GetComponentInChildren<Text>().text = "";
        }
    }

    //[MenuItem("Tools/Read file")]
    public void ReadString(string answer, string index)
    {
        TextAsset ELDragAndDrop = (TextAsset)Resources.Load("ELDragAndDrop");
        if (ELDragAndDrop.text.Contains(answer))
        {
            if (soundSource && soundCorrect) soundSource.GetComponent<AudioSource>().PlayOneShot(soundCorrect);

            Sprite sprite = Resources.Load("New Folder/Buttons/correct", typeof(Sprite)) as Sprite;
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index).GetComponent<Image>().sprite = sprite;
            if (PlayerPrefs.GetString("IsDragCorrect") != "")
            {
                if (PlayerPrefs.GetString("IsDragCorrect") != "F")
                {
                    PlayerPrefs.SetString("IsDragCorrect", "T");
                }
            }
            else
            {
                PlayerPrefs.SetString("IsDragCorrect", "T");
            }
        }
        else
        {
            if (soundSource && soundWrong) soundSource.GetComponent<AudioSource>().PlayOneShot(soundWrong);

            Sprite sprite = Resources.Load("New Folder/Buttons/wrong", typeof(Sprite)) as Sprite;
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index).GetComponent<Image>().sprite = sprite;
            PlayerPrefs.SetString("IsDragCorrect", "F");
        }
        String[] answerArray = answer.ToString().Split(new string[] { " --- " }, StringSplitOptions.None);
        if (answerArray[0].Contains("-"))
        {
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index + "/" + index).GetComponent<Text>().text = answerArray[0].Trim().Replace("-", answerArray[1].Trim());
        }
        else
        {
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index + "/" + index).GetComponent<Text>().text = answerArray[0].Trim() + " " + answerArray[1].Trim();
        }

        int getDragAndDropCount = PlayerPrefs.GetInt("DragAndDropCount");
        string getIsDragCorrect = PlayerPrefs.GetString("IsDragCorrect");
        PlayerPrefs.SetInt("DragAndDropCount", getDragAndDropCount + 1);
        getDragAndDropCount = PlayerPrefs.GetInt("DragAndDropCount");
        if (getDragAndDropCount == 4)
        {
            if (getIsDragCorrect == "T")
            {
                int dragAndDropScore = PlayerPrefs.GetInt("DragAndDropScore");
                PlayerPrefs.SetInt("DragAndDropScore", dragAndDropScore + 1);
                GameObject.Find("ScoreText").GetComponent<Text>().text = "Score: " + (dragAndDropScore + 1);

                if (bonusObject && bonusObject.GetComponent<Animation>())
                {
                    // Animate the bonus object
                    bonusObject.GetComponent<Animation>().Play();

                    // Reset the bonus animation
                    bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = 1;
                }
            }
            int getDragAndDropCurrentCount = PlayerPrefs.GetInt("DragAndDropCurrentCount", 0);
            PlayerPrefs.SetInt("DragAndDropCurrentCount", getDragAndDropCurrentCount + 1);
            StartCoroutine(QuestionFlip(0.5f));
        }
    }
}
