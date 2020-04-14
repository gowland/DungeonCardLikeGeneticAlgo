using System;
using System.Collections.Generic;
using GeneticSolver;
using GeneticSolver.GenomeProperty;
using GeneticSolver.RequiredInterfaces;

namespace GeneticAlgo.Values
{
    public class MyThingGenomeDescription : IGenomeDescription<MyThing>
    {
        private readonly Random _random = new Random();

        public IEnumerable<IGenomeProperty<MyThing>> Properties => new[]
        {
            new IntegerGenomeProperty<MyThing>(g => g.A, (g, val) => g.A = val, 0, 1000, -30, 30, _random),
            new IntegerGenomeProperty<MyThing>(g => g.B, (g, val) => g.B = val, 0, 1000, -30, 30, _random),
            new IntegerGenomeProperty<MyThing>(g => g.C, (g, val) => g.C = val, 0, 1000, -30, 30, _random),
            new IntegerGenomeProperty<MyThing>(g => g.D, (g, val) => g.D = val, 0, 1000, -30, 30, _random),
            new IntegerGenomeProperty<MyThing>(g => g.E, (g, val) => g.E = val, 0, 1000, -30, 30, _random),
        };
    }
}