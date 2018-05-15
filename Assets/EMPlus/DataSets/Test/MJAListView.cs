using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MJAListView {
	public GameObject gameObject;
	private GameObject headerLine;
	private GameObject[] textLines;
	private int currentIndex = -1;
	private int offset = 0;

	private MJAData.Field fieldType;
	private int[] ids;
	private string[] lineTexts;

	public MJAListView(string name) {
		gameObject = new GameObject (name);
		headerLine = new GameObject (name + " Header");
		headerLine.transform.SetParent (transform);
		var headText = headerLine.AddComponent<TextMesh> ();
		headText.text = "header";
		headText.fontStyle = FontStyle.Bold;
		headText.anchor = TextAnchor.MiddleLeft;

		textLines = new GameObject[MJAData.maxLines];
		for (int n = 0; n < MJAData.maxLines; n++) {
			textLines [n] = new GameObject (name + " Line " + n);
			textLines [n].transform.SetParent (transform);
			var text = textLines [n].AddComponent<TextMesh> ();
			text.text = "line number " + n;
			text.anchor = TextAnchor.MiddleLeft;
		}
		Reshape ();
		Update ();
	}

    public void Cleanup()
    {
        MonoBehaviour.Destroy(gameObject);
    }

    public void Reshape() {
		var headText = headerLine.GetComponent<TextMesh> ();
		headText.fontSize = (MJAData.fontResolution * 3) / 2;
		headText.characterSize = 10f * MJAData.textScale / (float)MJAData.fontResolution;
		headerLine.transform.localPosition = new Vector3 (0f, 0f, 0f);

		for (int n = 0; n < MJAData.maxLines; n++) {
			var text = textLines [n].GetComponent<TextMesh> ();
			text.fontSize = MJAData.fontResolution;
			text.characterSize = 10f * MJAData.textScale / (float)MJAData.fontResolution;
			textLines [n].transform.localPosition = new Vector3 (0f, -MJAData.lineSpacing * MJAData.textScale * (n+1.5f), 0f);
		}
	}

	public void Update() {
		if (ids == null || lineTexts == null)
			return;
		for (int n = 0; n < MJAData.maxLines; n++) {
			var text = textLines [n].GetComponent<TextMesh> ();
			if ((n == 0 && offset > 0) || (n == MJAData.maxLines -1 && n + offset < ids.Length - 1))
				text.color = new Color (1, 1, 1, 0.3f);
			else if (n + offset == currentIndex)
				text.color = new Color (1, 0.8f, 0.2f, 1);
			else
				text.color = new Color (1, 1, 1, 1);

			if ((n + offset) >= ids.Length)
				text.text = "";
			else
				text.text = lineTexts[n + offset];//"Index: " + (n + offset) + " ID: " + ids[n + offset];
		}
	}

	public bool Move(int direction) {
		if (ids == null || ids.Length < 2) {
			offset = 0;
			currentIndex = 0;
			Update ();
			return false;
		}
		int oldIndex = currentIndex;
		// Debug.Log ("Pre Move: curr: " + currentIndex + " offset: " + offset + " len: " + ids.Length);
		if (direction == 0) {
			currentIndex = 0;
			offset = 0;
		} else if (direction < 0) {
			currentIndex = Mathf.Max (0, currentIndex - 1);
			if (currentIndex - offset == 0 && offset > 0)
				offset--;
		} else {
			currentIndex = Mathf.Min (ids.Length - 1, currentIndex + 1);
			if (currentIndex - offset == MJAData.maxLines - 1 && currentIndex < ids.Length - 1)
				offset++;
		}
		// Debug.Log ("Post Move: curr: " + currentIndex + " offset: " + offset + " len: " + ids.Length);
		Update ();
		return oldIndex != currentIndex;
	}

	public void SetData(MJAData.Field newFieldType, int[] idList, string header) {
		List<int> existingIds = new List<int>();
		for (int n = 0; n < idList.Length; n++) {
			int id = idList [n];
			switch (newFieldType) {
			case MJAData.Field.ConcertGroup:
				if (MJAData.archive.concertGroups [id] != null)
					existingIds.Add (id);
				break;
			case MJAData.Field.Concert:
				if (MJAData.archive.concerts [id] != null)
					existingIds.Add (id);
				break;
			case MJAData.Field.Image:
				if (MJAData.archive.images [id] != null)
					existingIds.Add (id);
				break;
			case MJAData.Field.ImageTag:
				if (MJAData.archive.imageTags [id] != null)
					existingIds.Add (id);
				break;

			default:
				break;
			}
		}
		ids = existingIds.ToArray();

		fieldType = newFieldType;
		lineTexts = new string[ids.Length];
		headerLine.GetComponent<TextMesh> ().text = header;
		for (int n = 0; n < ids.Length; n++) {
			switch (newFieldType) {
			    case MJAData.Field.ConcertGroup:
				    lineTexts [n] = MJAData.archive.concertGroups[ids[n]].nameEn;
				    break;
                case MJAData.Field.Concert:
                    lineTexts[n] = MJAData.archive.concerts[ids[n]].name;
                    break;
			case MJAData.Field.Image:
					lineTexts [n] = "image id: " + ids [n]; //MJAData.archive.images[ids[n]].id;
					break;
				case MJAData.Field.ImageTag:
					lineTexts[n] = MJAData.archive.imageTags[ids[n]].name;
					break;

                default:
				    lineTexts [n] = "here be dragons " + n;
				    break;
			}
		}
		Debug.Log ("New Data: " + idList.Length + " -> " + ids.Length);
	}

    public MJADetailView CreateDetailView()
    {
        MJADetailView view = null;
        switch (fieldType)
        {
            case MJAData.Field.ConcertGroup:
                view = new MJADetailViewConcertGroup("Concert Groups");
                break;
            case MJAData.Field.Concert:
                view = new MJADetailViewConcert("Concert");
                break;
			case MJAData.Field.Image:
				view = new MJADetailViewImage("Image");
				break;
			case MJAData.Field.ImageTag:
				view = new MJADetailViewImageTag("ImageTag");
				break;
            case MJAData.Field.Person:
                view = new MJADetailViewPerson("Person");
                break;
        }
        if (view != null)
            view.SetId(CurrentId);
        return view;
    }

	public int CurrentId {  get { return (currentIndex > -1 && currentIndex < ids.Length ? ids[currentIndex] : -1); } }
	public Transform transform { get { return gameObject.transform; } }
	public int Length { get { return (ids == null ? 0 : ids.Length); } }
}
