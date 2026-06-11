using System.Text.Json;

namespace AiHighlightsMcpServer.Services;

/// <summary>
/// A point in time within a media file.
/// Currently a thin wrapper over TimeSpan; designed to grow into a Curator-compatible
/// timecode (SMPTE, frame number, asset reference, edit version) without changing
/// the mapper's public signature.
/// </summary>
public record MediaTimecode(TimeSpan Offset)
{
    /// <summary>Returns HH:MM:SS — suitable as an FFmpeg -ss / -to argument.</summary>
    public string ToFfmpegTimecode() => Offset.ToString(@"hh\:mm\:ss");
    public override string ToString() => ToFfmpegTimecode();
}

/// <summary>
/// Sidecar JSON schema describing how a specific media file maps to match time.
/// One file per media asset, co-located with the media file.
/// Naming convention: {mediaFilename}.timemap.json
/// </summary>
public class MediaTimeMapConfig
{
    /// <summary>Seconds from video start to period-1 kick-off (the run-in).</summary>
    public int RunInSeconds { get; set; }

    /// <summary>
    /// Actual video duration (in seconds) of each period, measured from the media file.
    /// These cannot be derived from Opta because stoppage-time minutes are not 1:1 with
    /// real elapsed seconds. Measure using a media player.
    /// Periods not listed here are treated as zero-duration stubs (e.g. ET when not played).
    /// </summary>
    public Dictionary<int, int> MediaPeriodDurationsMapSeconds { get; set; } = new();

    /// <summary>
    /// The Opta timeMin value at which each period's clock begins.
    /// Opta resets the minute counter at each period boundary:
    ///   P1 = 0, P2 = 45, ET first half (P3) = 90, ET second half (P4) = 105, Penalties (P5) = 120
    /// Override here if your feed uses a different convention.
    /// </summary>
    public Dictionary<int, int> OptaPeriodStartMapMinutes { get; set; } = new()
    {
        { 1,   0 },
        { 2,  45 },
        { 3,  90 },
        { 4, 105 },
        { 5, 120 },
    };

    /// <summary>
    /// Seconds of inter-period content retained in the edited video.
    /// Key = the period that just finished (1 = half-time gap, 2 = full-time gap, etc.).
    /// Set to 0 when the break was fully cut (typical for YouTube test media).
    /// </summary>
    public Dictionary<int, int> InterPeriodGapSeconds { get; set; } = new()
    {
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
    };
}

public class MediaTimeMapper
{
    private readonly MediaTimeMapConfig _config;

    // Opta timeMin is 0-based: timeMin=0, timeSec=0 is the kick-off of period 1.
    // Period 2 resets to 45, ET periods to 90/105/120 (see PeriodStartMinutes).
    // Change this constant if a different feed uses 1-based minutes.
    public const int OptaMinuteBase = 0;

    public MediaTimeMapper(MediaTimeMapConfig config) => _config = config;

    public static MediaTimeMapper FromSidecarFile(string sidecarPath)
    {
        var json = File.ReadAllText(sidecarPath);
        var config = JsonSerializer.Deserialize<MediaTimeMapConfig>(json)
            ?? throw new InvalidDataException($"Could not deserialise sidecar: {sidecarPath}");
        return new MediaTimeMapper(config);
    }

    /// <summary>
    /// Converts an Opta event time to an offset from the start of the media file.
    /// </summary>
    /// <param name="period">Opta period: 1=P1, 2=P2, 3=ET first half, 4=ET second half, 5=Penalties</param>
    /// <param name="timeMin">
    /// Opta timeMin — 0-based, resets at each period boundary.
    /// P1 starts at 0; P2 resets to 45; ET periods reset to 90, 105, 120.
    /// </param>
    /// <param name="timeSec">Opta timeSec (0–59)</param>
    public MediaTimecode OptaToMediaTime(int period, int timeMin, int timeSec)
    {
        if (!_config.OptaPeriodStartMapMinutes.TryGetValue(period, out int periodStartMinute))
            throw new ArgumentOutOfRangeException(nameof(period), $"Unknown period: {period}");

        // Opta offset in seconds from the start of this period
        int optaOffsetSeconds = (timeMin - periodStartMinute) * 60 + timeSec;

        int mediaOffsetSeconds = _config.RunInSeconds
            + PrecedingPeriodsSeconds(period)
            + PrecedingGapsSeconds(period)
            + optaOffsetSeconds;

        return new MediaTimecode(TimeSpan.FromSeconds(mediaOffsetSeconds));
    }

    /// <summary>
    /// Convenience overload parsing an Opta time string in "MM:SS" format.
    /// </summary>
    public MediaTimecode OptaToMediaTime(int period, string optaTimeString)
    {
        var parts = optaTimeString.Split(':');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out int min)
            || !int.TryParse(parts[1], out int sec))
            throw new FormatException($"Expected MM:SS format, got: {optaTimeString}");

        return OptaToMediaTime(period, min, sec);
    }

    /// <summary>Exposes the loaded config — useful for diagnostics and testing.</summary>
    public MediaTimeMapConfig Config => _config;

    // Sum of video durations of all periods preceding the given one.
    private int PrecedingPeriodsSeconds(int period)
    {
        int total = 0;
        for (int p = 1; p < period; p++)
            total += _config.MediaPeriodDurationsMapSeconds.GetValueOrDefault(p, 0);
        return total;
    }

    // Sum of inter-period gap durations for all gaps preceding the given period.
    private int PrecedingGapsSeconds(int period)
    {
        int total = 0;
        for (int p = 1; p < period; p++)
            total += _config.InterPeriodGapSeconds.GetValueOrDefault(p, 0);
        return total;
    }
}
