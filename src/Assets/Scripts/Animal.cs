using System;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Living {
    // GameObjects
    SimulationEntityController SEC;
    GameController GC;
    static System.Random r = new System.Random();

    // If you are chasing, whether it is a food or a plant, we alias it here to keep track of?
    public GameObject target;
    public GameObject mate;
    public GameObject pursuer;

    // The array that is it's statline, an array of ints for ease of mutation
    // Size, Strength, Speed, Age, Diet

    //this is done because there will be no pre-fabs for predators
    //public new string SpeciesName = "Predator";

    //fully rested
    private readonly int EXHAUSTION_THRESHOLD = 100;

    //underfed
    private readonly int ENERGY_DANGER_LEVEL = 150;

    //fully healed
    private readonly int HEALTH_THRESHOLD = 50;

    //time until checking if followed
    private int framesTillCheck = 50;

    private readonly int FRAMES_TILL_CHECK_DEFAULT = 50;

    private int framesTillAttemptMate = 1000;

    //used to change rates when sleeping or running etc
    private int HealthChangeFactor = 1;
    private int EnergyChangeFactor = 1;
    private int EatSpeed = 5;

    //standard state change factors - for when doing nothing
    private int HealthChangeFactorStandard = 1;
    private int EnergyChangeFactorStandard;
    private float normalDetectionRange = 3; //placeholder value - is this in pixels...?

    // Values for live and health
    public int Health = 0;
    public int Exhaustion { get; set; }
    // public int Thirst;
    // public bool CanSwim;

    public enum States { Dead, Sleeping, Grazing, Fleeing, Chasing, Eating, Mating}

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

        sightAngle = 180;
        sightRadius = 20;
        EatSpeed = 5 * Traits[6];
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

            if (framesTillAttemptMate > 0)
            {
                framesTillAttemptMate--;
            }
        }
        else
        {
            //dont alway remove - the carcass should remain in the simulation to be eaten
            if (Energy <= 0)
            {
                SEC.remove(this);
                Destroy(this.gameObject);
                Destroy(this);
            }
        }

        switch (State)
        {
            case States.Mating:
                MoveTowards(mate.transform.position, 3);
                Mate();
                break;
            case States.Sleeping:
                break;
            case States.Grazing:
                pursuer = null;

                GameObject food = FindEdible();
                if (food == null)
                {
                    Wander();
                }
                else
                {

                    target = food;
                    State = States.Chasing;
                }
                break;
            case States.Chasing:
                if(target == null)
                {
                    break;
                }
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
                framesTillCheck--;

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
        victim.GetComponentInChildren<Animal>().Health = -1;
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
        if (State != States.Chasing && State != States.Eating) //or mating
        {
            target = null;
        }
        if (Health <= 0 || Energy <= 0)
        {
            State = States.Dead;
        }
        else if (State == States.Fleeing && framesTillCheck <= 0)
        {
            Heading = pursuer.transform.position - this.transform.position;
            Heading.Normalize();

            State = States.Grazing;
            framesTillCheck = FRAMES_TILL_CHECK_DEFAULT;
        }
        else if (SetPredatorIfExists(nearbyThings))
        {
            State = States.Fleeing;
        }
        else if (isEatable && inEatingRange && Energy <= 2000)
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
        else if (framesTillAttemptMate <= 0 && Energy > ENERGY_DANGER_LEVEL && ((mate = LocateMate()) != null))
        {
            State = States.Mating;
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
                    float currentDist = Vector3.Distance(nearbyThings[i].transform.position, this.transform.position);
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
        this.gameObject.transform.Translate(Heading * getCurrentSpeed() * pace / 50f, Space.World);
    }

    // Ca1lled to eat food (plants or animals)
    public override void Consume()
    {
        if (target.GetComponentInChildren<Living>().Energy < EatSpeed)
        {
            this.Energy += target.GetComponentInChildren<Living>().Energy;
            target.GetComponentInChildren<Living>().Energy = 0;
        }
        else
        {
            this.Energy += EatSpeed;
            target.GetComponentInChildren<Living>().Energy -= EatSpeed;
        }
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
        this.gameObject.transform.Translate(Heading * getCurrentSpeed() * pace / 50f, Space.World);
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
            Exhaustion += 2;

            sightRadius = normalDetectionRange / 3;

            EnergyChangeFactor = EnergyChangeFactorStandard / 2; //slow energy burn while asleep
            HealthChangeFactor = HealthChangeFactorStandard * 2; //heal fast while asleep
        }
    }

    #region movement
    public void Wander()
    {
        //walks the way it is looking
        gameObject.transform.Translate(Heading * getCurrentSpeed() / 35f, Space.World);
        if (r.Next(10) < 4)
        {
            for(int j = 0; j < 3; j++)
            {
                Turn(r.Next(2));
            }
        }

        if (r.Next(400)<2)
        {
            TurnAround();
        }
        if (OutsideBounderies(((this.gameObject.transform.position + ((Quaternion.AngleAxis(sightAngle / 2, Vector3.forward) * Heading) * 3)).x), ((this.gameObject.transform.position + ((Quaternion.AngleAxis(sightAngle / 2, Vector3.forward) * Heading) * 3)).y)))
        {
            Turn(0);
        }
        else if (OutsideBounderies(((this.gameObject.transform.position + ((Quaternion.AngleAxis(-sightAngle / 2, Vector3.forward) * Heading) * 3)).x), ((this.gameObject.transform.position + ((Quaternion.AngleAxis(-sightAngle / 2, Vector3.forward) * Heading) * 3)).y)))
        {
            Turn(1);
        }
    }
    #endregion

    public bool CanSee(GameObject thing)
    {
        bool IsSeen = false;
        if (Vector2.Distance(thing.transform.position, gameObject.transform.position) < sightRadius && (Vector3.Angle(Heading, (thing.transform.position - this.gameObject.transform.position)) < sightAngle))
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
            if (CanSee(LivingThing) && LivingThing != gameObject)
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
            if (!food.GetComponentInChildren<ILiving>().IsSameSpecies(this.GetComponentInChildren<ILiving>()) && (Desired == "Both" || food.GetComponentInChildren<ILiving>().getType() == Desired))
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
        if (x > ((GC.WIDTH - 5) / 2) || x < (-(GC.WIDTH + 5) / 2) || y > ((GC.HEIGHT - 5) / 2) || y < (-(GC.HEIGHT + 5) / 2))
        {
            yes = true;
        }
        return yes;
    }

    private GameObject LocateMate()
    {
        GameObject CloseMate = null;

        List<GameObject> NearByLivingThings = AllYouCanSee();

        foreach (GameObject PotentialMate in NearByLivingThings)
        {
            if (framesTillAttemptMate <= 0 && getType() == PotentialMate.GetComponentInChildren<ILiving>().getType() && IsSameSpecies(PotentialMate.GetComponentInChildren<ILiving>()) && 
                PotentialMate.GetComponentInChildren<Animal>().State != States.Dead && PotentialMate != this)
            {
                CloseMate = PotentialMate;
            }
        }

        return CloseMate;
    }

    private void Mate()
    {
        if (Vector3.Distance(this.transform.position, mate.transform.position) < UtilityConstants.INTERACT_RANGE)
        {
            mate.GetComponentInChildren<Animal>().Energy /= 2;
            Energy /= 2;
            GC.Spawn(SpeciesName, this.gameObject.transform.position.x, this.gameObject.transform.position.y);
            State = States.Grazing;

            //resetting framesTillAttemptMate
            framesTillAttemptMate = 25 * Traits[5];
        }

    }

    private int getCurrentSpeed()
    {
        int toReturn = Traits[2];

        if (Exhaustion < 0)
        {
            toReturn /= 2;
        }

        return toReturn;
    }

    private void TurnAround()
    {
        Heading = Heading * -1;
    }
}
