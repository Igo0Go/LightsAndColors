using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleBuildController : MonoBehaviour
{
    public CreationModule creationModule;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            creationModule.Activate();
            return;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            creationModule.Build(true);
            return;
        }
        if (Input.GetKeyUp(KeyCode.B))
        {
            creationModule.Build(false);
            return;
        }
    }
}
