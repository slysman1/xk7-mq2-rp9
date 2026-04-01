using UnityEngine;

public interface IInteractable
{
    void Interact(Transform caller = null);
    void SeconderyInteraction(Transform caller = null);
    bool AlternativeInteractionReady() => true;
    float GetInteractionTime() => 0;
}
