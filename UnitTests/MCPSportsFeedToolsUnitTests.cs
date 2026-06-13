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
    public class MCPSportsFeedToolsUnitTests
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
        public void TestSportFeedFilterQueries()
        {
            object wantedItems
                = SportsFeedProcessorTools.ReadSportFeed(
                    "root-liveData-event_items",
                    bindingsJson: """{"typeId": 16}""",
                    wantedAttributes: new List<string> { "typeId", "playerName", "timeMin", "timeSec", "periodId" }).Result;

            Assert.IsNotNull(wantedItems);
        }

        [TestMethod]
        public void TestSportFeedQueries()
        {
            var wantedLiveDataEventAttributes = new List<string> { "playerName", "timeMin", "timeSec", "periodId" };
            const string liveDataEventBindingsJson = """{"typeId": "16"}""";

            // liveData unfiltered
            object sportFeedLiveData = SportsFeedProcessorTools.ReadSportFeed("root-liveData").Result;
            object sportFeedLiveDataEvent = SportsFeedProcessorTools.ReadSportFeed("root-liveData-event_items").Result;
            object sportFeedLiveDataEventQualifier = SportsFeedProcessorTools.ReadSportFeed("root-liveData-event-qualifier_items").Result;

            // liveData filtered
            object sportFeedLiveDataFiltered
                = SportsFeedProcessorTools.ReadSportFeed("root-liveData", bindingsJson: liveDataEventBindingsJson, wantedAttributes: wantedLiveDataEventAttributes).Result;
            object sportFeedLiveDataEventFiltered
                = SportsFeedProcessorTools.ReadSportFeed("root-liveData-event_items", bindingsJson: liveDataEventBindingsJson, wantedAttributes: wantedLiveDataEventAttributes).Result;
            object sportFeedLiveDataQualifiersFiltered
                = SportsFeedProcessorTools.ReadSportFeed("root-liveData-event-qualifier_items", bindingsJson: liveDataEventBindingsJson, wantedAttributes: wantedLiveDataEventAttributes).Result;

            // liveData-matchDetails
            object sportFeedLiveDataMatchDetails = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails").Result;
            object sportFeedLiveDataMatchDetailsPeriod = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-period_items").Result;
            object SportFeedLiveDataMatchDetailsScoresText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores").Result;
            object SportFeedLiveDataMatchDetailsScoresEtText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores-et").Result;
            object SportFeedLiveDataMatchDetailsScoresFtText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores-ft").Result;
            object SportFeedLiveDataMatchDetailsScoresHtText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores-ht").Result;
            object SportFeedLiveDataMatchDetailsScoresPenText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores-pen").Result;
            object SportFeedLiveDataMatchDetailsScoresTotalText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores-total").Result;
            object SportFeedLiveDataMatchDetailsScoresTotalUnconfirmedText = SportsFeedProcessorTools.ReadSportFeed("root-liveData-matchDetails-scores-totalUnconfirmed").Result;

            // matchInfo
            object SportFeedMatchInfoText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo_items").Result;
            object SportFeedMatchInfoCompetitionText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-competition_items").Result;
            object SportFeedMatchInfoCompetitionCountryText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-competition-country_items").Result;
            object SportFeedMatchInfoContestantText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-contestant_items").Result;
            object SportFeedMatchInfoContestantCountryText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-contestant-country_items").Result;

            object SportFeedMatchInfoRulesetText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-ruleset_items").Result;
            object SportFeedMatchInfoSeriesText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-series_items").Result;
            object SportFeedMatchInfoSportText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-sport_items").Result;
            object SportFeedMatchInfoStageText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-stage_items").Result;
            object SportFeedMatchInfoTournamentCalendarText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-tournamentCalendar_items").Result;
            object SportFeedMatchInfoVenueText = SportsFeedProcessorTools.ReadSportFeed("root-matchInfo-venue_items").Result;

            // Assert.AreEqual(1, JsonSerializer.Deserialize<JsonElement[]>(SportFeedLiveDataText).Length);
        }

        [TestMethod]
        public void TestSportFeedItemQueries()
        {
            // "root-liveData
            string SportFeedLiveDataEventItemsText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-event_items", "2581926701").Result;
            string SportFeedLiveDataEventQualifierItemsText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-event-qualifier_items", "4464621259").Result;
            string SportFeedLiveDataMatchDetailsPeriodText = SportsFeedProcessorTools.ReadSportFeedItem("root-liveData-matchDetails-period_items", "1").Result;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
