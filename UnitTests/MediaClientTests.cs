using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsWinFormsUi.MediaPipeline;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class MediaClientTests
    {
        // Example: a goal wants the build-up and the celebration; a foul much less.
        public static Dictionary<string, PaddingRule> paddingRules = new Dictionary<string, PaddingRule>
        {
            ["Goal"] = new(15, 15),
            ["Foul"] = new(5, 5),
            ["Card"] = new(10, 10),   // show the offence that earned it
        };

        public static MatchEventList sampleEvents = new MatchEventList(new[]
        {
            new MatchEvent("Goal", "B. Gruda", "Brighton \u0026 Hove Albion", 1, 11, 11),
            new MatchEvent("Card", "K. Mainoo", "Manchester United", 2, 59, 33),
            new MatchEvent("Goal", "D. Welbeck", "Brighton \u0026 Hove Albion", 2, 63, 56),
            new MatchEvent("Goal", "B. \u0160e\u0161ko", "Manchester United", 2, 84, 36),
            new MatchEvent("Card", "C. Kostoulas", "Brighton & Hove Albion", 2, 92, 4),
        });

        public static MediaTimeMapper mediaTimeMapper;
        public static HighlightBuilder highlightBuilder;

        //------------------------------------------------------------------------------
        [ClassInitialize]
        public static void ClassInitialise(TestContext testContext)
        {
            Debug.WriteLine("\nClassInitialise");

            mediaTimeMapper = new MediaTimeMapper(ManuVsBrightonMapperContext.StandardConfig());
            highlightBuilder = new HighlightBuilder(mediaTimeMapper, TimeSpan.FromSeconds(ManuVsBrightonMapperContext.mediaDurationSeconds), paddingRules);
        }

        //------------------------------------------------------------------------------
        [ClassCleanup]
        public static void ClassCleanup()
        {
            Debug.WriteLine("\nClassCleanup");
        }

        //------------------------------------------------------------------------------
        [TestInitialize]
        public async Task TestInitialise()
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
        public void HighlightBuilderTest()
        {
            //Goal — B.Gruda - Brighton & Hove Albion(11:11): In = 00:11:03 - Out = 00:11:33
            //Card — K.Mainoo - Manchester United(59:33): In = 01:01:15 - Out = 01:01:35
            //Goal — D.Welbeck - Brighton & Hove Albion(63:56): In = 01:05:33 - Out = 01:06:03
            //Goal — B.Šeško - Manchester United(84:36): In = 01:26:13 - Out = 01:26:43
            //Card — C.Kostoulas - Brighton & Hove Albion(92:04): In = 01:33:46 - Out = 01:34:06

            HighlightSegmentList highlights =  highlightBuilder.Build(sampleEvents);

            // write the results to the debug console
            foreach (var segment in highlights.Segments)
            {
                Debug.WriteLine($"{segment.Label}: In={segment.In} - Out={segment.Out}");
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
