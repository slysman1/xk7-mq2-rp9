using System.Collections;
using UnityEngine;

public class FocusArea : MonoBehaviour , IHoverable
{
    public bool hovered;// { get; private set; }

    public virtual void OnHoverEnter()
    {
        hovered = true;
    }

    public virtual void OnHoverExit()
    {
        hovered= false;
    }

    public void DisableCollider()
    {
        GetComponent<Collider>().enabled = false;
    }
}
