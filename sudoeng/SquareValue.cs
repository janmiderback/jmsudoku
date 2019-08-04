using System;

namespace SudoEng
{
    /// <summary>
    /// Bitfield enumeration representing a unique or undetermined value in a square.
    /// </summary>
    [Flags]
    internal enum SquareValue : ushort
    {
        None = 0,
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
        Four = 1 << 3,
        Five = 1 << 4,  
        Six = 1 << 5,
        Seven = 1 << 6,
        Eight = 1 << 7,
        Nine = 1 << 8,
        All = One | Two | Three | Four | Five | Six | Seven | Eight | Nine,
    }
}
