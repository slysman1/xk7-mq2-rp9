using UnityEngine;

public class Quest_DeliveryBoxScaler : MonoBehaviour
{
    [SerializeField] private float scaleSpeed = 1f;
    [SerializeField] private float maxScale = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Item_DeliveryBox deliveryBox = other.GetComponent<Item_DeliveryBox>();

        if(deliveryBox != null )
            deliveryBox.ScaleUp(maxScale, scaleSpeed);
    }
}
