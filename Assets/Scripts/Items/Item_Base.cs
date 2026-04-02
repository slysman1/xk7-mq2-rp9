using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public enum PlacementType { Anywhere, WallOnly }
public enum ItemWeightType { Light, Medium, Heavy, None }
public enum KinematicOnImpact { False, True }

//[RequireComponent(typeof(Object_Outline))]
[RequireComponent(typeof(Rigidbody))]
public class Item_Base : MonoBehaviour, IInteractable, IHighlightable
{
    public event Action OnItemHighlighted;
    public event Action OnItemPickedUp;
    public event Action OnItemDroppedEvent;

    public Object_Outline outline { get; private set; }
    public HeatEmission heatHandler { get; private set; }
    public Rigidbody rb { get; private set; }
    protected Collider[] colliders;
    private Dictionary<MeshCollider, bool> meshConvexStates = new Dictionary<MeshCollider, bool>();

    protected Player player
    {
        get
        {
            if (_player == null)
                _player = FindFirstObjectByType<Player>();

            return _player;
        }
    }
    private Player _player;

    protected Player_Inventory inventory;
    protected Mesh mesh;
    protected MeshRenderer meshRenderer;
    protected MeshFilter meshFilter;
    protected UI_OnObjectIndicator objectIndicator;
    protected UI_InputHelp inputHelp => UI.instance.inputHelp;
    public ItemHolder currentItemHolder { get; private set; }

    public bool blockOutline;
    [SerializeField] protected bool itemCanBePickedUp = true;
    public ItemDataSO itemData;




    [Header("Audio Touch/Drop Impact")]
    [SerializeField] private string imapctPersonalSound;
    [SerializeField] private float minImpactVelocity = 0.25f;
    [SerializeField] private float impactSoundCooldown = 0.25f;
    private float lastPlayTime;


    private string inHolderLayer = "InteractableInHolder";
    private string cameraLayer = "ItemInHand";
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();


    protected virtual void Awake()
    {
        outline = GetComponent<Object_Outline>();
        rb = GetComponentInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        mesh = GetComponentInChildren<MeshFilter>(true).mesh;
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshFilter = GetComponentInChildren<MeshFilter>();
        heatHandler = GetComponent<HeatEmission>();

        CacheOriginalLayers();
        CacheMeshConvexState();
    }



    protected virtual void Start()
    {
        inventory = player.inventory;
    }

    public virtual void Interact(Transform carryPoint)
    {
        inventory.TryPickup(this);
    }


    public virtual bool CanBePickedUp() => itemCanBePickedUp;
    public void SetCanPickUpTo(bool canPickup) => itemCanBePickedUp = canPickup;

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }
    public void SetTorque(Vector3 torq)
    {
        rb.AddTorque(torq, ForceMode.VelocityChange);
    }
    public void EnableCollider(bool enable)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (var col in colliders)
            col.enabled = enable;
    }
    public virtual void EnableKinematic(bool isKinematic)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = isKinematic;

        EnableConvex(!isKinematic); // dynamic = needs convex, kinematic = restore original
    }

    public virtual void EnableCamPriority(bool enable)
    {
        if (enable)
        {
            foreach (var kvp in originalLayers)
                kvp.Key.layer = LayerMask.NameToLayer(cameraLayer);
        }
        else
            EnableOriginalLayers();
    }

    public virtual void SeconderyInteraction(Transform caller = null) { }

    public void SetItemHolder(ItemHolder holder)
    {
        if (holder == null && currentItemHolder != null)
        {
            var prev = currentItemHolder;
            currentItemHolder = null; // clear first to break the loop
            prev.RemoveItem(this);
        }
        else
        {
            currentItemHolder = holder;
        }
    }



    public virtual void OnItemPickup()
    {
        EnableCamPriority(true);
        EnableConvex(true);

        // De-tach from holder
        if (currentItemHolder != null)
            currentItemHolder.RemoveItem(this);

        Audio.PlaySFX("item_default_pickup", transform);
        HideIndicator(true);
        OnItemPickedUp?.Invoke();

    }

    private void EnableConvex(bool enable)
    {
        if (enable)
        {
            foreach (var pair in meshConvexStates)
                pair.Key.convex = true;
        }
        else
        {
            foreach (var pair in meshConvexStates)
                pair.Key.convex = pair.Value;
        }
    }

    public void EnableInHolderLayer(bool inHolder)
    {
        if (inHolder)
        {
            foreach (var kvp in originalLayers)
                kvp.Key.layer = LayerMask.NameToLayer(inHolderLayer);
        }
        else
            EnableOriginalLayers();
    }

    public void EnableOriginalLayers()
    {
        foreach (var kvp in originalLayers)
            kvp.Key.layer = kvp.Value;
    }

    public virtual void OnItemDrop()
    {
        EnableCamPriority(false);
        EnableCollider(true);
        HideIndicator(false);
        OnItemDroppedEvent?.Invoke();
    }

    public virtual void OnItemBeingPlaced(Vector3 placementPosition)
    {

    }

    public virtual void OnItemUnpack()
    {
        EnableKinematic(false);
        transform.localScale = Vector3.one;
    }


    public virtual void Highlight(bool enable)
    {

        ShowInputUI(enable);

        if (outline == null)
            return;

        if (enable && blockOutline)
        {
            outline.EnableOutline(OutlineType.None);
            return;
        }


        if (enable)
            OnItemHighlighted?.Invoke();

        outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastPlayTime < impactSoundCooldown)
            return;

        float impactForce = collision.relativeVelocity.magnitude;
        float velocity = rb.linearVelocity.magnitude;

        if (impactForce < minImpactVelocity)
            return;

        if (velocity <= 0)
            return;

        OnItemImpact(collision);
        lastPlayTime = Time.time;
    }

    private void OnCollisionStay(Collision collision)
    {
        TryPlayImpact(collision);
    }

    void TryPlayImpact(Collision collision)
    {

        if (Time.time - lastPlayTime < impactSoundCooldown)
            return;

        if (rb.linearVelocity.magnitude <= 0)
            return;

        if (collision.relativeVelocity.magnitude > minImpactVelocity)
        {
            lastPlayTime = Time.time;
            OnItemImpact(collision);
        }
    }

    protected virtual void OnItemImpact(Collision collision)
    {
        Audio.PlaySFX("item_default_impact", transform);

        if (itemData.kinematicOnImpact == KinematicOnImpact.True && collision.collider.GetComponent<Player>() == null)
        {
            if (rb.linearVelocity.magnitude <= 0.1f)
                EnableKinematic(true);
        }
    }

    protected void HideIndicator(bool hide)
    {
        if (objectIndicator == null)
            objectIndicator = GetComponentInChildren<UI_OnObjectIndicator>();

        if (objectIndicator != null)
            objectIndicator.HideIndicator(hide);
    }

    public virtual void ShowInputUI(bool enable)
    {

        if (enable)
        {
            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }
        }
        else
            inputHelp.RemoveInput();
    }

    public void PauseCollider(float pauseDuration = 0)
    {
        StartCoroutine(PauseColliderCo(pauseDuration));
    }
    private IEnumerator PauseColliderCo(float pauseDuration = 0)
    {
        EnableCollider(false);
        yield return new WaitForSeconds(pauseDuration);

        EnableCollider(true);
    }




    public string GetItemId() => itemData.itemId;


    public Collider GetCollider() => colliders.Length > 0 ? colliders[0] : null;




    protected void CacheOriginalLayers()  // was private
    {
        originalLayers.Clear();

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (originalLayers.ContainsKey(t.gameObject) == false)
                originalLayers.Add(t.gameObject, t.gameObject.layer);
    }

    private void CacheMeshConvexState()
    {
        foreach (var col in GetComponentsInChildren<MeshCollider>())
            meshConvexStates[col] = col.convex;
    }

    protected virtual bool IsStandingStraight()
    {
        return Vector3.Angle(transform.up, Vector3.up) < 5f; // tolerance in degrees
    }

    public virtual void EnableAsItWereInHolder(bool display)
    {
        EnableCamPriority(!display);
    }

    public virtual int GetMaxStack() => itemData.maxStackInHand;
    public float GetStackYOffset() => itemData.itemStackYoffset;
    public Vector3 GetInHandPosition() => itemData.inHandPosition;
    public Vector3 GetInHandRotation() => itemData.inHandRotation;
    public ItemWeightType GetItemWeightType() => itemData.weightType;
    public bool CanStackWith(Item_Base item, int inHandCount)
    {
        return inHandCount < GetMaxStack() && itemData.canStackWith.Contains(item.itemData);
    }




#if UNITY_EDITOR
    [ContextMenu("Copy Position&Rotation In Hand")]
    public void UpdateInHandPostionAndRotation()
    {
        itemData.inHandPosition = transform.localPosition;

        SerializedObject so = new SerializedObject(transform);
        SerializedProperty rot = so.FindProperty("m_LocalEulerAnglesHint");
        itemData.inHandRotation = rot.vector3Value;

        UnityEditor.EditorUtility.SetDirty(itemData);
    }

    public void AssignItemData(ItemDataSO itemData) => this.itemData = itemData;
#endif
}
