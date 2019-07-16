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

        // Next question
    }

    void Update()
    {
        if (currentPlayer < players.Length)
        {
            // Move the players object so that the current player is centered in the screen
            if (players[currentPlayer].nameText && bonusObject.position.x != players[currentPlayer].nameText.transform.position.x)
            {
                playersObject.anchoredPosition = new Vector2(Mathf.Lerp(playersObject.anchoredPosition.x, currentPlayer * -200 - 100, Time.deltaTime * 10), playersObject.anchoredPosition.y);
            }

            // Make the score count up to its current value, for the current player
            if (players[currentPlayer].score < players[currentPlayer].scoreCount)
            {
                // Count up to the courrent value
                players[currentPlayer].score = Mathf.Lerp(players[currentPlayer].score, players[currentPlayer].scoreCount, Time.deltaTime * 10);

                // Round up the score value
                players[currentPlayer].score = Mathf.CeilToInt(players[currentPlayer].score);

                // Update the score text
                UpdateScore();
            }

            // Update the lives bar
            if (players[currentPlayer].livesBar)
            {
                // If the lives bar has a text in it, update it. Otherwise, resize the lives bar based on the number of lives left
                if (players[currentPlayer].livesBar.transform.Find("Text")) players[currentPlayer].livesBar.transform.Find("Text").GetComponent<Text>().text = players[currentPlayer].lives.ToString();
                else players[currentPlayer].livesBar.rectTransform.sizeDelta = Vector2.Lerp(players[currentPlayer].livesBar.rectTransform.sizeDelta, new Vector2(players[currentPlayer].lives * livesBarWidth, players[currentPlayer].livesBar.rectTransform.sizeDelta.y), Time.deltaTime * 8);
            }
        }
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
            string answer = gameObject.GetComponentInChildren<Text>().text + " --- " + PlayerPrefs.GetString("OnBeginDrag");
            ReadString(answer.Trim(), gameObject.name);
            DragHandeler.itemBeingDragged.transform.SetParent(transform);
            ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
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

            //GameObject.GetComponent<TQGGameController>();

            //StartCoroutine(AskQuestion(true));

            //StartCoroutine(Camera.main.GetComponent<TQGGameController>().AskQuestion(true));
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
                players[currentPlayer].scoreCount += 1;


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
            StartCoroutine(QuestionFlip(0.5f));
        }
    }
}
