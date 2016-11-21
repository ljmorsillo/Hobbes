using Microsoft.VisualStudio.TestTools.UnitTesting;
using ircda.hobbes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ircda.hobbes.Tests
{
    [TestClass()]
    public class ContextDriverTests
    {
        [TestMethod()]
        public void CheckConfidenceTest()
        {
            try
            {
                ContextDriver.CheckConfidence(null);  // this should fail nicely
            }
            catch(Exception ex)
            {
                Assert.Fail("Problem: Check Context Failure {0} @ {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}