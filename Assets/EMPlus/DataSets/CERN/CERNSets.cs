using System;
using System.Collections;
using System.Collections.Generic;

namespace CERNDataset
{

    [Serializable]
    public class Image : DatasetReferenced
    {
        public string comment, description, eformat, full_name, localFile;
        // references
        public int record;
    }

    [Serializable]
    public class Record : DatasetReferenced
    {
        public string title, copyrightDate, copyrightStatement;
        // references
        public int copyrightHolder;
        public int[] authors;
        public int[] collections;
        public int[] files;
        public int[] keywords;
        public int[] subjects;
    }

    [Serializable]
    public class Author : DatasetReferenced
    {
        public string first_name, last_name, full_name;
        // references
        public int[] records;
    }

    [Serializable]
    public class Keyword : DatasetReferenced
    {
        public string term;
        // references
        public int[] records;
    }

    [Serializable]
    public class Subject : DatasetReferenced
    {
        public string term;
        // references
        public int[] records;
    }

    [Serializable]
    public class CopyrightHolder : DatasetReferenced
    {
        public string name;
        // references
        public int[] records;
    }

    [Serializable]
    public class Collection : DatasetReferenced
    {
        public string primary, secondary;
        // references
        public int[] records;
    }
}