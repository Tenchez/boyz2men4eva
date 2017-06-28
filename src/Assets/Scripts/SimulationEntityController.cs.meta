using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationEntityController : MonoBehaviour {
    public List<GameObject> Plants = new List<GameObject>();
    public List<GameObject> Animals = new List<GameObject>();
    public List<GameObject> AllLiving = new List<GameObject>();

    public List<IInanimate> Rocks = new List<IInanimate>();
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
        AllLiving.AddRange(Plants);
        AllLiving.AddRange(Animals);
    }
}
