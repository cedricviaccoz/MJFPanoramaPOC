using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_WSA && !UNITY_EDITOR
using Newtonsoft;
#endif

using CERNDataset;

[CreateAssetMenu(menuName = "EMPlus Dataset/CERN")]
public class CERN : ScriptableObject
{
    [HideInInspector] [NonSerialized] public bool dataLoaded = false;

    [HideInInspector] [NonSerialized] public ImageList images;
    [HideInInspector] [NonSerialized] public RecordList records;
    [HideInInspector] [NonSerialized] public AuthorList authors;
    [HideInInspector] [NonSerialized] public KeywordList keywords;
    [HideInInspector] [NonSerialized] public SubjectList subjects;
    [HideInInspector] [NonSerialized] public CopyrightHolderList copyrightHolders;
    [HideInInspector] [NonSerialized] public CollectionList collections;

    public string jsonPath = "/Users/voelzow/CERN";
    public bool loadImages = true;
    public bool loadRecords = true;
    public bool loadAuthors = true;
    public bool loadKeywords = true;
    public bool loadSubjects = true;
    public bool loadCopyrightHolders = true;
    public bool loadCollections = true;

    private T LoadJson<T>(string filename)
    {
        if (!File.Exists(filename))
        {
            Debug.LogError("file " + filename + " does not exist! (path: " + jsonPath + ")");
            return default(T);
        }

#if (!UNITY_WSA) || UNITY_EDITOR
        T o = JsonUtility.FromJson<T>(File.ReadAllText(filename));
#else
        T o = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(UnityEngine.Windows.File.ReadAllBytes(filename)));
#endif
        return o;
    }

    public void Load()
    {
        Debug.Log("CERN archive loading from path: " + jsonPath);
        if (loadImages)
        {
            images = LoadJson<ImageList>(jsonPath + "/files.json");
            images.PrepareData();
            Debug.Log("CERN Images loaded: " + images.items.Length);
        }
        if (loadRecords)
        {
            records = LoadJson<RecordList>(jsonPath + "/records.json");
            records.PrepareData();
            Debug.Log("CERN Records loaded: " + records.items.Length);
        }
        if (loadAuthors)
        {
            authors = LoadJson<AuthorList>(jsonPath + "/authors.json");
            authors.PrepareData();
            Debug.Log("CERN Authors loaded: " + authors.items.Length);
        }
        if (loadKeywords)
        {
            keywords = LoadJson<KeywordList>(jsonPath + "/keywords.json");
            keywords.PrepareData();
            Debug.Log("CERN Keywords loaded: " + keywords.items.Length);
        }
        if (loadSubjects)
        {
            subjects = LoadJson<SubjectList>(jsonPath + "/subjects.json");
            subjects.PrepareData();
            Debug.Log("CERN Subjects loaded: " + subjects.items.Length);
        }
        if (loadCopyrightHolders)
        {
            copyrightHolders = LoadJson<CopyrightHolderList>(jsonPath + "/copyright.json");
            copyrightHolders.PrepareData();
            Debug.Log("CERN CopyrightHolders loaded: " + copyrightHolders.items.Length);
        }
        if (loadCollections)
        {
            collections = LoadJson<CollectionList>(jsonPath + "/collections.json");
            collections.PrepareData();
            Debug.Log("CERN Collections loaded: " + collections.items.Length);
        }

        dataLoaded = true;
    }

    // Singleton - rarely needed though
    private static CERN _inst = null;
    private bool _loaded = false;
    public static CERN Instance
    {
        get
        {
            if (!_inst)
                _inst = Resources.FindObjectsOfTypeAll<CERN>().FirstOrDefault();
            if (!_inst.Loaded)
                _inst.Load();
            return _inst;
        }
    }
    public bool Loaded { get { return _loaded; } }
}
