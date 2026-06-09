////////////////////////////////////////////////////////////////////////////////
//
////////////////////////////////////////////////////////////////////////////////

using MCPServer.MCPTools;
using System.Diagnostics;

////////////////////////////////////////////////////////////////////////////////

namespace UnitTests
{
    //------------------------------------------------------------------------------
    [TestClass]
    public class MCPErrorHandlingUnitTests
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
        public void TestErrorHandling()
        {
            // these nestings don't support ids

            // "root-liveData
            string SportFeedLiveDataItemsText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData", "0").Result;
            string SportFeedLiveDataMatchDetailsText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-matchDetails_items", "0").Result;
            string SportFeedLiveDataMatchDetailsScoresText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-matchDetails-scores", "0").Result;
            string SportFeedLiveDataMatchDetailsFullTimeScoresText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-matchDetails-scores-ft_items", "0").Result;

            // root-matchInfo
            string SportFeedMatchInfoText = SportsFeedProcessorTools.ReadSportFeedItem("root-matchInfo_items", "0").Result;
            string SportFeedMatchInfoCompetitionText = SportsFeedProcessorTools.ReadSportFeedItem("root-matchInfo-competition_items", "0").Result;
            string SportFeedMatchInfoCompetitionCountryText = SportsFeedProcessorTools.ReadSportFeedItem("root-matchInfo-competition-counry_items", "0").Result;

            // period value can only be 1 or 2
            string SportFeedLiveDataMatchDetailsPeriodText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-matchDetails-period_items", "0").Result;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
