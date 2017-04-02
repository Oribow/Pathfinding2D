using UnityEngine;
using System.Collections;

public class KeepGameObjectOnLoad : MonoBehaviour {

    void Awake()
    {
        if(GameObject.FindObjectsOfType<KeepGameObjectOnLoad>().Length == 1)
        {
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
	
}
