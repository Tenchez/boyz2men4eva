using UnityEngine;
using System.Collections;

public abstract class Living : MonoBehaviour, ILiving
{
    public int Energy;

    public int[] Traits = new int[5];
    public string SpeciesName;

    // Use this for initialization
    public abstract void Start();

    // Update is called once per frame
    public abstract void Update();

    public abstract string getType();

    public abstract void Consume();

    public abstract void Reproduce();

    public abstract void EnergyTick();

    public bool isSameSpecies(ILiving other)
    {
        Living creature = (Living)other;

        bool same;

        if (creature.getType() == getType())
        {
            same = false;
        }
        else
        {
            same = true;

            for (int i = 0; i < this.Traits.Length; i++)
            {
                if (this.Traits[i] != creature.Traits[i])
                {
                    same = false;
                }
            }
        }

        return same;
    }
}
