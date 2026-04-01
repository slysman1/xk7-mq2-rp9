using System.Collections;
using UnityEngine;

public class Holder_ItemRejecter : MonoBehaviour
{
    [SerializeField] private ItemHolder holder;   // assign your CoinPress_TemplateHolder
    [SerializeField] private BoxCollider checkArea; // assign a box collider (set to trigger)
    [SerializeField] private float rejectWaitTime = 0.25f;


    public IEnumerator RejectItemsIfNeededCo()
    {
        if (holder == null || checkArea == null)
            yield break;

        bool rejectedSomething = false;

        Collider[] hits = Physics.OverlapBox(
            checkArea.bounds.center,
            checkArea.bounds.extents,
            checkArea.transform.rotation
        );

        foreach (Collider col in hits)
        {
            // Skip the holder’s own colliders
            if (col.transform.IsChildOf(transform.root))
                continue;

            Item_Base item = col.GetComponentInParent<Item_Base>();
            if (item == null) continue;

            if (holder.GetCurrentItems().Contains(item) == false)
            {
                rejectedSomething = true;
                holder.RejectItem(item);
            }
        }

        // If nothing rejected -> finish instantly
        if (!rejectedSomething)
            yield break;

        // Otherwise wait for physics to settle
        yield return new WaitForSeconds(rejectWaitTime);
    }

#if UNITY_EDITOR
    // Draw the overlap box in editor
    private void OnDrawGizmosSelected()
    {
        if (checkArea == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = checkArea.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(checkArea.center, checkArea.size);
    }
#endif
}
