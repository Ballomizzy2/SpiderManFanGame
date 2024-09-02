using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DimensionManager : MonoBehaviour
{
    [SerializeField]
    private PostProcessVolume mainVolume;
    [SerializeField]
    private Outline outline;

    [SerializeField]
    private Skybox skybox;

    [SerializeField]
    private List<PostProcessProfile> profiles = new List<PostProcessProfile>();


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchDimension()
    {
        mainVolume.profile = profiles[Random.Range(0, profiles.Count)];
        Debug.Log("Switched");
    }
}
