using System;
using System.Collections.Generic;
using System.Text;
using Bleatingsheep.NewHydrant.Utilities;
using Xunit;

namespace UnitTests
{
    public class IncrementFormatTests
    {

        [Theory]
        [InlineData(0, "")]
        [InlineData(0.03, " (+3%)")]
        [InlineData(0.001, " (+.1%)")]
        [InlineData(0.00001, " (+)")]
        [InlineData(-0.9, " (-90%)")]
        [InlineData(-0.001, " (-.1%)")]
        [InlineData(-0.00015, " (-.02%)")]
        [InlineData(-0.00004, " (-)")]
        [InlineData(-0.00005, " (-.01%)")]
        public void PercentageTests(double percent, string expected)
            => Assert.Equal(expected, IncrementUtility.FormatIncrementPercentage(percent));

        [Theory]
        [InlineData(0, "")]
        [InlineData(1, " (↓1)")]
        [InlineData(-1, " (↑1)")]
        public void DifferentSymbolTests(double increment, string expected)
            => Assert.Equal(expected, IncrementUtility.FormatIncrement(increment, '↓', '↑'));
    }
}
