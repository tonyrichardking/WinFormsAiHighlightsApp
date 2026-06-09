using System;
using System.Collections.Generic;
using System.Text;

namespace ContentExtraction
{
    public class Ma3JsonClasses
    {
        public class Rootobject
        {
            public Matchinfo matchInfo { get; set; }
            public Livedata liveData { get; set; }
        }

        public class Matchinfo
        {
            public string id { get; set; }
            public string coverageLevel { get; set; }
            public string date { get; set; }
            public string time { get; set; }
            public string localDate { get; set; }
            public string localTime { get; set; }
            public string week { get; set; }
            public int numberOfPeriods { get; set; }
            public int periodLength { get; set; }
            public DateTime lastUpdated { get; set; }
            public string description { get; set; }
            public Sport sport { get; set; }
            public Ruleset ruleset { get; set; }
            public Competition competition { get; set; }
            public Tournamentcalendar tournamentCalendar { get; set; }
            public Stage stage { get; set; }
            public Contestant[] contestant { get; set; }
            public Venue venue { get; set; }
            public Series series { get; set; }           
        }

        public class Sport
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Ruleset
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Competition
        {
            public string id { get; set; }
            public string name { get; set; }
            public string knownName { get; set; }
            public string competitionCode { get; set; }
            public string competitionFormat { get; set; }
            public Country country { get; set; }
        }

        public class Country
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Tournamentcalendar
        {
            public string id { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
            public string name { get; set; }
        }

        public class Stage
        {
            public string id { get; set; }
            public string formatId { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
            public string name { get; set; }
        }

        public class Venue
        {
            public string id { get; set; }
            public string neutral { get; set; }
            public string longName { get; set; }
            public string shortName { get; set; }
        }

        public class Contestant
        {
            public string id { get; set; }
            public string name { get; set; }
            public string shortName { get; set; }
            public string officialName { get; set; }
            public string code { get; set; }
            public string position { get; set; }
            public Country1 country { get; set; }
        }

        public class Country1
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Livedata
        {
            public Matchdetails matchDetails { get; set; }
            public Event[] @event { get; set; }
        }

        public class Matchdetails
        {
            public int periodId { get; set; }
            public string matchStatus { get; set; }
            public string winner { get; set; }
            public int matchLengthMin { get; set; }
            public int matchLengthSec { get; set; }
            public Period[] period { get; set; }
            public Scores scores { get; set; }
        }

        public class Scores
        {
            public Ht ht { get; set; }
            public Ft ft { get; set; }
            public Et et { get; set; }
            public Total total { get; set; }
            public Penalty penalty { get; set; }
        }

        public class Ht
        {
            public int home { get; set; }
            public int away { get; set; }
        }

        public class Ft
        {
            public int home { get; set; }
            public int away { get; set; }
        }

        public class Et
        {
            public int home { get; set; }
            public int away { get; set; }
        }

        public class Total
        {
            public int home { get; set; }
            public int away { get; set; }
        }

        public class Penalty
        {
            public int home { get; set; }
            public int away { get; set; }
        }

        public class Period
        {
            public int id { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public int lengthMin { get; set; }
            public int lengthSec { get; set; }
            public int announcedInjuryTime { get; set; }
        }

        public class Event
        {
            public long id { get; set; }
            public int eventId { get; set; }
            public int typeId { get; set; }
            public int periodId { get; set; }
            public int timeMin { get; set; }
            public int timeSec { get; set; }
            public string contestantId { get; set; }
            public int outcome { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public DateTime timeStamp { get; set; }
            public DateTime lastModified { get; set; }
            public Qualifier[] qualifier { get; set; }
            public string playerId { get; set; }
            public string playerName { get; set; }
            public int keyPass { get; set; }
            public int assist { get; set; }
        }

        public class Qualifier
        {
            public long id { get; set; }
            public int qualifierId { get; set; }
            public string value { get; set; }
        }
        public class Series
        {
            public long id { get; set; }
            public long ocId { get; set; }
            public long opId { get; set; }
            public string horizontal { get; set; }
            public string formatId { get; set; }
            public string name { get; set; }
        }
    }
}
