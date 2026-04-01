using UnityEngine;

public class Audio_AmbientManager : MonoBehaviour
{
    [SerializeField] private Transform positionsA;
    [SerializeField] private Transform positionsB;
    [SerializeField] private Audio_AmbientOneShot ambientOneShot;


    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //    PlayOneShot();
    }
    [ContextMenu("Start Play Shot")]
    public void PlayOneShot()
    {
        ambientOneShot.PlayOneShot();
    }
    
}
