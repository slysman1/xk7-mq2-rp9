using System.Collections;
using UnityEngine;


public class Item_Dirt : Item_Base
{

    protected virtual IEnumerator CleanDirtCo()
    {
        yield return null;
    }

    public override bool CanBePickedUp()
    {
        return false;
    }

    public override void ShowInputUI(bool enable)
    {

    }

    public virtual void SetupDirt(DirtDetails details)
    {
        transform.position = details.position;
        transform.rotation = details.rotation;
        meshFilter.mesh = details.mesh;
    }
}
