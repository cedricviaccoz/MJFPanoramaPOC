using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using MJADataset;

[CreateAssetMenu(menuName = "EMPlus Dataset/MontreuxJazzArchive")]
public class MontreuxJazzArchive : ScriptableObject
{
    [HideInInspector] [NonSerialized] public bool dataLoaded = false;

    [HideInInspector] [NonSerialized] public BandList bands;
    [HideInInspector] [NonSerialized] public PersonList persons;
    [HideInInspector] [NonSerialized] public ConcertList concerts;
    [HideInInspector] [NonSerialized] public ConcertGroupList concertGroups;
    [HideInInspector] [NonSerialized] public ImageList images;
    [HideInInspector] [NonSerialized] public ImageTagList imageTags;
    [HideInInspector] [NonSerialized] public LocationList locations;

    public string jsonPath = "/Users/voelzow/JazzArchiveJSON";
    public bool loadBands = true;
    public bool loadPersons = true;
    public bool loadConcerts = true;
    public bool loadConcertGroups = true;
    public bool loadImages = true;
    public bool loadImageTags = true;
    public bool loadLocations = true;

    private T loadJson<T>(string filename)
    {
        if (!File.Exists(filename))
        {
            Debug.LogError("file " + filename + " does not exist!");
            return default(T);
        }
        T o = JsonUtility.FromJson<T>(File.ReadAllText(filename));
        return o;
    }

    public void Load()
    {
        if (loadBands)
        {
            bands = loadJson<BandList>(jsonPath + "/bands.json");
            bands.PrepareData();
            Debug.Log("MJA Bands loaded: " + bands.items.Length);
        }
        if (loadPersons)
        {
            persons = loadJson<PersonList>(jsonPath + "/persons.json");
            persons.PrepareData();
            Debug.Log("MJA Persons loaded: " + persons.items.Length);
        }
        if (loadConcerts)
        {
            concerts = loadJson<ConcertList>(jsonPath + "/concerts.json");
            concerts.PrepareData();
            int ce = 0;
            int cs = 0;
            for (int n = concerts.items.Length - 1; n >= 0; n--)
            {
                ce += concerts.items[n].eventIds.Length;
                for (int m = concerts.items[n].events.Length - 1; m >= 0; m--)
                    if (concerts.items[n].events[m].type == "song")
                        cs++;
            }
            Debug.Log("MJA Concerts loaded: " + concerts.items.Length + " (with " + ce + " events, " + cs + " songs)");
        }
        if (loadConcertGroups)
        {
            concertGroups = loadJson<ConcertGroupList>(jsonPath + "/concertgroups.json");
            concertGroups.PrepareData();
            Debug.Log("MJA ConcertGroups loaded: " + concertGroups.items.Length);
        }
        if (loadImages)
        {
            images = loadJson<ImageList>(jsonPath + "/images.json");
            images.PrepareData();
            Debug.Log("MJA Images loaded: " + images.items.Length);
        }
        if (loadImageTags)
        {
            imageTags = loadJson<ImageTagList>(jsonPath + "/imagetags.json");
            imageTags.PrepareData();
            Debug.Log("MJA ImageTags loaded: " + imageTags.items.Length);
        }
        if (loadLocations)
        {
            locations = loadJson<LocationList>(jsonPath + "/locations.json");
            locations.PrepareData();
            Debug.Log("MJA locations loaded: " + locations.items.Length);
        }

        // generate missing relations
        if (loadLocations && loadImages)
        {
            for (int n = 0; n < images.images.Length; n++)
            {
                for (int m = 0; m < images.images[n].locations.Length; m++)
                {
                    if (images.images[n].locations != null && locations[images.images[n].locations[m]] != null)
                    {
                        if (locations[images.images[n].locations[m]].images == null)
                        {
                            locations[images.images[n].locations[m]].images = new int[1];
                            locations[images.images[n].locations[m]].images[0] = images.images[n].id;
                        }
                        else
                        {
                            List<int> list = locations[images.images[n].locations[m]].images.ToList();
                            list.Add(images.images[n].id);
                            locations[images.images[n].locations[m]].images = list.ToArray();
                        }
                    }
                }
            }
        }
        if (loadLocations && loadConcerts)
        {
            for (int n = 0; n < concerts.concerts.Length; n++)
            {
                if (concerts.concerts[n].location > 0 && locations[concerts.concerts[n].location] != null)
                {
                    if (locations[concerts.concerts[n].location].concerts == null)
                    {
                        locations[concerts.concerts[n].location].concerts = new int[1];
                        locations[concerts.concerts[n].location].concerts[0] = concerts.concerts[n].id;
                    }
                    else
                    {
                        List<int> list = locations[concerts.concerts[n].location].concerts.ToList();
                        list.Add(concerts.concerts[n].id);
                        locations[concerts.concerts[n].location].concerts = list.ToArray();
                    }
                }
            }
        }

        dataLoaded = true;
    }

    // Singleton - rarely needed though
    private static MontreuxJazzArchive _inst = null;
    private bool _loaded = false;
    public static MontreuxJazzArchive Instance
    {
        get
        {
            if (!_inst)
                _inst = Resources.FindObjectsOfTypeAll<MontreuxJazzArchive>().FirstOrDefault();
            if (!_inst.Loaded)
                _inst.Load();
            return _inst;
        }
    }
    public bool Loaded { get { return _loaded; } }
}
