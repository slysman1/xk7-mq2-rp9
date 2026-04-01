using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Sequence", fileName = "Tutorial Sequence")]
public class TutorialSequenceSO : ScriptableObject
{
    public TutorialStep[] steps;
}