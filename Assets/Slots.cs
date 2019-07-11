using System.Collections.Generic;
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
            DragHandeler.itemBeingDragged.transform.SetParent(transform);
            ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
        }
    }
}
