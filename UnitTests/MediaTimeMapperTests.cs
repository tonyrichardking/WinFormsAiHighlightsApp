using AiHighlightsMcpServer.Services;

namespace UnitTests;

[TestClass]
public class MediaTimeMapperTests
{
    // Shared config: 2m30s run-in, P1=48min video, P2=47min video, half-time fully cut.
    private static MediaTimeMapConfig StandardConfig() => new()
    {
        RunInSeconds = 150,
        PeriodDurationsSeconds = new() { { 1, 2880 }, { 2, 2820 } },
        PeriodStartMinutes     = new() { { 1, 0 }, { 2, 45 }, { 3, 90 }, { 4, 105 }, { 5, 120 } },
        InterPeriodGapSeconds  = new() { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } },
    };

    // -------------------------------------------------------------------------
    // Period 1 mapping
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period1_KickOff_MapsToRunIn()
    {
        // timeMin=0, timeSec=0 is exactly kick-off — media offset should equal RunInSeconds
        var mapper = new MediaTimeMapper(StandardConfig());
        var result = mapper.OptaToMediaTime(1, 0, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(150), result.Offset);
    }

    [TestMethod]
    public void Period1_MidMinute_OffsetAddedCorrectly()
    {
        // timeMin=1, timeSec=30 → 90 seconds into period 1 → media = 150 + 90 = 240s
        var mapper = new MediaTimeMapper(StandardConfig());
        var result = mapper.OptaToMediaTime(1, 1, 30);
        Assert.AreEqual(TimeSpan.FromSeconds(240), result.Offset);
    }

    [TestMethod]
    public void Period1_Minute45_StoppageTime_OffsetCorrect()
    {
        // timeMin=45, timeSec=0 = 2700s into P1 → media = 150 + 2700 = 2850s
        var mapper = new MediaTimeMapper(StandardConfig());
        var result = mapper.OptaToMediaTime(1, 45, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(2850), result.Offset);
    }

    // -------------------------------------------------------------------------
    // Period 2 mapping — clock resets to 45
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period2_KickOff_MapsToRunInPlusPeriod1DurationPlusGap()
    {
        // P2 kick-off: timeMin=45, timeSec=0 → 0s into P2
        // media = RunIn(150) + P1Duration(2880) + HalfTimeGap(0) + 0 = 3030s
        var mapper = new MediaTimeMapper(StandardConfig());
        var result = mapper.OptaToMediaTime(2, 45, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(3030), result.Offset);
    }

    [TestMethod]
    public void Period2_EarlyEvent_OffsetCorrect()
    {
        // timeMin=45, timeSec=30 → 30s into P2 → media = 3030 + 30 = 3060s
        var mapper = new MediaTimeMapper(StandardConfig());
        var result = mapper.OptaToMediaTime(2, 45, 30);
        Assert.AreEqual(TimeSpan.FromSeconds(3060), result.Offset);
    }

    [TestMethod]
    public void Period2_Minute90_StoppageTime_OffsetCorrect()
    {
        // timeMin=90, timeSec=0 → 2700s into P2 → media = 3030 + 2700 = 5730s
        var mapper = new MediaTimeMapper(StandardConfig());
        var result = mapper.OptaToMediaTime(2, 90, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(5730), result.Offset);
    }

    // -------------------------------------------------------------------------
    // Half-time gap retained in the edit
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period2_KickOff_WithHalfTimeGap_IncludesGapSeconds()
    {
        // Same as above but half-time gap = 600s retained in the edit
        // media = 150 + 2880 + 600 + 0 = 3630s
        var config = StandardConfig();
        config.InterPeriodGapSeconds[1] = 600;

        var mapper = new MediaTimeMapper(config);
        var result = mapper.OptaToMediaTime(2, 45, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(3630), result.Offset);
    }

    // -------------------------------------------------------------------------
    // Extra time stubs (P3 / P4)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Period3_KickOff_MapsCorrectly()
    {
        // ET config: P3 kick-off at timeMin=90, no ET break in video
        // media = RunIn(150) + P1(2880) + Gap1(0) + P2(2820) + Gap2(0) + 0 = 5850s
        var config = StandardConfig();
        config.PeriodDurationsSeconds[3] = 0; // not played yet, stub only

        var mapper = new MediaTimeMapper(config);
        var result = mapper.OptaToMediaTime(3, 90, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(5850), result.Offset);
    }

    [TestMethod]
    public void Period4_KickOff_MapsCorrectly()
    {
        // ET P1 = 10 min (600s), no breaks retained
        // media = 150 + 2880 + 0 + 2820 + 0 + 600 + 0 + 0 = 6450s
        var config = StandardConfig();
        config.PeriodDurationsSeconds[3] = 600;

        var mapper = new MediaTimeMapper(config);
        var result = mapper.OptaToMediaTime(4, 105, 0);
        Assert.AreEqual(TimeSpan.FromSeconds(6450), result.Offset);
    }

    // -------------------------------------------------------------------------
    // No run-in edge case
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ZeroRunIn_Period1_KickOff_MapsToZero()
    {
        var config = StandardConfig();
        config.RunInSeconds = 0;

        var mapper = new MediaTimeMapper(config);
        var result = mapper.OptaToMediaTime(1, 0, 0);
        Assert.AreEqual(TimeSpan.Zero, result.Offset);
    }

    // -------------------------------------------------------------------------
    // String overload (MM:SS)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void StringOverload_ParsesCorrectly()
    {
        var mapper = new MediaTimeMapper(StandardConfig());
        var byInts   = mapper.OptaToMediaTime(1, 12, 34);
        var byString = mapper.OptaToMediaTime(1, "12:34");
        Assert.AreEqual(byInts.Offset, byString.Offset);
    }

    [TestMethod]
    public void StringOverload_BadFormat_Throws()
    {
        var mapper = new MediaTimeMapper(StandardConfig());
        Assert.ThrowsExactly<FormatException>(() => mapper.OptaToMediaTime(1, "12-34"));
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
        Assert.AreEqual(original.PeriodDurationsSeconds[1], deserialized.PeriodDurationsSeconds[1]);
        Assert.AreEqual(original.PeriodDurationsSeconds[2], deserialized.PeriodDurationsSeconds[2]);
        Assert.AreEqual(original.PeriodStartMinutes[2],     deserialized.PeriodStartMinutes[2]);
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
