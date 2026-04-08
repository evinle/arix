using System;

namespace ArixBack.Services
{
    public class MathProblemGenerator
    {
        private static readonly Random _random = new Random();

        public MathProblem Generate(int tier)
        {
            return tier switch
            {
                1 => GenerateTier1(),
                2 => GenerateTier2(),
                3 => GenerateTier3(),
                _ => GenerateTier1()
            };
        }

        private MathProblem GenerateTier1()
        {
            // Addition and Subtraction
            int a = _random.Next(1, 100);
            int b = _random.Next(1, 100);
            bool isAdd = _random.Next(0, 2) == 0;

            if (isAdd)
            {
                return new MathProblem($"{a} + {b}", (a + b).ToString());
            }
            else
            {
                // Ensure no negative results for simplicity
                if (a < b) (a, b) = (b, a);
                return new MathProblem($"{a} - {b}", (a - b).ToString());
            }
        }

        private MathProblem GenerateTier2()
        {
            // Multiplication and Division
            bool isMul = _random.Next(0, 2) == 0;

            if (isMul)
            {
                int a = _random.Next(2, 13);
                int b = _random.Next(2, 21);
                return new MathProblem($"{a} * {b}", (a * b).ToString());
            }
            else
            {
                int b = _random.Next(2, 13);
                int result = _random.Next(2, 21);
                int a = b * result;
                return new MathProblem($"{a} / {b}", result.ToString());
            }
        }

        private MathProblem GenerateTier3()
        {
            // Square roots and Exponentials
            bool isSqrt = _random.Next(0, 2) == 0;

            if (isSqrt)
            {
                int result = _random.Next(2, 21);
                int a = result * result;
                return new MathProblem($"√{a}", result.ToString());
            }
            else
            {
                int a = _random.Next(2, 10);
                int b = _random.Next(2, 4); // Limited to x^2, x^3
                int result = (int)Math.Pow(a, b);
                return new MathProblem($"{a}^{b}", result.ToString());
            }
        }
    }

    public class MathProblem
    {
        public string Text { get; }
        public string Answer { get; }

        public MathProblem(string text, string answer)
        {
            Text = text;
            Answer = answer;
        }
    }
}
