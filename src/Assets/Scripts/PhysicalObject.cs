using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObject : MonoBehaviour, IInanimate {

    private readonly int DEFAULT_SIZE = 0;
    private readonly bool DEFAULT_PASS = false;
    private readonly string DEFAULT_DESC = "Rock";

    public int Size { get; set; }
    public bool Passable { get; set; }
    public string Description { get; set; }

    public PhysicalObject()
    {
        Size = DEFAULT_SIZE;
        Passable = DEFAULT_PASS;
        Description = DEFAULT_DESC;
    }

	// Use this for initialization
	void Start () {
		//PhysicalObject(); ??
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
