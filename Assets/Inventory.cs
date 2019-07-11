using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour, IHasChanged
{
    [SerializeField] Transform slots;
    [SerializeField] Text inventoryText;

    private void Start()
    {
        HasChanged();
    }

    public void HasChanged()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(" - ");
        foreach (Transform slotTransform in slots)
        {
            GameObject item = slotTransform.GetComponent<Slots>().item;
            if (item)
            {
                builder.Append(item.name);
                builder.Append(" - ");
            }
        }

        inventoryText.text = builder.ToString();
    }
}


namespace UnityEngine.EventSystems
{
    public interface IHasChanged : IEventSystemHandler
    {
        void HasChanged();
    }
}