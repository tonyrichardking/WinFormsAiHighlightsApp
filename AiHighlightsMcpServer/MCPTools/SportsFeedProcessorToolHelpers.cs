using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json.Linq;
using OllamaSharp;
using System.Globalization;
using System.Text.Json;
using TestServices;
using static ContentExtraction.Ma3JsonClasses;
namespace ContentExtraction
{
    /// <summary>
    /// Transplant this file into MCPTools
    /// </summary>
    public class SportsFeedProcessorToolHelpers
    {
        public static class JsonHelper
        {
            public static object Deserialize(string json)
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    return ToObject(doc.RootElement);
                }
            }

            public static object ToObject(JsonElement element)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.Object:
                        return element.EnumerateObject()
                                      .ToDictionary(prop => prop.Name,
                                                    prop => ToObject(prop.Value));

                    case JsonValueKind.Array:
                        return element.EnumerateArray()
                                      .Select(ToObject)
                                      .ToList();

                    case JsonValueKind.String:
                        return element.GetString();

                    case JsonValueKind.Number:
                        if (element.TryGetInt64(out long longValue))
                            return longValue;
                        return element.GetDouble();

                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return element.GetBoolean();

                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                    default:
                        return null;
                }
            }
        }

        public static class FilterHelpers
        {
            public static List<Dictionary<string, object>> DeserializeToRecords(string json)
            {
                switch (JsonHelper.Deserialize(json))
                {
                    case Dictionary<string, object> single:
                        return new List<Dictionary<string, object>> { single };

                    case List<object> many:
                        return many.OfType<Dictionary<string, object>>().ToList();

                    default:
                        return new List<Dictionary<string, object>>(); // empty for scalar/null top-level; throw instead if that's an error
                }
            }

            public static List<Dictionary<string, object>> FilterByAttribute(List<Dictionary<string, object>> inputItems, Dictionary<string, object> bindings, List<string> wantedAttributes, int maxResults = 200)
            {
                List<Dictionary<string, object>> wantedItems = new List<Dictionary<string, object>>();
                if (bindings != null)
                {
                    foreach (var inputItem in inputItems)
                    {
                        // The MCP server SDK binds the JSON arguments using System.Text.Json.  Because binding values are typed object,
                        // STJ doesn't box them as string/int/bool — it leaves each one as a JsonElement (ValueKind = Number : "16").

                        bool matchesAllBindings =
                            bindings == null ||
                            bindings.All(binding =>
                                inputItem.TryGetValue(binding.Key, out var actual) &&
                                ValuesMatch(binding.Value, actual));

                        if (matchesAllBindings)
                        {
                            wantedItems.Add(inputItem);
                        }
                    }
                }
                else
                {
                    wantedItems = inputItems;
                }

                List<Dictionary<string, object>> outputItems = new List<Dictionary<string, object>>();
                if (wantedAttributes != null)
                {
                    foreach (var wantedItem in wantedItems)
                    {
                        // create a new dictionary that only contains the wanted attributes and their values
                        Dictionary<string, object> wantedAttributeValues = new Dictionary<string, object>();
                        foreach (var attribute in wantedAttributes)
                        {
                            if (wantedItem.ContainsKey(attribute))
                            {
                                wantedAttributeValues[attribute] = wantedItem[attribute];
                            }
                            else
                            {
                                wantedAttributeValues[attribute] = null;
                            }
                        }

                        outputItems.Add(wantedAttributeValues);
                    }
                }
                else
                {
                    outputItems = wantedItems;
                }

                if (outputItems.Count > maxResults)
                {
                    Console.WriteLine($"Warning: Filtered results contain {outputItems.Count} items, which exceeds the maxResults limit of {maxResults}. Returning only the first {maxResults} items.");
                    outputItems = outputItems.Take(maxResults).ToList();
                }

                return outputItems;
            }
        }

        private static object Unwrap(object value)
        {
            // If the value is a JsonElement, unwrap it to a plain CLR value; otherwise return as-is (already unwrapped)

            if (value is JsonElement el)
            {
                switch (el.ValueKind)
                {
                    case JsonValueKind.String: return el.GetString();
                    case JsonValueKind.Number: return el.TryGetInt64(out long l) ? l : (object)el.GetDouble();
                    case JsonValueKind.True:
                    case JsonValueKind.False: return el.GetBoolean();
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined: return null;
                    default: return el.ToString(); // object/array -> raw JSON
                }
            }

            return value; // if already a plain CLR value 
        }

        private static bool ValuesMatch(object expected, object actual)
        {
            object e = Unwrap(expected);
            object a = Unwrap(actual);

            if (e == null || a == null)
                return e == null && a == null;

            return string.Equals(
                Convert.ToString(e, CultureInfo.InvariantCulture),
                Convert.ToString(a, CultureInfo.InvariantCulture),
                StringComparison.OrdinalIgnoreCase);
        }

        internal static string readEventAttributesForTypeId(int typeId)
        {
            string eventMap = readEventMap();
            var eventMapDict = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(eventMap);

            Dictionary<string, string> eventDict
                = eventMapDict["events"].FirstOrDefault(e => e["TypeId"] == typeId.ToString());

            string jsonString = JsonSerializer.Serialize(eventDict, new JsonSerializerOptions { WriteIndented = true });

            return jsonString;
        }

        internal static string readEventMap()
        {
            // read the schema index Json and return it as a string.

            string filePath = @"C:\Projects\Experiments_2025\FunWithMCP\Ollama\ContentExtraction\5. OptaMA3Map\EventMap.json";

            string jsonString = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            return jsonString;
        }

        internal static string readQualifierAttributesForQualifierId(int qualifierId)
        {
            string qualifierMap = readQualifierMap();
            var qualifierMapDict = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(qualifierMap);

            Dictionary<string, string> qualifierDict
                = qualifierMapDict["qualifiers"].FirstOrDefault(q => q["QualifierId"] == qualifierId.ToString());

            string jsonString = JsonSerializer.Serialize(qualifierDict, new JsonSerializerOptions { WriteIndented = true });

            return jsonString;
        }

        internal static string readQualifierMap()
        {
            // read the schema index Json and return it as a string.

            string filePath = @"C:\Projects\Experiments_2025\FunWithMCP\Ollama\ContentExtraction\5. OptaMA3Map\QualifierMap.json";
            string jsonString = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            return jsonString;
        }

        internal static string readSchemaIndex()
        {
            // read the schema index Json and return it as a string.

            string filePath = @"C:\Projects\Experiments_2025\FunWithMCP\Ollama\ContentExtraction\5. OptaMA3Map\SchemaIndex.json";

            string jsonString = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            return jsonString;
        }

        internal static string readSchemaDocumentForNesting(string nesting)
        {
            // transform the nesting into a file name,
            // e.g. root-matchInfo-contestant-item -> <schema document directory>/root-matchInfo-contestant-item.json,
            // read the Json and return it as a string.

            string filePath =
                Path.Combine(@"C:\Projects\Experiments_2025\FunWithMCP\Ollama\ContentExtraction\4. OptaMa3SchemaDocs", nesting) + ".json";

            string jsonString = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            return jsonString;
        }

        internal static string readSportFeedForNesting(string nesting)
        {
            string feedFilePath = @"C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\ManU v Brighton Media\MA3 Manchester United vs Brighton & Hove Albion.json";

            // why is this resource inaccessible due to it' protection level? It's public and the file path is correct. For now, hardcode the file path here instead of using the Resource reference.
            //string feedFilePath1 = Resource.Filepath_MA3_ManU_v_Brighton;

            string jsonString = readSportFeedItemsJsonText(feedFilePath, nesting);

            return jsonString;
        }

        internal static string readSportFeedForNestingAndId(string nesting, object id)
        {
            string feedFilePath = @"C:\Projects\Experiments_2025\FunWithMCP\Ollama\OllamaMcpWebHttpDemo\OllamaMcpWebHttpDemo\MCPServer\Data\Opta\MA3Match Events\2_OptaEventsMA3_ManUnited-NottForest.json";

            string jsonString = readSportFeedItemJsonText(feedFilePath, nesting, id);

            return jsonString;
        }

        // private helper methods returning Json strings for specific items based on nesting and id

        private static string readSportFeedItemsJsonText(string filePath, string nesting)
        {
            object resultJson = readSportFeedItemsForNesting(filePath, nesting);
            string jsonString = JsonSerializer.Serialize(resultJson, new JsonSerializerOptions { WriteIndented = true });

            return jsonString;
        }

        private static string readSportFeedItemJsonText(string filePath, string nesting, object id)
        {
            object resultJson = readSportFeedItemForNestingAndId(filePath, nesting, id);
            string jsonString = JsonSerializer.Serialize(resultJson, new JsonSerializerOptions { WriteIndented = true });

            return jsonString;
        }

        // private helper methods returning Json objects for specific items based on nesting and id

        private static object readSportFeedItemsForNesting(string filePath, string nesting)
        {
            if (!nesting.StartsWith("root-"))
            {
                return $"Error. Nesting parameter must be of form 'root-<x>-<y>-...'.  Found {nesting}";
            }

            try
            {
                List<string> levels = nesting.Replace("root-", "").Split('-').ToList();

                // read the feedJson text and deserialiseinto into a hierarchical Json object
                //string feedJson = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

                Ma3FeedDataProviderService feedData = new Ma3FeedDataProviderService(null);
                var feedJson = feedData.ReadMa3Json().Result;

                Ma3JsonClasses.Rootobject feedObject = JsonSerializer.Deserialize<Ma3JsonClasses.Rootobject>(feedJson);

                switch (levels[0])
                {
                    case "matchInfo_items":
                        Matchinfo matchInfo = feedObject.matchInfo;
                        return matchInfo;

                    case "matchInfo":
                        switch (levels[1])
                        {
                            case "competition_items":
                                Competition competition = feedObject.matchInfo.competition;
                                return competition;

                            case "competition":
                                switch (levels[2])
                                {
                                    case "country_items":
                                        Country country = feedObject.matchInfo.competition.country;
                                        return country;

                                    default: return null;
                                }

                            case "contestant_items":
                                List<Contestant> contestants = feedObject.matchInfo.contestant.ToList();
                                return contestants;

                            case "contestant":
                                switch (levels[2])
                                {
                                    case "country_items":
                                        List<Country1> countries = feedObject.matchInfo.contestant.Select(c => c.country).ToList();
                                        return countries;

                                    default: return null;
                                }

                            case "ruleset_items":
                                Ruleset ruleset = feedObject.matchInfo.ruleset;
                                return ruleset;

                            case "series_items":
                                Series series = feedObject.matchInfo.series;
                                return series;

                            case "sport_items":
                                Sport sports = feedObject.matchInfo.sport;
                                return sports;

                            case "stage_items":
                                Stage stage = feedObject.matchInfo.stage;
                                return stage;

                            case "tournamentCalendar_items":
                                Tournamentcalendar calendar = feedObject.matchInfo.tournamentCalendar;
                                return calendar;

                            case "venue_items":
                                Venue venue = feedObject.matchInfo.venue;
                                return venue;

                            default: return null;
                        }

                    case "liveData":
                        Livedata liveData = feedObject.liveData;

                        if (levels.Count() == 1)
                        {
                            return liveData;
                        }

                        switch (levels[1])
                        {
                            case "event":
                                {
                                    if (levels[2] == "qualifier_items")
                                    {
                                        List<Qualifier> qualifiers = liveData.@event.SelectMany(e => e.qualifier).ToList();

                                        return qualifiers;
                                    }

                                    return null;
                                }

                            case "event_items":
                                // array of event items
                                List<Event> events = liveData.@event.ToList();

                                return events;

                            case "matchDetails":
                                if (levels.Count == 2)
                                {
                                    Matchdetails matchDetails = liveData.matchDetails;

                                    return matchDetails;
                                }

                                // array of score items for half, full time, and total
                                switch (levels[2])
                                {
                                    case "period_items":
                                        {
                                            List<Period> periods = liveData.matchDetails.period.ToList();

                                            return periods;
                                        }

                                    case "scores":
                                        if (levels.Count == 3)
                                        {
                                            Scores scores = liveData.matchDetails.scores;

                                            return scores;
                                        }

                                        // array of score items for half, full time, extra time penalty, and total
                                        switch (levels[3])
                                        {
                                            // array of score items for half, full time, and total
                                            case "ht":
                                                {
                                                    Ht halfTimeScores = liveData.matchDetails.scores.ht;

                                                    return halfTimeScores;
                                                }

                                            case "ft":
                                                {
                                                    Ft fullTimeScores = liveData.matchDetails.scores.ft;

                                                    return fullTimeScores;
                                                }

                                            case "et":
                                                {
                                                    Et extraTimeScores = liveData.matchDetails.scores.et;

                                                    return extraTimeScores;
                                                }

                                            case "total":
                                                {
                                                    Total totalScores = liveData.matchDetails.scores.total;

                                                    return totalScores;
                                                }

                                            case "pen":
                                                {
                                                    Penalty penaltyScores = liveData.matchDetails.scores.penalty;

                                                    return penaltyScores;
                                                }

                                            default: return null;
                                        }

                                    default: return null;
                                }

                            default: return null;
                        }

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"readSportFeedItemsForNesting: {ex}, nesting={nesting}");
                return "Error";
            }
        }

        private static object readSportFeedItemForNestingAndId(string filePath, string nesting, object id)
        {
            List<string> levels = nesting.Replace("root-", "").Split('-').ToList();

            object items = readSportFeedItemsForNesting(filePath, nesting);

            switch (levels[0])
            {
                case "matchInfo":
                    switch (levels[1])
                    {
                        case "contestant_items":
                            // array of contestant items
                            List<Contestant> contestants = (List<Contestant>)items;
                            var idStr = Unwrap(id)?.ToString();
                            Contestant contestant = contestants.First(c => c.id == idStr);

                            return contestant;
                    }
                    return null;

                case "liveData":
                    switch (levels[1])
                    {
                        case "event_items":
                            // array of event items
                            List<Event> events = (List<Event>)items;
                            var unwrappedId = Unwrap(id);
                            long parsedId;
                            if (unwrappedId is long l) parsedId = l;
                            else if (unwrappedId is int i) parsedId = i;
                            else if (unwrappedId is double d) parsedId = Convert.ToInt64(d);
                            else parsedId = long.Parse(Convert.ToString(unwrappedId, System.Globalization.CultureInfo.InvariantCulture));

                            Event ev = events.First(e => e.id == parsedId);

                            return ev;
                    }
                    return null;

                default:
                    return null;
            }
        }
    }
}

