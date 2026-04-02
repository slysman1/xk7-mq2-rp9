using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static Alexdev.TweenUtils;

public class Item_Coin : Item_Base, IInteractable
{
    public static event Action OnCoinStamped;

    [SerializeField] private bool stampedByDefault;
    [Header("Stamp Details")]
    [SerializeField] private Material noStampMat;
    [SerializeField] private Material hasStampMat;
    [SerializeField] private GameObject onStampVfx;
    [SerializeField] private AudioSource sfx;


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

    public int GetCoinValue() => itemData.creditValue;

    

    public void OnStampFeedback(Vector3 feedbackVelocity, float minTorque, float maxTorque)
    {
        Vector3 velocity = GetWorldVelocity(feedbackVelocity);
        float torq = Random.Range(minTorque, maxTorque);

        if (Physics.CheckSphere(transform.position, 0.05f))
            transform.position += Vector3.up * 0.05f; // nudge up slightly


        rb.linearVelocity = velocity;
        rb.AddTorque(new Vector3(0, 0, torq), ForceMode.VelocityChange);

        GameObject newVfx = Instantiate(onStampVfx, transform.position, Quaternion.identity);
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
