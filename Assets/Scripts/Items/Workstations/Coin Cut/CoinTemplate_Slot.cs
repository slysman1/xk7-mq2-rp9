using UnityEngine;

public class CoinTemplate_Slot : MonoBehaviour
{
    public void SetupPlaceHolder(Material newMaterial)
    {
        GetComponentInChildren<MeshRenderer>().material = newMaterial;    
    }
}
