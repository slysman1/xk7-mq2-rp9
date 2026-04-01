using UnityEngine;

public class Item_DummyBucket : Item_Base
{
    [SerializeField] private Material happyFace;
    [SerializeField] private Material sadFace;

    public void SendBucketFlying(Vector3 velocity)
    {
        if(currentItemHolder != null)
            currentItemHolder.RemoveItem(this);

        transform.parent = null;
        EnableKinematic(false);
        PauseCollider(1.2f);
        SetVelocity(velocity);
        EnableHappyFace(false);
    }

    public void EnableHappyFace(bool enable)
    {
        meshRenderer.material = enable ? happyFace : sadFace;
    }

    public override void OnItemPickup()
    {
        base.OnItemPickup();
        EnableHappyFace(false);
    }
}
