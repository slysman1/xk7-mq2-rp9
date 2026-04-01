using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_PreviewObject : MonoBehaviour
{
    private Player_PreviewHandler previewHandler;
    private BoxCollider boxCollider;
    private Player player;

    private Item_Base itemBeingPlaced;
    [SerializeField] private float previewSmoothnes;

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private Material allowedPlacementMat;
    [SerializeField] private Material notAllowedPlacemntMat;


    [SerializeField] private LayerMask whatToIgnore;
    [SerializeField] private LayerMask whatIsWall;

    private List<Collider> collidersInContact = new List<Collider>();


    private void Awake()
    {
        player = GetComponentInParent<Player>();
        previewHandler = GetComponentInParent<Player_PreviewHandler>();
    }

    public void EnablePreviw(Item_Base itemBeingPlaced,bool visible = true)
    {
        this.itemBeingPlaced = itemBeingPlaced;
        //meshFilter.sharedMesh = itemBeingPlaced.GetPreviwMesh();
        transform.localScale = Vector3.one;

        gameObject.SetActive(visible);

        if (boxCollider != null)
            Destroy(boxCollider);

        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }


    public bool ItemCanBePlaced()
    {
        if (itemBeingPlaced.itemData.placementType == PlacementType.WallOnly)
        {
            return player.raycaster.HitWall(out _) && NoBlockingContacts();
        }

        else if (collidersInContact.Count == 0)
        {
            return true;
        }

        return false;
    }

    private bool NoBlockingContacts()
    {
        foreach (var col in collidersInContact)
        {
            int colLayer = col.gameObject.layer;

            // If it's NOT in the ignored layer mask → blocking
            if ((whatIsWall.value & (1 << colLayer)) == 0)
                return false;
        }

        return true; // all contacts are on ignored layers
    }


    private void UpdatePreviwColor()
    {
        meshRenderer.material = ItemCanBePlaced() ? allowedPlacementMat : notAllowedPlacemntMat;
    }

  

    public void DisablePreview()
    {
        gameObject.SetActive(false);
        collidersInContact.Clear();
    }

    public void SetPosition(Vector3 pos)
    {
        //transform.position = pos;

        //transform.position = Vector3.Lerp(transform.position, pos, 15f * Time.deltaTime);

        float smoothSpeed = previewSmoothnes;
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, pos, t);
    }
    public void SetRotation(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        transform.rotation = Quaternion.Euler(euler);
    }


    private IEnumerator ContactCheckCo()
    {
        while (true)
        {
            UpdateContacts();
            yield return new WaitForSeconds(.2f);
        }
    }

    private void UpdateContacts()
    {
        collidersInContact.Clear();
        if (boxCollider == null) return;

        // World-space oriented box
        Vector3 worldCenter = boxCollider.transform.TransformPoint(boxCollider.center);
        Vector3 worldHalfExts = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) * 0.5f;
        Quaternion worldRot = boxCollider.transform.rotation;

        // Query
        Collider[] hits = Physics.OverlapBox(worldCenter, worldHalfExts, worldRot); //, contactMask);//,QueryTriggerInteraction.Ignore);

        foreach (var hit in hits)
        {
            if (hit == null) 
                continue;

            if (ReferenceEquals(hit, boxCollider))
                continue;      // ignore self

            if (ShouldBeIgnored(hit))
                continue;               

            // your custom ignores
            collidersInContact.Add(hit);
        }

        UpdatePreviwColor();
    }

    private void OnEnable()
    {
        StartCoroutine(ContactCheckCo());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private bool ShouldBeIgnored(Collider other)
    {
        if (other.isTrigger)
            return true;

        if (((1 << other.gameObject.layer) & whatToIgnore) != 0)
            return true;

        if (other.transform.IsChildOf(transform))
            return true;

        return false;
    }
}
