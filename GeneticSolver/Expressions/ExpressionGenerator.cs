using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GeneticSolver.Expressions.Implementations;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Expressions
{
    public class ExpressionGenerator<T>
    {
        private readonly IRandom _random;
        private readonly IEnumerable<IExpression<T>> _boundValueExpressions;
        private readonly IEnumerable<Operation> _operations;
        private readonly IRandom _unweightedRandom = new UnWeightedRandom();

        public ExpressionGenerator(IRandom random, IEnumerable<IExpression<T>> boundValueExpressions, IEnumerable<Operation> operations)
        {
            _random = random;
            _boundValueExpressions = boundValueExpressions;
            _operations = operations;
        }
        public IExpression<T> GetRandomExpression()
        {
            return _unweightedRandom.SelectOption(new Func<IExpression<T>>[]
            {
                GetFunctionExpression,
                GetBoundValueExpression,
                GetMutableValueExpression,
            }).Invoke();
        }

        public IExpression<T> GetFunctionExpression()
        {
            return new FuncExpression<T>()
            {
                Left = GetRandomExpression(),
                Right = GetRandomExpression(),
                Operation = GetRandomOperation(),
            };
        }

        public Operation GetRandomOperation()
        {
            return _unweightedRandom.SelectOption(_operations);
        }

        public IExpression<T> GetRandomValueExpression()
        {
            return _random.NextBool()
                ? GetBoundValueExpression()
                : GetMutableValueExpression();
        }

        public IExpression<T> GetBoundValueExpression()
        {
            return _random.SelectOption(_boundValueExpressions);
        }

        public IExpression<T> GetMutableValueExpression()
        {
            return new ValueExpression<T>(_random.NextDouble(-20,20));
        }

        public IExpression<T> FromString(string expressionAsString)
        {
            // Build tree
            var tree = BuildTreeFromString(expressionAsString);

            // Convert tree to expressions
            IExpression<T> expression = Build(tree);

            return expression;
        }

        public static SimpleTreeNode BuildTreeFromString(string treeString)
        {
            SimpleTreeNode parent = new SimpleTreeNode();
            var stack = new Stack<SimpleTreeNode>();
            stack.Push(parent);
            string currentToken = "";

            bool CompleteToken()
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    // stack.Peek().Children.Add(new SimpleTreeNode() { Content = currentToken });
                    stack.Peek().Content = currentToken;
                    currentToken = "";

                    return true;
                }

                return false;
            }

            foreach (char c in treeString)
            {
                if (c == '(')
                {
                    CompleteToken();

                    var child = new SimpleTreeNode();
                    stack.Peek().Children.Add(child);
                    stack.Push(child);
                }
                else if (c == ')')
                {
                    CompleteToken();
                    parent = stack.Pop();
                }
                else
                {
                    currentToken += c;
                }
            }

            return stack.Peek();
        }

        public IExpression<T> Build(SimpleTreeNode node)
        {
            Stack<IExpression<T>> stack = new Stack<IExpression<T>>();

            // Determine type of node
            if (node.Children.Count == 2)
            {
                var matchingOperation = _operations.FirstOrDefault(o => node.Content.Trim() == o.ToString());

                if (matchingOperation != null)
                {
                    stack.Push(
                       new FuncExpression<T>()
                        {
                            Operation = matchingOperation,
                            Left = Build(node.Children[0]),
                            Right = Build(node.Children[1]),
                        });
                }
            }
            else
            {
                var matchingBoundValue =
                    _boundValueExpressions.FirstOrDefault(o => node.Content.Trim() == o.ToString());

                if (matchingBoundValue != null)
                {
                    return matchingBoundValue;
                }
                else if (double.TryParse(node.Content, out double value))
                {
                    return new ValueExpression<T>(value);
                }
                else
                {
                    throw new InvalidOperationException("Don't know how to parse tree");
                }
            }

            return stack.Peek();
        }

        public class SimpleTreeNode
        {
            public SimpleTreeNode()
            {

            }

            public string Content { get; set; }

            public List<SimpleTreeNode> Children { get; }  = new List<SimpleTreeNode>();

            public override string ToString()
            {
                if (Children.Any())
                {
                    var children = string.Join(", ", Children);
                    return $"({Content} : {children})";
                }
                else
                {
                    return $"({Content})";
                }
            }
        }
    }
}