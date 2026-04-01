using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Delivery_ItemDeliveryAnim : MonoBehaviour
{
    [Header("Item Delivery Sttings")]
    [SerializeField] private float deliverySpeed = 8f;
    [SerializeField] private Order_WhirlAnimationWaypoint[] deliveryWaypoints;
    [SerializeField] private float deliveryRejectGravity = 2;


    private void Start()
    {
        deliveryWaypoints = GetComponentsInChildren<Order_WhirlAnimationWaypoint>();
    }
    public void ResetItem(Item_Base item)
    {
        item.EnableKinematic(false);
        item.SetVelocity(Vector3.down * deliveryRejectGravity);
    }

    public void DeliverItem(Item_Base item, DeliveryAreaHolder_AllItems deliveryArea)
    {
        StartCoroutine(DeliverItemCo(item, deliveryArea));
    }

    public IEnumerator DeliverItemCo(Item_Base item,DeliveryAreaHolder_AllItems deliveryArea)
    {
        float scaleDuration = 0.25f;

        for (int i = 0; i < deliveryWaypoints.Length; i++)
        {
            if (i == deliveryWaypoints.Length - 2)
                StartCoroutine(ScaleLocal(item.transform, Vector3.zero, scaleDuration));

            Transform waypoint = deliveryWaypoints[i].transform;

            float t = 0f;
            Vector3 start = item.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime * deliverySpeed;
                item.transform.position = Vector3.Lerp(start, waypoint.position, t);
                yield return null;
            }
        }

        deliveryArea.RemoveItem(item);
        Audio.PlaySFX("delivery_door_accept_item", item.transform);
        ItemManager.instance.DestroyItem(item);
    }
}
