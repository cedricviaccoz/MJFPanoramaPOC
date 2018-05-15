using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CERNData {
	public enum Field {
		Image,
		Record,
		Keyword,
		Collection,
		Undefined
	};
	public static CERN archive = null;
	public static int maxLines = 20;
	public static byte[] defaultImageData;
}

public class CernRecordView : Transform {
	int imageCount = 0;
	int keywordCount = 0;
	int collectionCount = 0;
	int currentIndex = -1;
	CERNDataset.Record record;

	public Transform text;

	public CernRecordView(Transform prefab, Transform canvas, int id) {
		text = (Transform)Object.Instantiate (prefab, canvas);
		LoadRecord (id);
	}
	public void LoadRecord(int id)
	{
		text.name = "Record " + id;
		currentIndex = -1;
		record = CERNData.archive.records [id];
		imageCount = Mathf.Min(CERNData.maxLines, record.files.Length);
		keywordCount = Mathf.Min(CERNData.maxLines, record.keywords.Length);
		collectionCount = Mathf.Min(CERNData.maxLines, record.collections.Length);
		if (imageCount + keywordCount + collectionCount> 0)
			currentIndex = 0;
		Update ();
	}

	public bool OnImage() {
		return currentIndex > -1 && currentIndex < imageCount;
	}
	public string ImagePath() {
		return CERNData.archive.images [record.files [currentIndex]].localFile;
	}
	public int KeywordId() {
		if (currentIndex >= imageCount && currentIndex - imageCount < keywordCount)
			return record.keywords [currentIndex - imageCount];
		return -1;
	}
	public int CollectionId() {
		if (currentIndex >= imageCount+keywordCount && currentIndex - imageCount - keywordCount < keywordCount)
			return record.collections [currentIndex - imageCount - keywordCount];
		return -1;
	}

	public void Move(int dir) {
		if (currentIndex < 0 || dir == 0)
			return;
		currentIndex = Mathf.Clamp (currentIndex + dir, 0, imageCount + keywordCount + collectionCount - 1);
		Update ();
	}
	void Update() {
		string label = "<b>Record ID=" + record.id + "</b>\n" + record.title;
		if (imageCount > 0) {
			label += "\n\n<b>Images:</b>";
			for (int n = 0; n < imageCount; n++) {
				label += "\n" + (n == currentIndex ? "<color=orange>" : "<color=gray>") + CERNData.archive.images [record.files [n]].localFile + "</color>";
			}
		}
		if (keywordCount > 0) {
			label += "\n\n<b>Keywords:</b>";
			for (int n = 0; n < keywordCount; n++) {
				label += "\n" + (n == currentIndex - imageCount ? "<color=orange>" : "<color=gray>") + CERNData.archive.keywords [record.keywords [n]].term + "</color>";
			}
		}
		if (collectionCount > 0) {
			label += "\n\n<b>Collections:</b>";
			for (int n = 0; n < collectionCount; n++) {
				label += "\n" + (n == currentIndex - imageCount - keywordCount? "<color=orange>" : "<color=gray>") + CERNData.archive.collections [record.collections [n]].primary + "</color>";
			}
		}
		text.GetComponent<Text> ().text = label;
	}
}

public class CernKeywordView : Transform {
	int recordCount = 0;
	int currentIndex = -1;
	CERNDataset.Keyword keyword;
	public Transform text;

	public CernKeywordView(Transform prefab, Transform canvas, int id) {
		text = (Transform)Object.Instantiate (prefab, canvas);
		LoadKeyword (id);
	}
	public void LoadKeyword(int id)
	{
		text.name = "Keyword " + id;
		currentIndex = -1;
		keyword = CERNData.archive.keywords [id];
		recordCount = Mathf.Min(CERNData.maxLines, keyword.records.Length);
		if (recordCount > 0)
			currentIndex = 0;
		Update ();
	}

	public int RecordId() {
		if (currentIndex > -1 && currentIndex < recordCount)
			return keyword.records [currentIndex];
		return -1;
	}

	public void Move(int dir) {
		if (currentIndex < 0 || dir == 0)
			return;
		currentIndex = Mathf.Clamp (currentIndex + dir, 0, recordCount - 1);
		Update ();
	}
	void Update() {
		string label = "<b>Keyword ID=" + keyword.id + "</b>\n" + keyword.term;
		if (recordCount > 0) {
			label += "\n\n<b>Records:</b>";
			for (int n = 0; n < recordCount; n++) {
				label += "\n" + (n == currentIndex ? "<color=orange>" : "<color=gray>") + CERNData.archive.records [keyword.records [n]].title + "</color>";
			}
		}
		text.GetComponent<Text> ().text = label;
	}
}

public class CernCollectionView : Transform {
	int recordCount = 0;
	int currentIndex = -1;
	CERNDataset.Collection collection;
	public Transform text;

	public CernCollectionView(Transform prefab, Transform canvas, int id) {
		text = (Transform)Object.Instantiate (prefab, canvas);
		LoadCollection (id);
	}
	public void LoadCollection(int id) {
		text.name = "Collection " + id;
		currentIndex = -1;
		collection = CERNData.archive.collections [id];
		recordCount = Mathf.Min(CERNData.maxLines, collection.records.Length);
		if (recordCount > 0)
			currentIndex = 0;
		Update ();
	}

	public int RecordId() {
		if (currentIndex > -1 && currentIndex < recordCount)
			return collection.records [currentIndex];
		return -1;
	}

	public void Move(int dir) {
		if (currentIndex < 0 || dir == 0)
			return;
		currentIndex = Mathf.Clamp (currentIndex + dir, 0, recordCount - 1);
		Update ();
	}
	void Update() {
		string label = "<b>Keyword ID=" + collection.id + "</b>";
		if (collection.primary.Length > 0) label += "\nPrimary: " + collection.primary;
		if (collection.secondary.Length > 0) label += "\nSecondary: " + collection.secondary;
		if (recordCount > 0) {
			label += "\n\n<b>Records:</b>";
			for (int n = 0; n < recordCount; n++) {
				label += "\n" + (n == currentIndex ? "<color=orange>" : "<color=gray>") + CERNData.archive.records [collection.records [n]].title + "</color>";
			}
		}
		text.GetComponent<Text> ().text = label;
	}
}

public class CERNSimpleDemo : MonoBehaviour {
	[ReadOnlyWhenPlaying] public string defaultImagePath;
	public CERN archive;

    public Vector3 spawnPosition = new Vector3(2500, 2000, 240);

	public GameObject canvas;
	public Transform textPrefab;
	public Transform imageElement;

	private CERNData.Field currentMode = CERNData.Field.Record;

	private CernRecordView recordView;
	private CernKeywordView keywordView;
	private CernCollectionView collectionView;

    void Start () {
		imageElement.GetComponent<Image> ().enabled = false;

        if (Application.sandboxType != ApplicationSandboxType.NotSandboxed)
            archive.jsonPath = Application.streamingAssetsPath;
        CERNData.archive = archive;
        CERNData.archive.Load();

        if (defaultImagePath.Length > 0 && File.Exists(defaultImagePath))
			CERNData.defaultImageData = File.ReadAllBytes(defaultImagePath);

		recordView = new CernRecordView (textPrefab, canvas.transform, 39577);
		keywordView = new CernKeywordView (textPrefab, canvas.transform, 2434);
		collectionView = new CernCollectionView (textPrefab, canvas.transform, 3);

		SwitchToRecord(39577);
    }
		
	void Update () {
		int move = 0;
		bool onImage = false;
		bool enter = false;
		if (UnityEngine.Input.GetKeyDown (KeyCode.UpArrow))
			move = -1;
		else if (UnityEngine.Input.GetKeyDown (KeyCode.DownArrow))
			move = 1;
		else if (UnityEngine.Input.GetKeyDown (KeyCode.Return))
			enter = true;

		if (enter) {
			switch (currentMode) {
			case CERNData.Field.Record:
				if (recordView.KeywordId () > -1)
					SwitchToKeyword (recordView.KeywordId ());
				else if (recordView.CollectionId () > -1)
					SwitchToCollection (recordView.CollectionId ());
				break;
			case CERNData.Field.Keyword:
				if (keywordView.RecordId () > -1)
					SwitchToRecord (keywordView.RecordId ());
				break;
			case CERNData.Field.Collection:
				if (collectionView.RecordId () > -1)
					SwitchToRecord (collectionView.RecordId ());
				break;
			}
		}
		else if (move != 0) {
			switch (currentMode) {
			case CERNData.Field.Record:
				recordView.Move (move);
				onImage = recordView.OnImage ();
				break;
			case CERNData.Field.Collection:
				collectionView.Move (move);
				break;
			case CERNData.Field.Keyword:
				keywordView.Move (move);
				break;
			default:
				break;
			}
			Image img = imageElement.GetComponent<Image> ();
			if (onImage) {
				img.enabled = true;
				var path = CERNData.archive.jsonPath + "/Photos/" + recordView.ImagePath ();
				Debug.Log ("loading new image: " + path);
				img.sprite = IMG2Sprite.instance.LoadNewSprite (path);
				img.transform.localScale = new Vector3((float)img.sprite.texture.width / (float)img.sprite.texture.height, 1f, 1f);
			} else {
				img.enabled = false;
			}
		}
	}

	void SwitchToRecord(int id) {
		recordView.LoadRecord(id);
		currentMode = CERNData.Field.Record;
		bool onImage = recordView.OnImage ();
		Image img = imageElement.GetComponent<Image> ();
		if (onImage) {
			img.enabled = true;
			var path = CERNData.archive.jsonPath + "/Photos/" + recordView.ImagePath ();
			Debug.Log ("loading new image: " + path);
			img.sprite = IMG2Sprite.instance.LoadNewSprite (path);
			img.transform.localScale = new Vector3((float)img.sprite.texture.width / (float)img.sprite.texture.height, 1f, 1f);
		} else {
			img.enabled = false;
		}
		recordView.text.transform.localPosition		= Vector3.zero;
		keywordView.text.transform.localPosition	= spawnPosition;
		collectionView.text.transform.localPosition	= spawnPosition;
	}
	void SwitchToKeyword(int id) {
		keywordView.LoadKeyword(id);
		currentMode = CERNData.Field.Keyword;

		recordView.text.transform.localPosition		= spawnPosition;
		keywordView.text.transform.localPosition	= Vector3.zero;
		collectionView.text.transform.localPosition	= spawnPosition;
	}
	void SwitchToCollection(int id) {
		collectionView.LoadCollection(id);
		currentMode = CERNData.Field.Collection;

		recordView.text.transform.localPosition		= spawnPosition;
		keywordView.text.transform.localPosition	= spawnPosition;
		collectionView.text.transform.localPosition	= Vector3.zero;
	}
}
