using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : MonoBehaviour, IInanimate {

    private readonly bool DEFAULT_HAZARD = false;
    private readonly int DEFAULT_DEPTH = 0;
    private readonly int DEFAULT_X = 0;
    private readonly int DEFAULT_Y = 0;

    public bool Hazardous { get; set; }
    public int Depth { get; set; }
    public int XLength { get; set; }
    public int YLength { get; set; }

    //public enum ?? tides
    // public bool hasTides
    //public void doesHaveTides() {
    //      if (size is big enough) {
    //          hasTides = true;
    //              } else {
    //          hasTides = false;
    //} }

    public Liquid()
    {
        Hazardous = DEFAULT_HAZARD;
        Depth = DEFAULT_DEPTH;
        XLength = DEFAULT_X;
        YLength = DEFAULT_Y;
    }

	// Use this for initialization
	void Start () {
		//Liquid();   ??
        //hasTides();
	}
	
	// Update is called once per frame
	void Update () {
		//check tides
	}
}
