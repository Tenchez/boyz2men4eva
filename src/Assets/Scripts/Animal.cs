using System.Collections.Generic;
using UnityEngine;

public class Animal : Living {
    // GameObjects
    SimulationEntityController SEC;
    GameController GC;
    static System.Random r = new System.Random();

    // If you are chasing, whether it is a food or a plant, we alias it here to keep track of?
    public GameObject target;

    // The array that is it's statline, an array of ints for ease of mutation
    // Size, Strength, Speed, Age, Diet

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
    private float normalDetectionRange = 3; //placeholder value - is this in pixels...?
    
    // Values for live and health
    public int Health { get; set; }
    public int Exhaustion { get; set; }
    public bool Alive { get; set; }
    public bool Asleep { get; set; }
    // public int Thirst;
    // public bool CanSwim;

    public enum States { Dead, Sleeping, Grazing, Fleeing, Chasing, Eating}

    Vector3 Heading { get; set; }
    public float sightAngle { get; set; }
    public float sightRadius { get; set; }
    public States State;
    
    #region LivingMethods
    // Use this for initialization
    public override void Start()
    {
        GC = GameObject.Find("_GameController").GetComponent<GameController>();
        SEC = GameObject.Find("_SimulationEntityController").GetComponent<SimulationEntityController>();
        SEC.Animals.Add(this.gameObject);
        Heading = new Vector3(1, 1, 0);
        Alive = true;
        //Exhaustion = EXHAUSTION_THRESHOLD;
        Health = HEALTH_THRESHOLD;
        //EnergyChangeFactorStandard = Traits[0];
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Alive)
        {
            //should this be called every time, every other time, every time seconds of datetime.now % 2 = 0??
            EnergyTick();
            HealthTick();
            ExhaustionTick();
            Wander();
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

    // Ca1lled to eat food (plants or animals)
    public override void Consume()
    {

    }

    // Called to reproduce sexually
    public override void Reproduce()
    {

    }

    public override void EnergyTick()
    {
        //placeholder
        Energy -= EnergyChangeFactor;
    }

    public override string getType()
    {
        return "Animal";
    }
    #endregion

    public void MoveTowards(Vector3 position, float pace)
    {
        Heading = position - this.transform.position;
        Heading.Normalize();
        this.gameObject.transform.Translate(Heading * Traits[2] * pace / 50f, Space.World);
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
        //walks the way it is looking
        this.gameObject.transform.Translate(Heading * this.Traits[2] / 35f, Space.World);
        if (r.Next(10) < 4)
        {
            for(int j = 0; j < 3; j++)
            {
                Turn(r.Next(2));
            }
        }
        if (OutsideBounderies(((this.gameObject.transform.position + ((Quaternion.AngleAxis(sightAngle / 2, Vector3.forward) * Heading) * sightRadius)).x), ((this.gameObject.transform.position + ((Quaternion.AngleAxis(sightAngle / 2, Vector3.forward) * Heading) * sightRadius)).y)))
        {
            Turn(0);
        }
        else if (OutsideBounderies(((this.gameObject.transform.position + ((Quaternion.AngleAxis(-sightAngle / 2, Vector3.forward) * Heading) * sightRadius)).x), ((this.gameObject.transform.position + ((Quaternion.AngleAxis(-sightAngle / 2, Vector3.forward) * Heading) * sightRadius)).y)))
        {
            Turn(1);
        }
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

    public bool CanSee(GameObject thing)
    {
        bool IsSeen = false;
        if (Vector2.Distance(thing.transform.position, this.gameObject.transform.position) < sightRadius && (Vector3.Angle(Heading, (thing.transform.position - this.gameObject.transform.position)) < sightAngle))
        {
            IsSeen = true;
        }

        return IsSeen;
    }

    public List<GameObject> AllYouCanSee()
    {
        List<GameObject> InSight = new List<GameObject>();

        foreach (GameObject LivingThing in SEC.AllLiving)
        {
            if (CanSee(LivingThing))
            {
                InSight.Add(LivingThing);
            }
        }

        return InSight;
    }

    //returns null if no edible objects are in view range
    private GameObject FindEdible()
    {
        GameObject ToReturn = null;

        List<GameObject> InSight = AllYouCanSee();
        List<GameObject> InSightAndEdible = new List<GameObject>();

        string Desired;

        switch (Traits[4])
        {
            case 1:
                Desired = "Plant";

                break;
            case 2:
                Desired = "Both";

                break;
            case 3:
                Desired = "Animal";

                break;
            default:
                throw new System.Exception("Diet not found: Animal.FindEdible()");
                break;
        }

        foreach (GameObject food in InSight)
        {
            if (Desired == "Both" || food.GetComponentInChildren<ILiving>().getType() == Desired)
            {
                InSightAndEdible.Add(food);
            }
        }

        if (InSightAndEdible.Count > 0)
        {
            ToReturn = InSightAndEdible[0];
        }

        for (int i = 1; i < InSightAndEdible.Count; i++)
        {
            GameObject You = this.gameObject;
            GameObject Target = ToReturn;
            GameObject Test = InSightAndEdible[i];
            if (Vector3.Distance(You.transform.position, Target.transform.position) > Vector3.Distance(You.transform.position, Test.transform.position))
            {
                ToReturn = InSightAndEdible[i];
            }
        }

        return ToReturn;
    }

    private void Turn(int dir)
    {
        if (dir == 0)
        {
            Heading = Quaternion.AngleAxis(-2f, Vector3.forward) * Heading;
        }
        else if (dir == 1)
        {
            Heading = Quaternion.AngleAxis(2f, Vector3.forward) * Heading;
        }
    }

    private bool OutsideBounderies(float x, float y)
    {
        bool yes = false;
        if (x > ((GC.WIDTH - 3) / 2) || x < (-(GC.WIDTH + 3) / 2) || y > ((GC.HEIGHT - 3) / 2) || y < (-(GC.HEIGHT + 3) / 2))
        {
            yes = true;
        }
        return yes;
    }
}
