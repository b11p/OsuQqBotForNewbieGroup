using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Xunit;

namespace UnitTests
{
    public class RegexTest
    {
        [Fact]
        public void NullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new NullService().Test("");
            });
        }

        [Fact]
        public void DuplicateTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new DuplicateService().Test("");
            });
        }

        [Fact]
        public void CompTest()
        {
            var service = new MyService();
            var text = "3.5|this is text|467";

            var result = service.Test(text);

            Assert.True(result);
            Assert.Equal(3.5, service.Real);
            Assert.Equal("this is text", service.TextProperty);
            Assert.Equal(467, service.MyProperty);
        }

        public class MyService : Service
        {
            [Parameter("1")]
            public int MyProperty { get; set; }

            [Parameter("t")]
            public string TextProperty { get; set; }

            [Parameter("r")]
            public double Real { get; set; }

            public bool Test(string text)
            {
                return RegexCommand(new Regex(@"^(?<r>.+?)\|(?<t>.+)\|(.+?)$"), text);
            }
        }

        public class NullService : Service
        {
            [Parameter(null)]
            public int MyProperty { get; set; }

            public bool Test(string text)
            {
                return RegexCommand(new Regex(""), text);
            }
        }

        public class DuplicateService : Service
        {
            [Parameter("ss")]
            public int MyProperty { get; set; }

            [Parameter("ss")]
            public int MyProperty2 { get; set; }

            public bool Test(string text)
            {
                return RegexCommand(new Regex(""), text);
            }
        }
    }
}
