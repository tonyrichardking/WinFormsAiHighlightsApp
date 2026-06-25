using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsWinFormsUi.MediaPipeline;
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
    public class MediaPipelineUnitTests
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
        public async Task TestPlayer()
        {
            await MediaPlayer.PlayAsync(@"C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\Highlights\Highlights.mp4");
        }
    }
}