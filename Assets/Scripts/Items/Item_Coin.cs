using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Item_Coin : Item_Base, IInteractable
{
    public static event Action OnCoinStamped;

    [Header("Stamp Details")]
    [SerializeField] private Material noStampMat;
    [SerializeField] private Material hasStampMat;
    [SerializeField] private GameObject onStampVfx;
    [SerializeField] private AudioSource sfx;

    [Header("Coin Value")]
    [SerializeField] private int creditValue = 1;
    [SerializeField] private bool stampedByDefault;
    public bool hasStamp { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        EnableStamps(stampedByDefault);
    }

    protected override void OnItemImpact(Collision collision)
    {
        base.OnItemImpact(collision);
        Audio.PlaySFX("coin_impact", transform);
    }

    public int GetCoinValue() => creditValue;

    

    public void PlayFeedback(Vector3 velocity, float rotation)
    {
        rb.linearVelocity = velocity;
        rb.AddTorque(new Vector3(0, 0, rotation), ForceMode.VelocityChange);

        GameObject newVfx = Instantiate(onStampVfx, transform.position, Quaternion.identity);//,transform);
        //Audio.PlaySFX("coin_stamp", transform.position);
        //if(sfx != null) 
        //    sfx.Play();
    }

    public void EnableStamps(bool enable = true)
    {
        if (hasStamp && enable)
            return;

        if (hasStamp == false && enable == false)
            return;

        Material stampMaterial = enable ? hasStampMat : noStampMat;
        meshRenderer.material = new Material(stampMaterial);
        hasStamp = enable;
        HideIndicator(true);


        if (true)
            OnCoinStamped?.Invoke();
    }


    public ItemHolder GetItemHolder() => currentItemHolder;
    public Rigidbody GetRb() => rb;

    public Quaternion GetFacingRotation()
    {
        // Build a mask for ground layer (important: LayerMask.NameToLayer gives an index, not a mask)
        int groundLayer = LayerMask.NameToLayer("Ground");
        int mask = 1 << groundLayer;

        // Raycast along local up axis
        bool detectedGround = Physics.Raycast(transform.position, transform.up, Mathf.Infinity, mask);

        // Base coin rotation
        Quaternion coinRot = transform.rotation;

        if (!detectedGround)
        {
            // Heads up → return as is
            return coinRot;
        }
        else
        {
            // Tails up → flip 180° around local X
            return coinRot * Quaternion.AngleAxis(180f, transform.right);
        }
    }
    public bool IsFlying()
    {
        float velocityThreshold = 0.1f;

        // Check movement (any direction, not just Y)
        bool isMoving = rb.linearVelocity.magnitude > velocityThreshold;

        // Ground check
        int groundLayer = LayerMask.NameToLayer("Ground");
        int mask = 1 << groundLayer;

        bool isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            0.1f,
            mask
        );

        return isMoving && !isGrounded;
    }

    public void TryCreateCollectable()
    {
        if (IsFlying() == false)
            return;


        GameObject newCollectable = null;

        if (Collectable_Manager.instance.CanCreateCollectableOfType(CollectableCoinType.Octopus, out newCollectable))
        {
            if (newCollectable == null)
                return;

            Item_Base item = newCollectable.GetComponent<Item_Base>();
            float randomFeedbackRotation = Random.Range(10, 20);

            item.transform.position = transform.position;
            item.transform.rotation = transform.rotation;
            item.transform.parent = null;
            item.gameObject.SetActive(true);
            item.SetVelocity(new Vector3 (0, 3.5f, 0));
            item.SetTorque(new Vector3(0, 0, randomFeedbackRotation));
        }

    }

    public override void OnItemPickup()
    {
        base.OnItemPickup();

        if (currentItemHolder != null)
            currentItemHolder.RemoveItem(this);
    }
}
