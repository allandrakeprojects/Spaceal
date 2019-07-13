using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandeler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    Transform startParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameObject.Find("Question/Text").GetComponent<Text>().text.Contains("Drag"))
        {
            PlayerPrefs.SetString("OnBeginDrag", gameObject.GetComponentInChildren<Text>().text);
            PlayerPrefs.Save();

            itemBeingDragged = gameObject;
            startPosition = transform.position;
            startParent = transform.parent;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameObject.Find("Question/Text").GetComponent<Text>().text.Contains("Drag"))
        {
            transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (GameObject.Find("Question/Text").GetComponent<Text>().text.Contains("Drag"))
        {
            itemBeingDragged = null;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            transform.position = startPosition;
            //if (transform.parent != startParent)
            //{
            //    transform.position = startPosition;
            //}
        }
    }
}
