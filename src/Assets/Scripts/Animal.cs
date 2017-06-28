using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour, ILiving {
    // GameObjects
    SimulationEntityController SEC;

    // If you are chasing, whether it is a food or a plant, we alias it here to keep track of?
    public GameObject target;

    // The array that is it's statline, an array of ints for ease of mutation
    // Sise, Strength, Speed, Age, Diet
    public int[] Traits = new int[5];

    //fully rested
    private readonly int EXHAUSTION_THRESHOLD = 50;

    //fully healed
    private readonly int HEALTH_THRESHOLD = 50;

    //used to change rates when sleeping or running etc
    private int HealthChangeFactor = 1;
    private int EnergyChangeFactor = 1;

    //standard state change factors - for when doing nothing
    private int HealthChangeFactorStandard = 1;
    private int EnergyChangeFactorStandard;
    private float normalDetectionRange = 50; //placeholder value - is this in pixels...?
    
    // Values for live and health
    public int Health { get; set; }
    public int Exhaustion { get; set; }
    public int Energy { get; set; }
    public bool Alive { get; set; }
    public bool Asleep { get; set; }
    // public int Thirst;
    // public bool CanSwim;

    public enum States { Dead, Sleeping, Grazing, Fleeing, Chasing, Eating}

    Vector3 Heading { get; set; }
    public float sightAngle { get; set; }
    public float sightRadius { get; set; }
    public States State;

	// Use this for initialization
	void Start ()
    {
        SEC = GameObject.Find("_SimulationEntityController").GetComponent<SimulationEntityController>();
        SEC.Animals.Add(this.gameObject);

    //Alive = true;
    //Exhaustion = EXHAUSTION_THRESHOLD;
    //Health = HEALTH_THRESHOLD;
    //EnergyChangeFactorStandard = Traits[0];
}
	
	// Update is called once per frame
	void Update ()
    {
        if (Alive)
        {
            //should this be called every time, every other time, every time seconds of datetime.now % 2 = 0??
            EnergyTick();
            HealthTick();
            ExhaustionTick();

            if (Health == 0)
            {
                Alive = false;
            }
        }
        else
        {
            //dont alway remove - the carcass should remain in the simulation to be eaten
            if (Energy == 0)
            {
                //remove this animal from simulation
            }
        }

        switch (State)
        {
            case States.Sleeping:
                break;
            case States.Grazing:
                break;
            case States.Chasing:
                break;
            case States.Eating:
                break;
            case States.Fleeing:
                break;
            case States.Dead:
                break;
        }
    }

    // Called to eat food (plants or animals)
    public void Consume()
    {

    }

    // Called to reproduce sexually
    public void Reproduce()
    {

    }

    public void MoveTowards(Vector3 position, float pace)
    {
        Heading = position - this.transform.position;
        Heading.Normalize();
        this.gameObject.transform.Translate(Heading * Traits[2] * pace / 50f, Space.World);
    }

    public void EnergyTick()
    {
        //placeholder
        Energy -= EnergyChangeFactor;
    }

    public void HealthTick()
    {
        if (Health <= 0)
        {
            Alive = false;
        }
        else if (Energy > HEALTH_THRESHOLD * (1/5)) // just some small value of health threshold.. i guess?
        {
            Health += HealthChangeFactor;
            Energy -= EnergyChangeFactor;
        }
    }

    public void ExhaustionTick()
    {
        if (!Asleep)
        {
            Exhaustion--;

            if (Exhaustion < 0)
            {
                //go to sleep
                ChangeAsleep();
            }
        }
        else if (Exhaustion > EXHAUSTION_THRESHOLD || ThreatInDetectionRange())
        {
            //wake up
            ChangeAsleep();
        }
        else
        {
            //continue sleeping - placeholder
            Exhaustion++;
        }
    }

    public bool ThreatInDetectionRange()
    {
        //to be Implemented
        return false;
    }

    #region movement
    public void Wander()
    {
        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x + this.Traits[2]/35f, 0, this.gameObject.transform.position.z + this.Traits[2]/35f);
    }
    #endregion

    public void ChangeAsleep()
    {
        if (Asleep)
        {
            Asleep = false;

            sightRadius = normalDetectionRange;

            EnergyChangeFactor = EnergyChangeFactorStandard;
            HealthChangeFactor = HealthChangeFactorStandard;
        }
        else
        {
            Asleep = true;

            sightRadius = normalDetectionRange / 3;

            EnergyChangeFactor = EnergyChangeFactorStandard / 2; //slow energy burn while asleep
            HealthChangeFactor = HealthChangeFactorStandard * 2; //heal fast while asleep
        }
    }

    public string getType()
    {
        return "Animal";
    }

    public bool CanSee(GameObject thing)
    {
        bool canSee = false;
        if (Vector2.Distance(thing.transform.position, this.gameObject.transform.position) < sightRadius && (Vector3.Angle(Heading, (thing.transform.position - this.gameObject.transform.position)) < sightAngle))
        {
            canSee = true;
        }

        return canSee;
    }

    //returns null if no edible objects are in view range
    private GameObject findEdible()
    {
        GameObject toReturn = null;

        string desired;

        switch (Traits[4])
        {
            case 1:
                desired = "Plant";

                break;
            case 2:
                desired = "Both";

                break;
            case 3:
                desired = "Animal";

                break;
            default:
                throw new System.Exception("Diet not found: Animal.findedible()");
                break;
        }

        foreach (GameObject food in SEC.AllLiving)
        {

            // If nearby
            if (Vector3.Distance(this.gameObject.transform.position, food.transform.position) < sightRadius)
            {
                List<GameObject> InSight;// = METHOD();
/*
                foreach (GameObject edible in InSight)
                {
                    if (desired.Equals("Both") || food.GetComponentInChildren<ILiving>().getType().Equals(desired))
                    {
                        if (toReturn == null || Vector3.Distance(this.gameObject.transform.position, food.transform.position) < Vector3.Distance(this.gameObject.transform.position, toReturn.transform.position))
                        {
                            toReturn = food;
                        }
                    }
                }
                */
            }
        }

        return toReturn;
    }
}
