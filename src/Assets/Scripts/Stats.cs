using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats {

    #region all
    // How physically big the animal or plant is
    public enum Size
    {
        Tiny = 1,
        Small,
        Medium,
        Large,
        Huge,
        ReallyFlippinBig
    }

    // How much force something produces, stronger takes more energy
    public enum Strength
    {
        Frail = 1,
        Weak,
        Average,
        Strong,
        Herculean
    }

    // How quickly it travels, taking more energy the faster
    public enum Speed
    {
        NonMoving = 0, //for plants
        Tortoiseesque,
        Slow,
        Average,
        Quick,
        Speedy,
        Usain
    }

    // How long the animal lives naturally
    public enum Age
    {
        Infant = 1,
        Adolscent,
        Adult,
        Senior,
        OlderThanDirt
    }

    public enum ReproduceRate
    {
        Infertile,
        ExtremelyFast,
        Fast,
        Normal,
        Slow
    }
    #endregion

    #region animal
    // Whether or not it is an herbovore, omnivore, or carnivore
    public enum Diet
    {
        Plants = 1,
        Anything,
        Animals
    }

    public enum ConsumeRate
    {
        Slow = 1,
        Normal,
        Fast
    }
    #endregion

    #region plant
    public enum PlantEnergySource
    {
        Producer = 1,
        Decomposer //used later
    }

    // Plant regrowth rate
    public enum Regrowth
    {
        Slow = 1,
        Average,
        Weedlike
    }
    #endregion
}
