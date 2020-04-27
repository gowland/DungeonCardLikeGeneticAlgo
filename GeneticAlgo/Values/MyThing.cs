using System;

namespace GeneticAlgo.Values
{
    public class MyThing : ICloneable
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public int E { get; set; }

        public int Sum => A + B + C + D + E;

        public object Clone()
        {
            return new MyThing
            {
                A = A,
                B = B,
                C = C,
                D = D,
                E = E,
            };
        }
    }
}