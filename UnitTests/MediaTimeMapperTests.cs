using AiHighlightsMcpServer.Services;

namespace UnitTests;

[TestClass]
public class MediaTimeMapperTests
{
    // durations measured from the ManU v Brighton 1920x1080 GOP25.mp4 test media file
    const int mediaRunInSeconds = 7;                    // kick-off happens 7 seconds into the video
    const int mediaInterPeriodGapSeconds = 30;          // the edited video retains 30 seconds of half-time content (from 46:21 to 46:51)
    const int mediaPeriod1Seconds = 2775;               // period 1 runs from 0:08 to 46:15 in the media file = 2775s (46:15 agrees with Opta)
    const int mediaPeriod2Seconds = 3003;               // period 2 runs from 46:51 to 96:54 in the media file = 3003s (50:03 agrees with Opta)

    const int period1 = 1;
    const int period2 = 2;
    const int period3 = 3;
    const int period4 = 4;
    const int mediaPeriod1KickOff = mediaRunInSeconds;
    const int mediaPeriod2KickOff = mediaPeriod1KickOff + mediaPeriod1Seconds + mediaInterPeriodGapSeconds;

    private static MediaTimeMapConfig StandardConfig() => new()
    {
        RunInSeconds = mediaRunInSeconds,
        MediaPeriodDurationsMapSeconds = new() { { period1, mediaPeriod1Seconds }, { period2, mediaPeriod2Seconds } },
        OptaPeriodStartMapMinutes     = new() { { period1, 0 }, { period2, 45 } },
        InterPeriodGapSeconds  = new() { { period1, mediaInterPeriodGapSeconds }, { period2, 0 } },
    };

    // -------------------------------------------------------------------------
    // Period 1 mapping
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period1_KickOff_MapsToRunIn()
    {
        // timeMin=0, timeSec=0 is exactly kick-off — media offset should equal RunInSeconds
        var mapper = new MediaTimeMapper(StandardConfig());
        var mediaTimeResult = mapper.OptaToMediaTime(period1, 0, 0);
        var wanted = TimeSpan.FromSeconds(mediaPeriod1KickOff);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }

    [TestMethod]
    public void Period1_MidMinute_OffsetAddedCorrectly()
    {
        // timeMin=1, timeSec=30 → 90 seconds into period 1 → media = 7 + 90 = 97s
        var mapper = new MediaTimeMapper(StandardConfig());
        var mediaTimeResult = mapper.OptaToMediaTime(period1, 1, 30);
        var wanted = TimeSpan.FromSeconds(97);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }

    [TestMethod]
    public void Period1_Minute45_StoppageTime_OffsetCorrect()
    {
        // timeMin=45, timeSec=0 = 2700s into P1 → media = 7 + 2700 = 2707s
        var mapper = new MediaTimeMapper(StandardConfig());
        var mediaTimeResult = mapper.OptaToMediaTime(period1, 45, 0);
        var wanted = TimeSpan.FromSeconds(2707);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }

    // -------------------------------------------------------------------------
    // Period 2 mapping — clock resets to 45
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period2_KickOff_MapsToRunInPlusPeriod1DurationPlusGap()
    {
        // P2 kick-off: timeMin=45, timeSec=0 → 0s into P2
        // media = RunIn(7) + P1Duration(2775) + HalfTimeGap(30) + 0 = 2812s
        var mapper = new MediaTimeMapper(StandardConfig());
        var mediaTimeResult = mapper.OptaToMediaTime(period2, 45, 0);
        var wanted = TimeSpan.FromSeconds(mediaPeriod2KickOff);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }

    [TestMethod]
    public void Period2_EarlyEvent_OffsetCorrect()
    {
        // timeMin=45, timeSec=30 → 30s into P2 → media = 2812 + 30 = 2842s
        var mapper = new MediaTimeMapper(StandardConfig());
        var mediaTimeResult = mapper.OptaToMediaTime(period2, 45, 30);
        var wanted = TimeSpan.FromSeconds(mediaPeriod2KickOff + 30);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }

    [TestMethod]
    public void Period2_Minute90_FullTime_OffsetCorrect()
    {
        // timeMin=90, timeSec=0 → 2700s into P2 → media = 2812 + 2700 = 5512s
        var mapper = new MediaTimeMapper(StandardConfig());
        var mediaTimeResult = mapper.OptaToMediaTime(period2, 90, 0);
        var wanted = TimeSpan.FromSeconds(mediaPeriod2KickOff + 45 * 60);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }

    // -------------------------------------------------------------------------
    // Half-time gap retained in the edit
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period2_KickOff_MapsToRunInPlusPeriod1DurationPlusIncreasedGap()
    {
        // Same as above but half-time gap = 600s retained in the edit
        var config = StandardConfig();
        config.InterPeriodGapSeconds[1] = 600;

        var mapper = new MediaTimeMapper(config);
        var mediaTimeResult = mapper.OptaToMediaTime(period2, 45, 0);
        var wanted = TimeSpan.FromSeconds(mediaRunInSeconds + mediaPeriod1Seconds + config.InterPeriodGapSeconds[1]);
        Assert.AreEqual(wanted, mediaTimeResult.Offset);
    }


/*
    // -------------------------------------------------------------------------
    // Extra time stubs (P3 / P4)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period3_KickOff_MapsCorrectly()
    {
        // ET config: P3 kick-off at timeMin=90, no ET break in video
        // media = RunIn(150) + P1(2880) + Gap1(0) + P2(2820) + Gap2(0) + 0 = 5850s
        var config = StandardConfig();
        config.MediaPeriodDurationsMapSeconds[3] = 0; // not played yet, stub only

        var mapper = new MediaTimeMapper(config);
        var mediaTimeResult = mapper.OptaToMediaTime(period3, 90, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(5850), mediaTimeResult.Offset);
    }

    [TestMethod]
    public void Period4_KickOff_MapsCorrectly()
    {
        // ET P1 = 10 min (600s), no breaks retained
        // media = 150 + 2880 + 0 + 2820 + 0 + 600 + 0 + 0 = 6450s
        var config = StandardConfig();
        config.MediaPeriodDurationsMapSeconds[3] = 600;

        var mapper = new MediaTimeMapper(config);
        var mediaTimeResult = mapper.OptaToMediaTime(period4, 105, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(6450), mediaTimeResult.Offset);
    }

*/

    // -------------------------------------------------------------------------
    // No run-in edge case
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ZeroRunIn_Period1_KickOff_MapsToZero()
    {
        var config = StandardConfig();
        config.RunInSeconds = 0;

        var mapper = new MediaTimeMapper(config);
        var mediaTimeResult = mapper.OptaToMediaTime(period1, 0, 0);
        Assert.AreEqual(TimeSpan.Zero, mediaTimeResult.Offset);
    }

    // -------------------------------------------------------------------------
    // String overload (MM:SS)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void StringOverload_ParsesCorrectly()
    {
        var mapper = new MediaTimeMapper(StandardConfig());
        var byInts   = mapper.OptaToMediaTime(period1, 12, 34);
        var byString = mapper.OptaToMediaTime(period1, "12:34");
        Assert.AreEqual(byInts.Offset, byString.Offset);
    }

    [TestMethod]
    public void StringOverload_BadFormat_Throws()
    {
        var mapper = new MediaTimeMapper(StandardConfig());
        Assert.ThrowsExactly<FormatException>(() => mapper.OptaToMediaTime(period1, "12-34"));
    }

    // -------------------------------------------------------------------------
    // FFmpeg timecode string format
    // -------------------------------------------------------------------------

    [TestMethod]
    public void FfmpegTimecode_FormatsAsHhMmSs()
    {
        // 3661 seconds = 1h 1m 1s
        var tc = new MediaTimecode(TimeSpan.FromSeconds(3661));
        Assert.AreEqual("01:01:01", tc.ToFfmpegTimecode());
    }

    [TestMethod]
    public void FfmpegTimecode_ZeroOffset_FormatsCorrectly()
    {
        var tc = new MediaTimecode(TimeSpan.Zero);
        Assert.AreEqual("00:00:00", tc.ToFfmpegTimecode());
    }

    [TestMethod]
    public void ToString_MatchesFfmpegTimecode()
    {
        var tc = new MediaTimecode(TimeSpan.FromSeconds(5400));
        Assert.AreEqual(tc.ToFfmpegTimecode(), tc.ToString());
    }

    // -------------------------------------------------------------------------
    // Unknown period throws
    // -------------------------------------------------------------------------

    [TestMethod]
    public void UnknownPeriod_Throws()
    {
        var mapper = new MediaTimeMapper(StandardConfig());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => mapper.OptaToMediaTime(99, 0, 0));
    }

    // -------------------------------------------------------------------------
    // Sidecar round-trip via JSON
    // -------------------------------------------------------------------------

    [TestMethod]
    public void SidecarJson_RoundTrip_PreservesConfig()
    {
        var original = StandardConfig();
        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<MediaTimeMapConfig>(json)!;

        Assert.AreEqual(original.RunInSeconds,  deserialized.RunInSeconds);
        Assert.AreEqual(original.MediaPeriodDurationsMapSeconds[1], deserialized.MediaPeriodDurationsMapSeconds[1]);
        Assert.AreEqual(original.MediaPeriodDurationsMapSeconds[2], deserialized.MediaPeriodDurationsMapSeconds[2]);
        Assert.AreEqual(original.OptaPeriodStartMapMinutes[2],     deserialized.OptaPeriodStartMapMinutes[2]);
        Assert.AreEqual(original.InterPeriodGapSeconds[1],  deserialized.InterPeriodGapSeconds[1]);
    }

    // -------------------------------------------------------------------------
    // OptaMinuteBase is visible for documentation / override reference
    // -------------------------------------------------------------------------

    [TestMethod]
    public void OptaMinuteBase_IsZero()
    {
        Assert.AreEqual(0, MediaTimeMapper.OptaMinuteBase);
    }
}
