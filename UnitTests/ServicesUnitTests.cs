////////////////////////////////////////////////////////////////////////////////
//
////////////////////////////////////////////////////////////////////////////////

using ContentExtraction;
using MCPServer.MCPTools;
using System.Diagnostics;
using System.Text.Json;
using TestServices;

////////////////////////////////////////////////////////////////////////////////

namespace UnitTests
{
    //------------------------------------------------------------------------------
    [TestClass]
    public class ServicesUnitTests
    {
        //------------------------------------------------------------------------------
        [ClassCleanup]
        public static void ClassCleanup()
        {
            Debug.WriteLine("\nClassCleanup");
        }

        //------------------------------------------------------------------------------
        [TestInitialize]
        public void TestInitialise()
        {
            Debug.WriteLine("\nTestInitialise");
        }

        //------------------------------------------------------------------------------
        [TestCleanup]
        public void TestCleanup()
        {
            Debug.WriteLine("\nTestCleanup");
        }

        [TestMethod]
        public void TestMa3ObjectFeedReader()
        {
            Ma3FeedDataProviderService feedData = new Ma3FeedDataProviderService(null);

            var result = feedData.ReadMa3Objects().Result;

            Assert.AreEqual(result.liveData.@event.Count(), 1745); 
        }

        [TestMethod]
        public void TestMa3JsonFeedReader()
        {
            Ma3FeedDataProviderService feedData = new Ma3FeedDataProviderService(null);

            var result = feedData.ReadMa3Json().Result;

            Dictionary<string, object> list = SportsFeedProcessorToolHelpers.JsonHelper.Deserialize(result) as Dictionary<string, object>;

            //Assert.AreEqual(result.liveData.@event.Count(), 1745);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
