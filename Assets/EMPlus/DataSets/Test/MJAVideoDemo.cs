using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/***
 * This demo will take all mp4 files in a directory and display a list of the concerts that 
 * the videos belong to with their corresponding lists of ConcertEvents that refer to those videos
 ***/

public class MJAVideoDemo : MonoBehaviour
{
    public MontreuxJazzArchive archive;

    // path to the directory that contains the videos (subdirectories are ignored...)
    public string videoPath;

    // the two UI elements that hold the lists to navigate the concerts/events
    public Text concertText;
    public Text eventText;

    public MeshRenderer videoTarget;

    // currently selected concert
    MJADataset.Concert concert = null;

    // these hold the title and ids of the events that belong to the active concert
    List<string> concertEventTitles;
    List<int> concertEventIds;
    List<int> concertFileIds;

    // these hold all of the event and concert ids that are involved with the videos
    List<int> fileIdList;
    List<int> concertIdList;

    // whether interaction happens in the concert list or the event list
    bool inConcertlist = true;

    // current indices of both lists
    int concertIndex = 0;
    int eventIndex = 0;

    // the actual video player
    VideoPlayer videoPlayer;

    void Start()
    {
        archive.Load();
        fileIdList = new List<int>();
        concertIdList = new List<int>();

        // generate list of all available videos in given directory (not subdirectories)
        string[] videoFiles = System.IO.Directory.GetFiles(videoPath);
        char[] separator = { '/', '\\' };
        foreach (string file in videoFiles)
        {
            if (!file.EndsWith(".mp4"))
                continue;
            string fileNo = file.Substring(0, file.Length - 4); // get rid of the extension
            if (fileNo.Contains("/") || fileNo.Contains("\\"))
                fileNo = fileNo.Substring(fileNo.LastIndexOfAny(separator) + 1); // get rid of the full path
            fileIdList.Add(int.Parse(fileNo));
        }
        Debug.Log("video files in folder: " + fileIdList.ToArray().Length);

        // find concerts with evens matching the videos
        foreach (MJADataset.Concert concert in archive.concerts)
        {
            foreach (MJADataset.Concert._ConcertEvent concertEvent in concert.events)
            {
                if (concertEvent.sources == null || concertEvent.sources.Length == 0) continue;
                string[] urlSplit = concertEvent.sources[0].url.Split('/');
                var lastSegment = urlSplit[urlSplit.Length - 1];
                int fileId = int.Parse(lastSegment.Substring(0, lastSegment.Length - 4));
                if (fileIdList.Contains(fileId) && !concertIdList.Contains(concert.id))
                    concertIdList.Add(concert.id);
            }
        }
        Debug.Log("concerts from which the videos are: " + concertIdList.ToArray().Length);

        if (concertIdList.ToArray().Length == 0) return;

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCameraAlpha = 1F;

        UpdateConcertList();
    }

    void LoadConcert(int concertId)
    {
        eventIndex = 0;
        concert = archive.concerts[concertId];
        concertEventTitles = new List<string>();
        concertEventIds = new List<int>();
        concertFileIds = new List<int>();
        Debug.Log(concert.name);
        foreach (MJADataset.Concert._ConcertEvent concertEvent in concert.events)
        {
            if (concertEvent.sources == null || concertEvent.sources.Length == 0) continue;
            string[] urlSplit = concertEvent.sources[0].url.Split('/');
            var lastSegment = urlSplit[urlSplit.Length - 1];
            int fileId = int.Parse(lastSegment.Substring(0, lastSegment.Length - 4));
            if (fileIdList.Contains(fileId))
            {
                concertFileIds.Add(fileId);
                concertEventIds.Add(concertEvent.id);
                // Debug.Log("contains event:" + concertEvent.id + " file: " + fileId);
                if (concertEvent.title != null && concertEvent.title.Length > 0)
                    concertEventTitles.Add(concertEvent.title);
                else
                    concertEventTitles.Add("[" + concertEvent.type + " id=" + concertEvent.id + "]");
            }
        }
        UpdateEventList();
    }

    // gets called every time the user changes the current concert to highlight the corresponding entry
    void UpdateConcertList()
    {
        LoadConcert(concertIdList[concertIndex]);
        int n = 0;
        if (inConcertlist)
            concertText.text = "<b><color=black>Concerts:</color></b>";
        else
            concertText.text = "<b><color=#333333>Concerts:</color></b>";
        foreach (int concertid in concertIdList)
        {
            if (n == concertIndex)
            {
                if (inConcertlist)
                    concertText.text += "\n<color=#ffaa00>" + archive.concerts[concertid].name + "</color>";
                else
                    concertText.text += "\n<color=#7f5500>" + archive.concerts[concertid].name + "</color>";
            }
            else
            {
                if (inConcertlist)
                    concertText.text += "\n<color=#000000>" + archive.concerts[concertid].name + "</color>";
                else
                    concertText.text += "\n<color=#333333>" + archive.concerts[concertid].name + "</color>";
            }
            n++;
        }
    }

    // updates the display of events - gets called from LoadConcert when new concert is selected or whenever the highlight of the event list needs an update
    void UpdateEventList()
    {
        int n = 0;
        if (inConcertlist)
            eventText.text = "<b><color=#333333>Concert Events:</color></b>";
        else
            eventText.text = "<b><color=black>Concert Events:</color></b>";
        foreach (string title in concertEventTitles)
        {
            if (n == eventIndex)
            {
                string videoUrl = videoPath + "/" + concertFileIds[n] + ".mp4";
                if (videoUrl != videoPlayer.url)
                {
                    videoPlayer.url = videoPath + "/" + concertFileIds[n] + ".mp4";
                    videoPlayer.Play();
                }

                if (inConcertlist)
                    eventText.text += "\n<color=#7f5500>" + title + "</color>";
                else
                    eventText.text += "\n<color=#ffaa00>" + title + "</color>";
            }
            else
            {
                if (inConcertlist)
                    eventText.text += "\n<color=#333333>" + title + "</color>";
                else
                    eventText.text += "\n<color=#000000>" + title + "</color>";
            }
            n++;
        }
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (inConcertlist)
            {
                if (concertIndex < concertIdList.Count - 1)
                {
                    concertIndex++;
                    UpdateConcertList();
                }
            }
            else
            {
                if (eventIndex < concertEventIds.Count - 1)
                {
                    eventIndex++;
                    UpdateEventList();
                }
            }
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (inConcertlist)
            {
                if (concertIndex > 0)
                {
                    concertIndex--;
                    UpdateConcertList();
                }
            }
            else
            {
                if (eventIndex > 0)
                {
                    eventIndex--;
                    UpdateEventList();
                }
            }
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) && inConcertlist || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) && !inConcertlist)
        {
            inConcertlist = !inConcertlist;
            UpdateConcertList();
        }
    }
}