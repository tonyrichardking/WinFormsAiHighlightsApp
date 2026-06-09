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
    public class MCPSchemaUnitTests
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
        public void TestMCPSchemaTools()
        {
            string descriptionNesting = "root-matchInfo-description";
            string contestantsItemNesting = "root-matchInfo-contestant_items";
            string eventsItemNesting = "root-liveData-event_items";

            // contestant item nesting
            string contestantItemId = "6eqit8ye8aomdsrrq0hk3v7gh";
            string contestantJsonSchemaText = SportsFeedProcessorTools.ReadSchemaDocument(contestantsItemNesting).Result;
            //object contestantSportFeedItemsText = SportsFeedProcessorTools.ReadSportFeed(contestantsItemNesting).Result;
            string contestantSportFeedItemText = SportsFeedProcessorTools.ReadSportFeedItem(contestantsItemNesting, contestantItemId).Result;

            // event item nesting
            string eventItemId = "2581965521";
            string eventJsonSchemaText = SportsFeedProcessorTools.ReadSchemaDocument(eventsItemNesting).Result;
            //object eventSportFeedItemsText = SportsFeedProcessorTools.ReadSportFeed(eventsItemNesting).Result;
            string eventSportFeedItemText = SportsFeedProcessorTools.ReadSportFeedItem(eventsItemNesting, eventItemId).Result;

            string schemaIndexText = SportsFeedProcessorTools.ReadSchemaIndex().Result;

            //// list of event item attributes
            //List<Dictionary<string, string>> eventItemAttributes = SportsFeedProcessorTools.ReadSportFeedAttributes
            //    (eventsItemNesting, new List<string> { "playerName", "timeMin", "timeSec" }).Result;

            // read eventMap
            string eventMap = SportsFeedProcessorTools.ReadEventMap().Result;

            // read qualifierMap
            string qualifierMap = SportsFeedProcessorTools.ReadQualifierMap().Result;

            // read event for typeId
            string eventAttrs = SportsFeedProcessorTools.ReadEventAttributesForTypeId(16).Result;

            // read qualifier for qualifierId
            string qualifierAttrs = SportsFeedProcessorTools.ReadQualifierAttributesForQualifierId(13).Result;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
