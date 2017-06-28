using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : Living
{
    //used for reproduction
    private int ENERGY_THRESHOLD = 100;

    //GameObjects
    SimulationEntityController SEC;

    // The array that is it's statline, an array of ints for ease of mutation
    // RegrowthRate
    public new int[] Traits = new int[1];

    public new int Energy { get; set; }

    // Use this for initialization
    public override void Start()
    {
        SEC = GameObject.Find("_SimulationEntityController").GetComponent<SimulationEntityController>();
        SEC.Plants.Add(this.gameObject);
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
        Energy /= 2;
        //Plant pl = new Plant();
        //copy this plant to new
        //pl.Energy = (Energy / 2); // 1/4 of original energy, thus 1/4 of energy is expended in reproduction
        //PC.Plants.Add(pl.gameObject);
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
            //remove this plant from game
        }
    }

    public override string getType()
    {
        return "Plant";
    }
}
