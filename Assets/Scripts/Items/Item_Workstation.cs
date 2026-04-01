using UnityEngine;


public class Item_Workstation : Item_Base
{
    protected Workstation workstation;
    protected Item_Base itemInHand
    {
       get
        {
            _itemInHand = inventory.GetTopItem();

            return _itemInHand;
        }
    }
    protected Item_Base _itemInHand;


    [Header("Mesh Convex Update")]
    [SerializeField] private bool updateMeshesConvex;
    [SerializeField] private MeshCollider[] meshColliders;



    protected override void Start()
    {
        base.Start();
        workstation = GetComponent<Workstation>();
        EnableKinematic(true);
    }
    public override void Interact(Transform caller = null)
    {
        if (player.interaction.QuickPressLMB())
        {
            if (workstation != null && workstation.CanBeExecuted())
                workstation.ExecuteInteraction(caller);
            // quick click but can't execute — do nothing
        }
        else if (itemCanBePickedUp)
        {
            base.Interact(caller); // hold — pickup
        }
    }


    protected override void OnItemImpact(Collision collision)
    {

        if (collision.collider.GetComponent<Player>() != null)
            return;

        if (itemData.kinematicOnImpact == KinematicOnImpact.True)
        {

            Audio.PlaySFX("item_default_impact", transform);
            Invoke(nameof(FixateWorkstation), .5f);
        }
    }

    private void FixateWorkstation()
    {
        EnableKinematic(true);
        EnableCamPriority(false);
    }
}
