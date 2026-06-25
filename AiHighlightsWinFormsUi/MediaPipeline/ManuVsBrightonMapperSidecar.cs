namespace AiHighlightsWinFormsUi.MediaPipeline;

public static class ManuVsBrightonMapperSidecar
{
    // TODO:- this becomes the media sidecar file

    /*

    Opta Match time to Media time mapping
    -------------------------------------

                              Opta match time
          ------------------------------       ------------------------------         
         | 0       period 1      46:15  |     | 45       period 2      95:07 |   
          ------------------------------       ------------------------------     


                        Media Time (Youtube test media)  
     ------------------------------------------------------------------------------      
    | XXX | 7                     46:21 | XXX | 50:04                  96:55 | XXX |
     ------------------------------------------------------------------------------
    run-in                       half-time gap 30 seconds                     run-out

    */

    // durations measured from the ManU v Brighton 1920x1080 GOP25.mp4 test media file
    public const int mediaRunInSeconds = 7;                    // kick-off happens 7 seconds into the video
    public const int mediaInterPeriodGapSeconds = 30;          // the edited video retains 30 seconds of half-time content (from 46:21 to 46:51)
    public const int mediaPeriod1Seconds = 2775;               // period 1 runs from 0:08 to 46:15 in the media file = 2775s (46:15 agrees with Opta)
    public const int mediaPeriod2Seconds = 3003;               // period 2 runs from 46:51 to 96:54 in the media file = 3003s (50:03 agrees with Opta)
    public const int mediaDurationSeconds = 5832;

    public const int period1 = 1;
    public const int period2 = 2;
    public const int period3 = 3;
    public const int period4 = 4;
    public const int mediaPeriod1KickOff = mediaRunInSeconds;
    public const int mediaPeriod2KickOff = mediaPeriod1KickOff + mediaPeriod1Seconds + mediaInterPeriodGapSeconds;

    public static MediaTimeMapConfig StandardConfig() => new()
    {
        RunInSeconds = mediaRunInSeconds,
        MediaPeriodDurationsMapSeconds = new() { { period1, mediaPeriod1Seconds }, { period2, mediaPeriod2Seconds } },
        OptaPeriodStartMapMinutes = new() { { period1, 0 }, { period2, 45 } },
        InterPeriodGapSeconds = new() { { period1, mediaInterPeriodGapSeconds }, { period2, 0 } },
    };
}
