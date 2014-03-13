using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Calculator
{
    public class StringCalculator
    {
        public int Add(string numbers)
        {
            var splitedString = SplitStringToNumbers(numbers);

            var negatives = new List<int>();

            var sum = splitedString.Aggregate(0, (i, s) =>
            {
                var tmp = ParseNumber(s);

                if (tmp < 0)
                {
                    negatives.Add(tmp);
                }

                return tmp + i;
            });

            if (negatives.Count > 0)
            {
                throw new Exception("negatives not allowed: " + string.Join(",", negatives));
            }

            return sum;
        }

        private int ParseNumber(string s)
        {
            int tmp;
            int.TryParse(s, out tmp);

            if (tmp > 1000)
            {
                tmp = 0;
            }

            return tmp;
        }

        private static string[] SplitStringToNumbers(string numbers)
        {
            var regex = new Regex(@"^//(?:\[?(?<delimiter>[^\[\]]+)\]?)+\n(?<numbers>.*)");

            var match = regex.Match(numbers);
            var separator = new[] {",", "\n"};
            
            if (match.Success)
            {
                separator = match.Groups["delimiter"].Captures
                                    .Cast<Capture>()
                                    .Select(x => x.Value).ToArray();

                numbers = match.Groups["numbers"].Value;
            }
            
            return numbers.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class TestStringCalculator
    {
        [TestCase("", 0, Description = "Empty string returns 0")]
        [TestCase("1", 1, Description = "String of \"0\" returns 1")]
        [TestCase("1,2", 3, Description = "String of \"1,2\" returns 3")]
        [TestCase("1,2,3", 6, Description = "String of \"1,2,3\" returns 6")]
        [TestCase("1\n2\n3", 6, Description = "String with newlines delimiters")]
        [TestCase("1\n2,3", 6, Description = "String with mixed newline and comma returns correct value")]
        [TestCase("//;\n1;2;3", 6, Description = "custom dilimiter returns correct result")]
        [TestCase("2, 1001", 2, Description = "Numbers bigger than 1000 should be ignored")]
        [TestCase("//[***]\n1***2***3", 6, Description = "Delimiters can be of any length with the following format")]
        [TestCase("//[*][%]\n1*2%3", 6, Description = "Allow multiple custom delimiters.")]
        [TestCase("//[**][%%]\n1**2%%3", 6, Description = "Allow multiple delimiters with length longer than one char.")]
        public void TestAdd(string input, int expected)
        {
            // Arrange
            var calc = new StringCalculator();

            // Act
            var actual = calc.Add(input);
            
            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCase("-1","negatives not allowed: -1")]
        [TestCase("-1,-2,-3", "negatives not allowed: -1,-2,-3")]
        [TestCase("-1,2,-3", "negatives not allowed: -1,-3")]
        public void NegativeNumberThrowsException(string input, string expectMessage)
        {
            // Arrange
            var calc = new StringCalculator();
            
            try
            {
                // Act
                calc.Add(input);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(ex.Message, expectMessage);
                return;
            }

            Assert.Fail("Exception should be thrown");

        }
    }
}
