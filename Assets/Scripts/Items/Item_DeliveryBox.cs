using System;
using System.Collections.Generic;
using UnityEngine;
using static Alexdev.TweenUtils;

[RequireComponent(typeof(BoxCollider))]
public class Item_DeliveryBox : Item_Base
{
    public static event Action OnBoxOpened;



    [SerializeField] private ParticleSystem particle;
    // ─────────── CONFIG ───────────
    [SerializeField] private List<Item_Base> containedItems;// = new ();

    //private List<GameObject> containedItems =   // leave empty → auto-fill from children
    [SerializeField] private LayerMask floorMask = ~0;               // default: everything
    [SerializeField] private float lift = 0.05f;                     // tiny offset to avoid z-fighting
    // ──────────────────────────────

    [Space]
    [Header("Unpack details")]
    [SerializeField] private float shakeDur;
    [SerializeField] private Vector3 shakeStr;
    private bool unpacked = false;
    private Coroutine scaleCo;

    [Header("Audio SFX Details")]
    [SerializeField] private string regularImpact = "deliveryBox_impact";
    [SerializeField] private string onMetapImpact = "deliveryBox_metal_impact";
    private bool isFirstrSound = true;

    protected override void OnItemImpact(Collision collision)
    {
        if (isFirstrSound)
        {
            Audio.PlaySFXFar(onMetapImpact, transform.position, new Vector2(2.5f, 6f));
            isFirstrSound = false;
            return;
        }


            
        bool isMetal = collision.transform.GetComponent<MetalIdentifier>() != null;

        if (isMetal)
            Audio.PlaySFXFar(onMetapImpact, transform.position, new Vector2(2f, 6f));
        else
            Audio.PlaySFX(regularImpact, transform);
    }



    public override void Interact(Transform caller)
    {
        if (particle != null)
        {
            particle.transform.parent = null;
            particle.gameObject.SetActive(true); // enable particle system
        }


        if (player.interaction.QuickPressLMB())
        {
            Unpack();
        }
        else if (itemCanBePickedUp)
        {
            base.Interact(caller); // hold — pickup
        }

    }

    public void SetupBox(List<Item_Base> items)
    {
        if (items == null || items.Count == 0)
            return;

        containedItems = items ?? new List<Item_Base>();
        unpacked = false;
        Debug.Log("list was set");
    }

    public void ScaleUp(float maxScale, float scaleSpeed)
    {
        float targetScale = .8f;
        float finalScale = Mathf.Clamp(targetScale, targetScale, maxScale);

        if (scaleCo != null)
            StopCoroutine(scaleCo);

        scaleCo = StartCoroutine(ScaleLocal(transform, Vector3.one * finalScale, scaleSpeed));
    }

    private void Unpack()
    {

        OnBoxOpened?.Invoke();
        EnableCollider(false);
        UI.instance.taskIndicator.RemoveTarget(transform);
        Audio.PlaySFX("deliveryBox_open", transform);

        bool needCameraShake = false;

        // Crate footprint (ignore tilt)
        var col = GetComponent<BoxCollider>();
        var centre = transform.position;
        var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        float width = col.size.x * transform.lossyScale.x;
        float depth = col.size.z * transform.lossyScale.z;

        // Square-ish grid
        int n = containedItems.Count;
        int perRow = Mathf.CeilToInt(Mathf.Sqrt(n));
        float stepX = width / perRow;
        float stepZ = depth / perRow;
        int index = 0;

        for (int z = 0; z < perRow && index < n; z++)
        {
            for (int x = 0; x < perRow && index < n; x++)
            {
                // local offset centred on crate
                var local = new Vector3(
                    (x + 0.5f) * stepX - width * 0.5f,
                    0,
                    (z + 0.5f) * stepZ - depth * 0.5f);

                // convert to world space
                var pos = centre + right * local.x + forward * local.z;

                // drop to the floor
                if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out var hit, 5f, floorMask))
                    pos = hit.point;

                if (containedItems.Count > 0)
                {
                    var item = containedItems[index++];
                    item.transform.SetParent(null, true);     // detach
                    item.transform.position = pos + Vector3.up * lift;
                    item.transform.rotation = Quaternion.identity;
                    item.gameObject.SetActive(true);          // in case it was hidden
                    item.OnItemUnpack();


                    if (item.GetItemWeightType() == ItemWeightType.Heavy)
                        needCameraShake = true;
                }
            }
        }

        unpacked = true;

        if (needCameraShake)
            Player.instance.cameraEffects.Shake(shakeDur, shakeStr);


        ItemManager.instance.DestroyItem(this);
    }

    public override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);
        if (enable)
        {
            UI.instance.inGameUI.inputHelp.AddInput(KeyType.LMB, "input_delivery_box_open",true);

            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }
        }
        else
            UI.instance.inGameUI.inputHelp.RemoveInput();
    }

    public List<Item_Base> GetContainedItems() => containedItems;
}
