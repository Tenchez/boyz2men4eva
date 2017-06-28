using UnityEngine;
using System.Collections;

public abstract class Living : MonoBehaviour, ILiving
{
    public int Energy;

    // Use this for initialization
    public abstract void Start();

    // Update is called once per frame
    public abstract void Update();

    public abstract string getType();

    public abstract void Consume();

    public abstract void Reproduce();

    public abstract void EnergyTick();
}
