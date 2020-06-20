using System;
using System.ComponentModel.Design;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using GeneticSolver.Expressions;
using GeneticSolver.Expressions.Implementations;
using GeneticSolver.Random;
using NUnit.Framework;

namespace GeneticSolver.Tests
{
    [TestFixture]
    public class ExpressionsFixture
    {

        [TestCase("(C) + (1)")]
        [TestCase("((23) * (Foo)) + ((Blah) - (42))")]
        public void BuildTreeFromString(string input)
        {
            Console.WriteLine(ExpressionGenerator<SomeClass>.BuildTreeFromString(input));
        }

        [TestCase("(02.00000) + (01.00000)")]
        [TestCase("((23.00000) * (45.20000)) + ((99.00000) - (42.00000))")]
        [TestCase("(SomeValue) + (-01.00001)")]
        public void FromString (string input)
        {
            var expressionGenerator = new ExpressionGenerator<SomeClass>(
                new UnWeightedRandom(),
               new []
                {
                    new BoundValueExpression<SomeClass>(someClass => someClass.SomeValue, nameof(SomeClass.SomeValue)),
                },
               new []
                {

                        new Operation((a,b) => a + b, "+"),
                        new Operation((a,b) => a - b, "-"),
                        new Operation((a,b) => a * b, "*"),
                });

            var fromString = expressionGenerator.FromString(input);
            Console.WriteLine(fromString);
            Assert.That(fromString.ToString(), Is.EqualTo(input));
        }

        [TestFixture]
        public class FuncExpressionFixture
        {
            [Test]
            public void Evaluate_WhenCalled_ReturnsExpected()
            {
                var expression = new FuncExpression<SomeClass>()
                {
                    Left = new ValueExpression<SomeClass>(5.0),
                    Right = new ValueExpression<SomeClass>(9.0),
                    Operation = new Operation((a,b) => a+b, "+"),
                };

                var someClass = new SomeClass();

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(14.0));
            }

            [Test]
            public void Evaluate_WhenClonedAndCloneHasLeftChanged_Unchanged()
            {
                var expression = new FuncExpression<SomeClass>()
                {
                    Left = new ValueExpression<SomeClass>(5.0),
                    Right = new ValueExpression<SomeClass>(9.0),
                    Operation = new Operation((a,b) => a+b, "+"),
                };

                var clone = (FuncExpression<SomeClass>)expression.Clone();
                clone.Left = new ValueExpression<SomeClass>(3.0);

                var someClass = new SomeClass();

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(14.0));
                Assert.That(clone.Evaluate(someClass), Is.EqualTo(12.0));
            }

            [Test]
            public void Evaluate_WhenClonedAndCloneHasRightChanged_Unchanged()
            {
                var expression = new FuncExpression<SomeClass>()
                {
                    Left = new ValueExpression<SomeClass>(5.0),
                    Right = new ValueExpression<SomeClass>(9.0),
                    Operation = new Operation((a,b) => a+b, "+"),
                };

                var clone = (FuncExpression<SomeClass>)expression.Clone();
                clone.Right = new ValueExpression<SomeClass>(3.0);

                var someClass = new SomeClass();

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(14.0));
                Assert.That(clone.Evaluate(someClass), Is.EqualTo(8.0));
            }

            [Test]
            public void Evaluate_WhenClonedAndCloneHasOperationChanged_Unchanged()
            {
                var expression = new FuncExpression<SomeClass>()
                {
                    Left = new ValueExpression<SomeClass>(5.0),
                    Right = new ValueExpression<SomeClass>(9.0),
                    Operation = new Operation((a,b) => a+b, "+"),
                };

                var clone = (FuncExpression<SomeClass>)expression.Clone();
                clone.Operation = new Operation((a,b) => a*b, "*");

                var someClass = new SomeClass();

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(14.0));
                Assert.That(clone.Evaluate(someClass), Is.EqualTo(45.0));
            }
        }

        [TestFixture]
        public class ValueExpressionFixture
        {
            [Test]
            public void Value_WhenClonedValueChanges_Unchanged()
            {
                var expression = new ValueExpression<object>(1.0);
                var clone = (ValueExpression<object>) expression.Clone();
                clone.Value = 2.0;

                Assert.That(expression.Value, Is.EqualTo(1.0));
                Assert.That(clone.Value, Is.EqualTo(2.0));
            }
        }

        [TestFixture]
        public class BoundValueExpressionFixture
        {
            [Test]
            public void Evaluate_Called_ReturnsValueFromSource()
            {
                var expression = new BoundValueExpression<SomeClass>(s => s.SomeValue, nameof(SomeClass.SomeValue));
                var someClass = new SomeClass(){ SomeValue = 5.0 };

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(5.0));
            }

            [Test]
            public void Evaluate_CalledSecondTimeWithDifferentValueOnSource_ReturnsNewValue()
            {
                var expression = new BoundValueExpression<SomeClass>(s => s.SomeValue, nameof(SomeClass.SomeValue));

                var someClass = new SomeClass(){ SomeValue = 5.0 };
                expression.Evaluate(someClass);

                someClass.SomeValue = 9.0;

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(9.0));
            }

            [Test]
            public void Value_WhenCloned_Unchanged()
            {
                var expression = new BoundValueExpression<SomeClass>(s => s.SomeValue, nameof(SomeClass.SomeValue));
                var clone = (BoundValueExpression<SomeClass>) expression.Clone();

                var someClass = new SomeClass(){ SomeValue = 5.0 };

                Assert.That(expression.Evaluate(someClass), Is.EqualTo(5.0));
                Assert.That(clone.Evaluate(someClass), Is.EqualTo(5.0));
            }

        }
        private class SomeClass
        {
            public double SomeValue { get; set; }
        }
    }
}