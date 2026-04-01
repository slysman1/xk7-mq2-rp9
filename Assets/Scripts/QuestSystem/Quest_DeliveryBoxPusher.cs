using System.Collections.Generic;
using UnityEngine;

public class Quest_DeliveryBoxPusher : MonoBehaviour
{
    [SerializeField] private float pushPower = 2f;
    [SerializeField] private float repushDelay = 1f;
    [SerializeField] private bool initialPush = true;

    private Dictionary<Item_Base, float> stayTimer = new();

    private void OnTriggerEnter(Collider other)
    {
        if (initialPush == false)
            return;

        Item_Base item = other.GetComponent<Item_Base>();

        if (item == null) 
            return;

        
        item.SetVelocity(transform.forward * pushPower);
        stayTimer[item] = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        Item_Base item = other.GetComponent<Item_Base>();

        if (item == null)
            return;

        if (item.rb.isKinematic)
            item.EnableKinematic(false);

        if (!stayTimer.ContainsKey(item))
            stayTimer[item] = 0f;

        stayTimer[item] += Time.deltaTime;

        if (stayTimer[item] >= repushDelay)
        {
            item.SetVelocity(transform.forward * pushPower);
            stayTimer[item] = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Item_Base item = other.GetComponent<Item_Base>();

        if (item == null)
            return;

        stayTimer.Remove(item);
    }
}
