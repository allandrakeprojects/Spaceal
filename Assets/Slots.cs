using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    [Tooltip("Limit the total number of questions asked, regardless of whether we answered correctly or not. Use this if you want to have a strict number of questions asked in the game (ex: 10 questions). If you keep it at 0 the number of questions will not be limited and you will go through all the question groups in the quiz before finishing it")]
    public int questionLimit = 0;

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

    // The total number of questions we asked. This is used to check if we reached the question limit.
    internal int questionLimitCount = 0;

    [Tooltip("Sort the list of questions from lowest bonus to highest bonus and put them into groups. Use this if you want the questions to be displayed from the easiest to the hardest ( The difficulty of a question is decided by the bonus value you give to it )")]
    public bool sortQuestions = true;
















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
        if (PlayerPrefs.GetInt("DragAndDropCurrentCount") <= PlayerPrefs.GetInt("DragAndDropLimit"))
        {
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
            print("computation");
        }

        PlayerPrefs.DeleteKey("IsDragCorrect");
        PlayerPrefs.SetInt("DragAndDropCount", 0);
    }

    private System.Random rand = new System.Random();

    void Update()
    {
        //// Make the score count up to its current value, for the current player
        //print(players[currentPlayer].score + " --- " + players[currentPlayer].scoreCount);
        //if (players[currentPlayer].score < players[currentPlayer].scoreCount)
        //{
        //    // Count up to the courrent value
        //    players[currentPlayer].score = Mathf.Lerp(players[currentPlayer].score, players[currentPlayer].scoreCount, Time.deltaTime * 10);

        //    // Round up the score value
        //    players[currentPlayer].score = Mathf.CeilToInt(players[currentPlayer].score);

        //    // Update the score text
        //    UpdateScore();
        //}
    }

    void UpdateScore()
    {
        //Update the score text
        //if ( scoreText )    scoreText.GetComponent<Text>().text = score.ToString();

        //Update the score text for the current player
        GameObject.Find("ScoreText").GetComponent<Text>().text = "Score: " + players[currentPlayer].score.ToString();

        // If we reach the victory score we win the game
        // adpd update
        //if (scoreToVictory > 0 && players[currentPlayer].score >= scoreToVictory) StartCoroutine(Victory(0));
    }


    void Start()
    {
    }

    public GameObject item
    {
        get
        {
            if (transform.childCount > 0)
            {
                //Debug.Log(gameObject.name.ToString().ToLower() + " " + gameObject.GetComponentInChildren<Text>().text);
                //if (gameObject.name.ToString().ToLower().Contains("buttonanswer"))
                //{
                //    if (gameObject.GetComponentInChildren<Text>().text == "L / - / N")
                //    {
                //        PlayerPrefs.SetString("Question Answer0", "M||" + gameObject.name.ToLower().Replace("buttonanswer", "").ToString() + " ");
                //        PlayerPrefs.Save();
                //    }
                //    if (gameObject.GetComponentInChildren<Text>().text == "V / - / X")
                //    {
                //        PlayerPrefs.SetString("Question Answer1", "W||" + gameObject.name.ToLower().Replace("buttonanswer", "").ToString() + " ");
                //        PlayerPrefs.Save();
                //    }
                //    if (gameObject.GetComponentInChildren<Text>().text == "- / E / F")
                //    {
                //        PlayerPrefs.SetString("Question Answer2", "D||" + gameObject.name.ToLower().Replace("buttonanswer", "").ToString() + " ");
                //        PlayerPrefs.Save();
                //    }
                //    if (gameObject.GetComponentInChildren<Text>().text == "- / B / C")
                //    {
                //        PlayerPrefs.SetString("Question Answer3", "A||" + gameObject.name.ToLower().Replace("buttonanswer", "").ToString() + " ");
                //        PlayerPrefs.Save();
                //    }
                //}

                //if (gameObject.name.ToString().ToLower().Contains("slot"))
                //{
                //    string correctOrWrong = "";
                //    for (int i = 0; i < 4; i++)
                //    {
                //        if (PlayerPrefs.GetString("Question Answer" + i).Trim() == gameObject.GetComponentInChildren<Text>().text + "||" + gameObject.name.ToLower().Replace("slot", "").ToString().Trim())
                //        {
                //            correctOrWrong = "correct";
                //            PlayerPrefs.DeleteKey("Question Answer" + i);
                //            int questionAnswerCount = PlayerPrefs.GetInt("Question Answer Count");
                //            PlayerPrefs.DeleteKey("Question Answer Count");
                //            PlayerPrefs.SetInt("Question Answer Count", questionAnswerCount - 1);
                //            PlayerPrefs.Save();
                //            break;
                //        }
                //        else
                //        {
                //            correctOrWrong = "wrong";
                //        }
                //    }

                //    Debug.Log(correctOrWrong);
                //}

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

    [MenuItem("Tools/Read file")]
    public void ReadString(string answer, string index)
    {
        string path = "Assets/TQGAssets/Resources/ELDragAndDrop.txt";
        StreamReader reader = new StreamReader(path);
        if (reader.ReadToEnd().ToString().Contains(answer))
        {
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
            Sprite sprite = Resources.Load("New Folder/Buttons/wrong", typeof(Sprite)) as Sprite;
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index).GetComponent<Image>().sprite = sprite;
            PlayerPrefs.SetString("IsDragCorrect", "F");
        }
        reader.Close();
        String[] answerArray = answer.ToString().Split(new string[] { " --- " }, StringSplitOptions.None);
        if (answerArray[0].Contains("-"))
        {
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index + "/" + index).GetComponent<Text>().text = answerArray[0].Trim().Replace("-", answerArray[1].Trim());
        }
        else
        {
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index + "/" + index).GetComponent<Text>().text = answerArray[1].Trim() + ". - " + answerArray[0].Trim();
        }

        int getDragAndDropCount = PlayerPrefs.GetInt("DragAndDropCount");
        string getIsDragCorrect = PlayerPrefs.GetString("IsDragCorrect");
        PlayerPrefs.SetInt("DragAndDropCount", getDragAndDropCount+1);
        getDragAndDropCount = PlayerPrefs.GetInt("DragAndDropCount");
        if (getDragAndDropCount == 4)
        {
            if (getIsDragCorrect == "T")
            {
                Debug.Log("Correct all dasdasdasdas");

                int dragAndDropScore = PlayerPrefs.GetInt("DragAndDropScore");
                PlayerPrefs.SetInt("DragAndDropScore", dragAndDropScore+1);
                GameObject.Find("ScoreText").GetComponent<Text>().text = "Score: " + (dragAndDropScore+1);
                //players[currentPlayer].score = Mathf.Lerp(players[currentPlayer].score, players[currentPlayer].scoreCount, Time.deltaTime * 10);
                //players[currentPlayer].score = Mathf.CeilToInt(players[currentPlayer].score);
                print(players[currentPlayer].score + " --- " + players[currentPlayer].scoreCount);



                if (bonusObject && bonusObject.GetComponent<Animation>())
                {
                    // Animate the bonus object
                    bonusObject.GetComponent<Animation>().Play();

                    // Reset the bonus animation
                    bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = 1;
                }
            }
            else
            {
                Debug.Log("Wrong!!!");
            }
            Debug.Log("Reset question");
            int getDragAndDropCurrentCount = PlayerPrefs.GetInt("DragAndDropCurrentCount", 0);
            PlayerPrefs.SetInt("DragAndDropCurrentCount", getDragAndDropCurrentCount + 1);
            StartCoroutine(QuestionFlip(0.5f));
        }
    }
}
