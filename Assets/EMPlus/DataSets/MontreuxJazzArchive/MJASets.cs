using System;
using System.Collections;
using System.Collections.Generic;

namespace MJADataset
{

    [Serializable]
    public class Band : DatasetReferenced
    {
        public struct _Lineup
        {
            public int[] members;
            public int[] concerts;
        }
        public string name;
        public _Lineup[] lineups;
    }

    [Serializable]
    public class Person : DatasetReferenced
    {
        public string publicName, fullName, firstName, lastName, stageName, nickname;

        // references
        public int[] concerts;
        public int[] bands;
        public int[] images;
        public int[] mainImages;
        public int mainImage;
    }

    [Serializable]
    public class ConcertGroup : DatasetReferenced
    {
        [Serializable]
        public struct _ConcertGroupDescriptions
        {
            [Serializable]
            public struct _ConcertGroupDescription
            {
                public string name;
                public string description;
                public string credit;
            }
            public _ConcertGroupDescription en;
            public _ConcertGroupDescription fr;
        }
        public string startDate;
        public string endDate;
        public _ConcertGroupDescriptions descriptions;

        // references
        public int[] concerts;
        public int mainImage;
        public int[] mainImages;

        public string Date { get { return startDate + " - " + endDate; } }
        public string nameEn { get { return descriptions.en.name; } }
        public string nameFr { get { return descriptions.fr.name; } }
        public string descriptionEn { get { return descriptions.en.description; } }
        public string descriptionFr { get { return descriptions.fr.description; } }
    }

    [Serializable]
    public class Concert : DatasetReferenced
    {
        [Serializable]
        public class _ConcertEvent
        {
            [Serializable]
            public struct _ConcertEventSource
            {
                public int index;
                public string type;
                public string url;
            }
            public int id;
            public int order;
            public string type;
            public string title;
            public _ConcertEventSource[] sources;
        }

        [Serializable]
        public struct _Musician
        {
            public int id;
            public string[] instruments;
            public int band;
        }
        [Serializable]
        public struct _ConcertGroup
        {
            public int id;
            // public string startDate;
            // public string endDate;
        }

        public string key;
        public string date;
        public int order;
        public string name;

        public _ConcertGroup[] concertGroups;
        public _Musician[] musicians;
        public _ConcertEvent[] events;

        // references
        public int location;
        public int[] genres;
        public int[] mainGenres;
        public int[] images;
        public int[] mainImages;
        public int mainImage;

        // generated references
        public int[] eventIds;
        public int[] concertGroupIds;

        public void PrepareData()
        {
            concertGroupIds = new int[concertGroups.Length];
            for (int n = 0; n < concertGroups.Length; n++)
                concertGroupIds[n] = concertGroups[n].id;

            eventIds = new int[events.Length];
            for (int n = 0; n < events.Length; n++)
                eventIds[n] = events[n].id;
        }
    }

    [Serializable]
    public class ImageTag : DatasetReferenced
    {
        public string name;
        public int[] images;
    }

    [Serializable]
    public class Location : DatasetReferenced
    {
        public string name;

        // generated references
        public int[] images;
        public int[] concerts;
    }

    [Serializable]
    public class Image : DatasetReferenced
    {
        [Serializable]
        public struct _Date
        {
            public int year;
            public int month;
            public int day;
        }
        [Serializable]
        public struct _Photographer
        {
            public string fullName;
            public string firstName;
            public string lastName;
        }
        [Serializable]
        public struct _MediaSource
        {
            public int index;
            public string url;
            public string size;
            public int maxSize;
        }
        public string credit;
        public string orientation;
        public int[] tags;
        public int[] artists;
        public int[] concerts;
        public int[] locations;

        public _Date date;
        public _Photographer[] photographers;
        public _MediaSource[] mediaSources;

        public string Date { get { return String.Format("{0:0000}-{1:00}-{2:00}", date.year, date.month, date.day); } }
    };

}