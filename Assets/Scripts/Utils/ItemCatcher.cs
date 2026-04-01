using UnityEngine;

public class ItemCatcher : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.transform.position = new Vector3(other.transform.position.x, 2, other.transform.position.z);
    }
}
