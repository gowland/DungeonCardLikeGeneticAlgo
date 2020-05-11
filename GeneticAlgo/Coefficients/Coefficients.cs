using System;

namespace GeneticAlgo.Coefficients
{
    public class Coefficients : ICloneable
    {
        public double FifthLevel { get; set; }
        public double FourthLevel { get; set; }
        public double ThirdLevel { get; set; }
        public double SecondLevel { get; set; }
        public double FirstLevel { get; set; }

        public double Calc(double x)
        {
            return FifthLevel * Math.Pow(x, 4)
                   + FourthLevel * Math.Pow(x, 3)
                   + ThirdLevel * Math.Pow(x, 2)
                   + SecondLevel * Math.Pow(x, 1)
                   + FirstLevel;
        }

        public object Clone()
        {
            return new Coefficients
            {
                FirstLevel = FirstLevel,
                SecondLevel = SecondLevel,
                ThirdLevel = ThirdLevel,
                FourthLevel = FourthLevel,
                FifthLevel = FifthLevel
            };
        }
    }
}