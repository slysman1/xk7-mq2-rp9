using System.Net.NetworkInformation;
using UnityEngine;

public class CrackedWall_Brick : MonoBehaviour
{
    private EmissionCycle emissionCycle;
    private Rigidbody rb;

    public bool canBePushed;
    public FocusPoint focusArea;


    private void Awake()
    {
        focusArea = GetComponentInChildren<FocusPoint>(true);
        emissionCycle = GetComponentInChildren<EmissionCycle>(true);
        rb = GetComponent<Rigidbody>();
    }

    public void TryToPush(float pushVelocity)
    {
        if (canBePushed)
        {
            if (focusArea != null && focusArea.hovered == false)
                return;

            if(focusArea != null)
                focusArea.DisableCollider();

            rb.isKinematic = false;

            rb.AddForce(transform.forward * pushVelocity, ForceMode.VelocityChange);

        }
    }

    public void Collapse()
    {
        canBePushed = true;
        rb.isKinematic = false;
        rb.AddForce(-transform.forward * 2f, ForceMode.VelocityChange);
        rb.AddForce(-transform.up * 1.5f, ForceMode.VelocityChange);
    }

    public void Highlight(bool enable)
    {
        if(canBePushed && focusArea != null)
            focusArea.gameObject.SetActive(enable);
    }

    public void EnableBrick(bool enable)
    {
        canBePushed = enable;
        emissionCycle.EnableEmissionCycle(enable);
    }

}
