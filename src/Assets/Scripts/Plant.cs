using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Plant : Living
{
    //used for reproduction
    private int ENERGY_THRESHOLD = 300;

    private static System.Random r = new Random();

    //GameObjects
    SimulationEntityController SEC;
    GameController GC;

    // The array that is it's statline, an array of ints for ease of mutation
    // RegrowthRate

    // Use this for initialization
    public override void Start()
    {
        SEC = GameObject.Find("_SimulationEntityController").GetComponent<SimulationEntityController>();
        SEC.Plants.Add(gameObject);
        GC = GameObject.Find("_GameController").GetComponent<GameController>();

        Energy = 10;
    }

    // Update is called once per frame
    public override void Update()
    {
        //Everytime, just scale it to make that work
        EnergyTick();
        Consume();
        TryReproduce();
    }

    // Sunlight, maybe water later
    public override void Consume()
    {
        //if (weather)
        Energy += 3;
    }

    public void TryReproduce()
    {
        double energyFactor;

        switch (Traits[0])
        {
            case 1:
                //Stats.Regrowth.Slow

                energyFactor = 1.5;

                break;
            case 2:
                //Stats.Regrowth.Average

                energyFactor = 1;

                break;
            case 3:
                //Stats.Regrowth.Weedlike

                energyFactor = .5;

                break;
            default:
                throw new System.Exception("Stats.Regrowth enum value not found -- Plant, TryReproduce()");
                break;
        }

        if ((double)Energy > ENERGY_THRESHOLD * energyFactor)
        {
            Reproduce();
        }
    }

    // Probably just appearing next to the original
    public override void Reproduce()
    {
        int modX = r.Next(11);
        int modY = r.Next(11);

        modX -= 5;
        modY -= 5;

        Energy /= 2;
        GC.Spawn(SpeciesName, this.gameObject.transform.position.x+modX, this.gameObject.transform.position.y+modY);
    }

    public override void EnergyTick()
    {
        if (Energy >= 0)
        {
            //placeholder
            Energy--;
        }
        else
        {
            SEC.remove(this);
            Destroy(gameObject);
            Destroy(this);
        }
    }

    public override string getType()
    {
        return "Plant";
    }
}
