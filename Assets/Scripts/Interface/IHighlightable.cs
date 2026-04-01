using UnityEngine;

public interface IHighlightable 
{
    void Highlight(bool enable); // optional
    bool HightlightNeedsUpdate() => false;


}
