using UnityEngine;

//[RequireComponent(typeof(Object_Outline))]
public class Interaction_Button : MonoBehaviour, IInteractable, IHighlightable
{
    protected Object_Outline outline;
    protected Coroutine interactionCo;

    protected virtual void Awake()
    {
        outline = GetComponent<Object_Outline>();
    }

    public virtual void Highlight(bool enable)
    {
        outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);
    }

    public virtual void Interact(Transform caller = null)
    {
        
    }

    public virtual void SeconderyInteraction(Transform caller = null)
    {
        //throw new System.NotImplementedException();
    }


    protected virtual void ShowInputUI(bool enable)
    {

    }
}
