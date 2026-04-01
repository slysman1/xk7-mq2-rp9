using UnityEngine;

public class FocusPoint : MonoBehaviour, IHoverable
{
    public bool hovered;
    private EmissionCycle emissionCycle;

    private void Awake()
    {
        emissionCycle = GetComponent<EmissionCycle>();
    }

    private void Start()
    {
        emissionCycle?.EnableEmissionCycle(true);
    }

    public virtual void OnHoverEnter()
    {
        hovered = true;
        emissionCycle?.TransitionToPeak();
    }

    public virtual void OnHoverExit()
    {
        hovered = false;
        emissionCycle?.EnableEmissionCycle(true);
    }

    public void DisableCollider()
    {
        GetComponent<Collider>().enabled = false;
    }
}