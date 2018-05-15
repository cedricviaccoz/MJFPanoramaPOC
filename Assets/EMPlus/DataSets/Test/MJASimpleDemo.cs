using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MJASimpleDemo : MonoBehaviour {
	[ReadOnlyWhenPlaying] public int maxLines = 10;
    [ReadOnlyWhenPlaying] public string defaultImagePath;
    public int fontResolution = 16;
	public float textScale = 0.5f;
	public float lineSpacing = 1.0f;
    public float mainImageSize = 2.0f;
	public MontreuxJazzArchive archive;

	private MJAListView listView = null;
	private MJADetailView detailView = null;
    private bool inList = true;

    public Vector3 leftPosition = new Vector3(-20, 8, 11);
    public Vector3 rightPosition = new Vector3(-2, 8, 11);
    public Vector3 spawnPosition = new Vector3(50, 8, 24);


    void Start () {
        listView = new MJAListView ("Concert Group ListView");
		detailView = new MJADetailViewConcertGroup ("Concert Group DetailView");

        listView.transform.position = leftPosition;
        detailView.transform.position = spawnPosition;

        OnValidate();

        if (Application.sandboxType != ApplicationSandboxType.NotSandboxed)
            archive.jsonPath = Application.streamingAssetsPath;
        MJAData.archive = archive;
        MJAData.archive.Load();

        if (defaultImagePath.Length > 0 && File.Exists(defaultImagePath))
            MJAData.defaultImageData = File.ReadAllBytes(defaultImagePath);
        else
            Debug.Log("no default image or file not found");

        listView.SetData(MJAData.Field.ConcertGroup, MJAData.archive.concertGroups.GetAllIds(), "All Concert Groups");
        listView.Move(0); // set first entry active
        listView.Update();

        detailView.SetId(listView.CurrentId);
    }

    void OnValidate () {
		MJAData.fontResolution = fontResolution;
		MJAData.textScale = textScale;
        MJAData.lineSpacing = lineSpacing;
        MJAData.mainImageSize = mainImageSize;
        MJAData.maxLines = maxLines;

        if (listView != null) {
			listView.Reshape ();
		}
		if (detailView != null) {
			detailView.Reshape ();
		}
	}
	
	void Update () {
		bool moved = false;
        if (inList)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
            {
                moved = listView.Move(1);
                listView.Update();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
            {
                moved = listView.Move(-1);
                listView.Update();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                inList = false;
                detailView.Move(0);
                listView.Cleanup();
                listView = detailView.CreateListView();
                listView.transform.position = spawnPosition;
                listView.Update();
            }
            if (moved)
            {
                detailView.SetId (listView.CurrentId);
            }
        } else
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
            {
                moved = detailView.Move(1);
                detailView.Update();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
            {
                moved = detailView.Move(-1);
                detailView.Update();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
				inList = true;
				listView.Move (0);
				detailView.Cleanup ();
				detailView = listView.CreateDetailView ();
				detailView.transform.position = spawnPosition;
				detailView.Reshape ();
            }
            if (moved)
            {
                listView.Cleanup();
                listView = detailView.CreateListView();
                listView.transform.position = spawnPosition;
                listView.Update();
            }
        }

        // move the two panels into place
        if (inList)
        {
            listView.transform.position = Vector3.Lerp(listView.transform.position, leftPosition, 0.2f);
            detailView.transform.position = Vector3.Lerp(detailView.transform.position, rightPosition, 0.2f);
        } else
        {
            listView.transform.position = Vector3.Lerp(listView.transform.position, rightPosition, 0.2f);
            detailView.transform.position = Vector3.Lerp(detailView.transform.position, leftPosition, 0.2f);
        }
    }
}
