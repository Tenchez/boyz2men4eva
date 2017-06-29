using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Living {
    // GameObjects
    SimulationEntityController SEC;
    GameController GC;

    // If you are chasing, whether it is a food or a plant, we alias it here to keep track of?
    public GameObject target;
    public GameObject mate;
    public GameObject pursuer;

    // The array that is it's statline, an array of ints for ease of mutation
    // Sise, Strength, Speed, Age, Diet
    public new int[] Traits = new int[5];

    //this is done because there will be no pre-fabs for predators
    public new string SpeciesName = "Predator";

    //fully rested
    private readonly int EXHAUSTION_THRESHOLD = 50;

    //underfed
    private readonly int ENERGY_DANGER_LEVEL = 150;

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
    public new int Energy { get; set; }
    // public int Thirst;
    // public bool CanSwim;

    public enum States { Dead, Sleeping, Grazing, Fleeing, Chasing, Eating}

    Vector3 Heading { get; set; }
    public float sightAngle { get; set; }
    public float sightRadius { get; set; }
    public States State;
    
    // Use this for initialization
    public override void Start()
    {
        GC = GameObject.Find("_GameController").GetComponent<GameController>();
        SEC = GameObject.Find("_SimulationEntityController").GetComponent<SimulationEntityController>();
        SEC.Animals.Add(this.gameObject);
        Heading = new Vector3(1, 1, 0);

        //initial value
        Energy = EXHAUSTION_THRESHOLD * 10;

        Exhaustion = EXHAUSTION_THRESHOLD;
        Health = HEALTH_THRESHOLD;
        EnergyChangeFactorStandard = Traits[0];
    }

    // Update is called once per frame
    public override void Update()
    {
        if (State != States.Dead)
        {
            //should this be called every time, every other time, every time seconds of datetime.now % 2 = 0??
            EnergyTick();
            HealthTick();
            ExhaustionTick();
        }
        else
        {
            //dont alway remove - the carcass should remain in the simulation to be eaten
            if (Energy == 0)
            {
                SEC.Animals.Remove(this.transform.gameObject);
                Destroy(this);
            }
        }

        switch (State)
        {
            case States.Sleeping:
                break;
            case States.Grazing:
                GameObject food = FindEdible();
                if (food == null)
                {
                    Wander();
                }
                else
                {
                    target = food;
                }
                break;
            case States.Chasing:
                MoveTowards(target.transform.position, 2);
                if (target.GetComponentInChildren<ILiving>().getType() == "Animal")
                {
                    if (Vector3.Distance(target.gameObject.transform.position, this.gameObject.transform.position) < UtilityConstants.INTERACT_RANGE)
                    {
                        AttackTarget(target);
                    }
                }
                break;
            case States.Eating:
                Consume();
                if (target.GetComponentInChildren<Living>().Energy <= 0)
                {
                    target = null;
                }
                break;
            case States.Fleeing:
                Flee(2);
                break;
            case States.Dead:
                break;
            default:
                throw new Exception("State not found: Animal.Update()");
        }

        AdjustState();
    }

    public void AttackTarget(GameObject victim)
    {
        victim.GetComponentInChildren<Animal>().Health = 0;
    }

    public bool IsTargetWithinInteractRange()
    {
        bool within = false;

        if (target != null)
        {
            within = Vector3.Distance(this.gameObject.transform.position, target.transform.position) <
                     UtilityConstants.INTERACT_RANGE;
        }
        return within;
    }

    public bool IsTargetEdible()
    {
        bool targetIsEatable = false;

        if (target != null)
        {
            if (target.GetComponentInChildren<ILiving>().getType() == "Plant")
            {
                targetIsEatable = true;
            }
            else if (target.GetComponentInChildren<ILiving>().getType() == "Animal")
            {
                targetIsEatable = target.GetComponentInChildren<Animal>().State == States.Dead;
            }
        }
        return targetIsEatable;
    }

    public void AdjustState()
    {
        List<GameObject> nearbyThings = AllYouCanSee();
        bool inEatingRange = IsTargetWithinInteractRange();
        bool isEatable = IsTargetEdible();
        if (State != States.Chasing) //or mating
        {
            target = null;
        }
        if (Health < 0 || Energy < 0)
        {
            State = States.Dead;
        }
        else if (SetPredatorIfExists(nearbyThings))
        {
            State = States.Fleeing;
        }
        else if (isEatable && inEatingRange && Energy <= 10000)
        {
            State = States.Eating;
        }
        else if (Exhaustion < 10 && State != States.Sleeping)
        {
            State = States.Sleeping;
        }
        else if (Exhaustion > EXHAUSTION_THRESHOLD && State == States.Sleeping)
        {
            State = States.Grazing;
        }
        else if (isEatable && inEatingRange)
        {
            State = States.Eating;
        }
        else if (target != null)
        {
            State = States.Chasing;
        }
        else if (State != States.Sleeping)
        { 
            State = States.Grazing;
        }
    }

    public bool SetPredatorIfExists(List<GameObject> nearbyThings)
    {
        bool predatorExists = false;
        GameObject closestPredator = null;
        float closestPredDistance = Mathf.Infinity;
        for (int i = 0; i < nearbyThings.Count; i++)
        {
            if (nearbyThings[i].GetComponentInChildren<ILiving>().getType() == "Animal")
            {
                if (nearbyThings[i].GetComponentInChildren<Animal>().Traits[1] > this.Traits[1])
                {
                    float currentDist = Vector3.Distance(closestPredator.transform.position, this.transform.position);
                    if (closestPredator == null || currentDist < closestPredDistance)
                    {
                        predatorExists = true;
                        closestPredator = nearbyThings[i];
                        closestPredDistance = currentDist;
                    }
                }
            }
        }

        if (closestPredator != null)
        {
            pursuer = closestPredator;
        }

        return predatorExists;
    }

    public void Flee(float pace)
    {
        Heading = -1 * (pursuer.transform.position - this.transform.position);
        Heading.Normalize();
        this.gameObject.transform.Translate(Heading * Traits[2] * pace / 50f, Space.World);
    }

    // Ca1lled to eat food (plants or animals)
    public override void Consume()
    {
        this.Energy += target.GetComponentInChildren<Living>().Energy;
        target.GetComponentInChildren<Living>().Energy = 0;
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

    public void MoveTowards(Vector3 position, float pace)
    {
        Heading = position - this.transform.position;
        Heading.Normalize();
        this.gameObject.transform.Translate(Heading * Traits[2] * pace / 50f, Space.World);
    }

    public void HealthTick()
    {
        if (Health < HEALTH_THRESHOLD && Energy > ENERGY_DANGER_LEVEL*2) // just some small value of health threshold.. i guess?
        {
            Health += HealthChangeFactor;
            Energy -= EnergyChangeFactor;
        }
        else if (Energy < ENERGY_DANGER_LEVEL)
        {
            Health--;
        }
    }

    public void ExhaustionTick()
    {
        if (State != States.Sleeping)
        {
            Exhaustion--;

            sightRadius = normalDetectionRange;

            EnergyChangeFactor = EnergyChangeFactorStandard;
            HealthChangeFactor = HealthChangeFactorStandard;
        }
        else
        {
            //continue sleeping - placeholder
            Exhaustion++;

            sightRadius = normalDetectionRange / 3;

            EnergyChangeFactor = EnergyChangeFactorStandard / 2; //slow energy burn while asleep
            HealthChangeFactor = HealthChangeFactorStandard * 2; //heal fast while asleep
        }
    }

    #region movement
    public void Wander()
    {
        //walks the way it is looking
        this.gameObject.transform.Translate(Heading * this.Traits[2] / 20f, Space.World);
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

    //    private GameObject LocateMate()
    //    {
    //        GameObject CloseMate = null;
    //
    //        List<GameObject> NearByLivingThings = AllYouCanSee();
    //
    //        foreach (GameObject PotentialMate in NearByLivingThings)
    //        {
    //            if (IsSameSpecies(PotentialMate))
    //            {
    //                CloseMate = PotentialMate;
    //            }
    //        }
    //
    //        return CloseMate;
    //    }
    //
    //    private void Mate()
    //    {
    //        if (Vector3.Distance(this.transform.position, mate.transform.position) < 5)
    //        {
    //            mate.GetComponentInChildren<Animal>().Energy /= 2;
    //            Energy /= 2;
    //            GC.Spawn("Squirrel", this.gameObject.transform.position.x, this.gameObject.transform.position.x);
    //        }
    //    }
}
