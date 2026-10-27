using UnityEngine;

public class SetVolumeHigh : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.SetVolume(1f);
    }
    
}
