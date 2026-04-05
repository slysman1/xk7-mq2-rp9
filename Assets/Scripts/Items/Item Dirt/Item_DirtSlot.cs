using UnityEngine;

public class Item_DirtSlot : MonoBehaviour
{
    
    protected MeshFilter meshFilter => GetComponent<MeshFilter>();
    protected MeshRenderer meshRenderer => GetComponent<MeshRenderer>();


    public void HideSlot()
    {
        meshRenderer.enabled = false;
        gameObject.SetActive(false);
    }
    public void ShowSlot()
    {
        gameObject.SetActive(true);
        meshRenderer.enabled = false; // Keep mesh hidden but slot active
    }

    public DirtDetails GetDetails()
    {
        return new DirtDetails(meshFilter.mesh, transform.position, transform.rotation);
    }
}

public class DirtDetails
{
    public Vector3 position;
    public Quaternion rotation;
    public Mesh mesh;

    public DirtDetails(Mesh mesh, Vector3 position, Quaternion rotation)
    {
        this.mesh = mesh;
        this.position = position;
        this.rotation = rotation;
    }
}