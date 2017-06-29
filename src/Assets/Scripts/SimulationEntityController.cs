using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SimulationEntityController : MonoBehaviour {
    public List<GameObject> Plants = new List<GameObject>();
    public List<GameObject> Animals = new List<GameObject>();
    public List<GameObject> AllLiving = new List<GameObject>();

    public List<IInanimate> Rocks = new List<IInanimate>();

    public void remove(ILiving obj)
    {
        if (Plants.Contains(((Living) obj).gameObject))
        {
            Plants.Remove(((Living) obj).gameObject);
        }
        else if (Animals.Contains(((Living) obj).gameObject))
        {
            Animals.Remove(((Living) obj).gameObject);
        }
        
        UpdateAll();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAll();
    }

    private void UpdateAll()
    {
        AllLiving.Clear();

        foreach (GameObject x in Plants)
        {
            AllLiving.Add(x);
        }

        foreach (GameObject x in Animals)
        {
            AllLiving.Add(x);
        }
    }
}
