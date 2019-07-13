using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slots : MonoBehaviour, IDropHandler
{
    List<string> questionAnswerList = new List<string>();
    string dasdsa = "dasdas";

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
    static void ReadString(string answer, string index)
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
            GameObject.Find("DragAndDropObject/ButtonAnswer" + index + "/" + index).GetComponent<Text>().text = answerArray[0].Trim() + " - " + answerArray[1].Trim();
        }
    }
}
