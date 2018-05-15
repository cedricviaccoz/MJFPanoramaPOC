#define USE_DXT_IMAGES
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class MJADetailView {
	public GameObject gameObject;
	public MJAData.Field fieldType;
	protected GameObject headerLine;
    protected int currentId = -1;

    protected GameObject mainImage;
    protected Texture2D mainImageTex;
    protected GameObject infoBlock;
    protected GameObject[] textLines;
    protected int currentIndex = -1;
    protected string[] relations;
    protected Vector2 relationOffset = new Vector2(0f, -1f);

    public MJADetailView(string name, MJAData.Field newFieldType) {
		fieldType = newFieldType;
		gameObject = new GameObject (name);
		transform.position = Vector3.zero;
		headerLine = new GameObject (name + " Header");
		headerLine.transform.SetParent (transform);
		var headText = headerLine.AddComponent<TextMesh> ();
		headText.text = name;
		headText.fontStyle = FontStyle.Bold;
		headText.anchor = TextAnchor.MiddleLeft;

        infoBlock = new GameObject(name + " Info");
        infoBlock.transform.SetParent(transform);
        var infoText = infoBlock.AddComponent<TextMesh>();
        infoText.text = name + "Info Block";
        infoText.fontStyle = FontStyle.Bold;
        infoText.anchor = TextAnchor.UpperLeft;

        mainImage = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mainImage.name = name + " Main Image";
        mainImage.transform.SetParent(transform);
        mainImageTex = new Texture2D(2, 2);
        mainImage.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Transparent");
        mainImage.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", mainImageTex);
    }

    public virtual void Cleanup()
    {
        MonoBehaviour.Destroy(gameObject);
    }

    public void SetupRelationLines()
    {
        textLines = new GameObject[relations.Length];
        for (int n = 0; n < relations.Length; n++)
        {
            textLines[n] = new GameObject(gameObject.name + " Relation " + n);
            textLines[n].transform.SetParent(transform);
            var text = textLines[n].AddComponent<TextMesh>();
            text.text = relations[n];
            text.anchor = TextAnchor.MiddleLeft;
        }
    }

    public virtual void Reshape() {
		var headText = headerLine.GetComponent<TextMesh> ();
		headText.fontSize = (MJAData.fontResolution * 3) / 2;
		headText.characterSize = 10f * MJAData.textScale / (float)MJAData.fontResolution;
		headerLine.transform.localPosition = new Vector3 (0f, 0f, 0f);

        for (int n = 0; n < relations.Length; n++)
        {
            var text = textLines[n].GetComponent<TextMesh>();
            text.fontSize = MJAData.fontResolution;
            text.characterSize = 10f * MJAData.textScale / (float)MJAData.fontResolution;
            textLines[n].transform.localPosition = new Vector3(relationOffset.x, -MJAData.lineSpacing * MJAData.textScale * (n + 1.5f) + relationOffset.y, 0f);
        }

        var infoText = infoBlock.GetComponent<TextMesh>();
        infoText.fontSize = MJAData.fontResolution;
        infoText.characterSize = 10f * MJAData.textScale / (float)MJAData.fontResolution;
        infoBlock.transform.localPosition = new Vector3(MJAData.mainImageSize * 1.1f, -MJAData.textScale, 0f);
        mainImage.transform.localScale = new Vector3(MJAData.mainImageSize, MJAData.mainImageSize, MJAData.mainImageSize);
        mainImage.transform.localPosition = new Vector3(MJAData.mainImageSize * 0.5f, -MJAData.mainImageSize * 0.5f - MJAData.textScale, 0f);
    }

    public virtual void Update()
    {
        for (int n = 0; n < relations.Length; n++)
        {
            var text = textLines[n].GetComponent<TextMesh>();
            if (n == currentIndex)
                text.color = new Color(1, 0.8f, 0.2f, 1);
            else
                text.color = new Color(1, 1, 1, 1);
            text.text = "[ " + relations[n] + " ]";
        }
    }
    public void SwapImage(int imageId)
    {
#if USE_DXT_IMAGES
        string imageUrl = MJAData.archive.images[imageId] != null ? MJAData.archive.images[imageId].mediaSources[1].url : "invalid";
        imageUrl = MJAData.archive.jsonPath + "/DXT5_s320" + imageUrl.Substring(6) + ".dxt";
//        string imageUrl = MJAData.archive.images[imageId] != null ? MJAData.archive.images[imageId].mediaSources[3].url : "invalid";
//        imageUrl = MJAData.archive.jsonPath + "/DXT5_l2048" + imageUrl.Substring(6) + ".dxt";

        mainImageTex = LoadTextureDXT.Load(imageUrl, true);
        if (mainImageTex == null || mainImageTex.width < 10)
        {
            Debug.Log("Error loading image: " + imageUrl);
            if (MJAData.defaultImageData != null)
                mainImageTex.LoadImage(MJAData.defaultImageData);
        }
        mainImage.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", mainImageTex);
#else
        string imageUrl = MJAData.archive.images[imageId] != null ? MJAData.archive.images[imageId].mediaSources[1].url : "invalid";
        if (imageUrl.StartsWith("file://"))
            imageUrl = MJAData.archive.jsonPath + "/images" + imageUrl.Substring(6);
        if (File.Exists(imageUrl))
        {
//            mainImageTex = new Texture2D(128, 128, TextureFormat.RGB24, false);
            if (!mainImageTex.LoadImage(File.ReadAllBytes(imageUrl)) || mainImageTex.width < 10)
            {
                Debug.Log("Error loading image: " + imageUrl);
                if (MJAData.defaultImageData != null)
                    mainImageTex.LoadImage(MJAData.defaultImageData);
            }
            else
            {
//                Debug.Log("Success loading image: " + imageUrl);
            }
            mainImage.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", mainImageTex);
        }
        else if (MJAData.defaultImageData != null)
                mainImageTex.LoadImage(MJAData.defaultImageData);
#endif
    }

    public abstract MJAListView CreateListView();

    public void SetId(int newId)
    {
        if (newId == currentId) return;
        currentId = newId;
        Update ();
    }

    public bool Move(int direction)
    {
        int oldIndex = currentIndex;
        Debug.Log ("Pre Move: curr: " + currentIndex + " len: " + relations);
        if (direction == 0)
        {
            currentIndex = 0;
        }
        else if (direction < 0)
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
        }
        else
        {
            currentIndex = Mathf.Min(relations.Length - 1, currentIndex + 1);
        }
        Debug.Log ("Post Move: curr: " + currentIndex + " len: " + relations);
        Update();

        for (int n = 0; n < relations.Length; n++)
        {
            var text = textLines[n].GetComponent<TextMesh>();
            if (n == currentIndex)
                text.color = new Color(1, 0.8f, 0.2f, 1);
            else
                text.color = new Color(1, 1, 1, 1);
        }
        return oldIndex != currentIndex;
    }

    public Transform transform { get { return gameObject.transform; } }
}

public class MJADetailViewConcertGroup : MJADetailView
{
    public MJADetailViewConcertGroup(string name) : base(name, MJAData.Field.ConcertGroup) {
        relationOffset = new Vector2(MJAData.mainImageSize * 1.1f, (MJAData.textScale * 2f + MJAData.lineSpacing) * -1.0f);
        relations = new string[2];
        relations[0] = "concerts";
        relations[1] = "images";
        SetupRelationLines();
    }

    public override void Update()
    {
        base.Update();
        var data = MJAData.archive.concertGroups[currentId];
        SwapImage(data.mainImage);
        headerLine.GetComponent<TextMesh>().text = data.nameEn;
        if (data.descriptionEn.Length > 50)
            infoBlock.GetComponent<TextMesh>().text = data.Date + "\n" + data.descriptionEn.Substring(0, 50) + " [..]";
        else
            infoBlock.GetComponent<TextMesh>().text = data.Date + "\n" + data.descriptionEn;
    }

    public override MJAListView CreateListView()
    {
        var data = MJAData.archive.concertGroups[currentId];
        MJAListView list = null;
        switch (currentIndex)
        {
            case 0: // concerts
                list = new MJAListView("concert list (concertGroup)");
                list.SetData(MJAData.Field.Concert, data.concerts, "Concerts: " + data.nameEn);
                break;
            case 1: // images
                list = new MJAListView("image list (concertGroup)");
                list.SetData(MJAData.Field.Image, data.mainImages, "Images: " + data.nameEn);
                break;
        }
        return list;
    }
}

public class MJADetailViewConcert : MJADetailView
{
    public MJADetailViewConcert(string name) : base(name, MJAData.Field.Concert)
    {
        relationOffset = new Vector2(MJAData.mainImageSize * 1.1f, -MJAData.textScale - MJAData.lineSpacing);
        relations = new string[2];
        relations[0] = "concert groups";
        relations[1] = "images";
        SetupRelationLines();
    }

    public override void Update()
    {
        base.Update();
        var data = MJAData.archive.concerts[currentId];
		SwapImage(data.images != null && data.images.Length > 0 ? data.images[0] : -1);
        headerLine.GetComponent<TextMesh>().text = data.name;
        infoBlock.GetComponent<TextMesh>().text = MJAData.archive.locations[data.location].name;
    }

    public override MJAListView CreateListView()
    {
        Debug.Log("MJADetailViewConcert CreateListView ID:" + currentId);
        var data = MJAData.archive.concerts[currentId];
        MJAListView list = null;
        switch (currentIndex)
        {
            case 0: // concertGroups
                list = new MJAListView("concert group list (concert)");
                list.SetData(MJAData.Field.ConcertGroup, data.concertGroupIds, "Concert Groups: " + data.name);
                break;
            case 1: // images
                list = new MJAListView("image list (concert)");
                list.SetData(MJAData.Field.Image, data.images, "Images: " + data.name);
                break;
        }
        return list;
    }
}

public class MJADetailViewImage : MJADetailView
{
	public MJADetailViewImage(string name) : base(name, MJAData.Field.Image)
	{
		relationOffset = new Vector2(MJAData.mainImageSize * 1.1f, -MJAData.textScale * 2f - MJAData.lineSpacing);
		relations = new string[3];
		relations[0] = "concerts";
		relations[1] = "tags";
		relations[2] = "artists";
		SetupRelationLines();
	}

	public override void Update()
	{
		base.Update();
		var data = MJAData.archive.images[currentId];
		SwapImage(data.id);
		headerLine.GetComponent<TextMesh>().text = "Image ID: " + data.id;
		infoBlock.GetComponent<TextMesh> ().text = 
			"Location: " + (data.locations.Length > 0 ? 
				(MJAData.archive.locations [data.locations [0]] != null ? MJAData.archive.locations [data.locations [0]].name : "invalid ID " + data.locations [0]) :
				"N/A") + 
			"\nDate: " + data.Date + 
			"\nCredits: " + data.credit;
	}

	public override MJAListView CreateListView()
	{
		Debug.Log("MJADetailViewImage CreateListView ID:" + currentId);
		var data = MJAData.archive.images[currentId];
		MJAListView list = null;
		switch (currentIndex)
		{
		case 0: // concerts
			list = new MJAListView("concert list (image)");
			list.SetData(MJAData.Field.Concert, data.concerts, "Concerts: Image ID=" + data.id);
			break;
		case 1: // tags
			list = new MJAListView("tag list (image)");
			list.SetData(MJAData.Field.ImageTag, data.tags, "Tags: Image ID=" + data.id);
			break;
		case 2: // artists
			list = new MJAListView("artist list (image)");
			list.SetData(MJAData.Field.Person, data.artists, "Artists: Image ID=" + data.id);
			break;
		}
		return list;
	}
}

public class MJADetailViewImageTag : MJADetailView
{
	public MJADetailViewImageTag(string name) : base(name, MJAData.Field.ImageTag)
	{
		relationOffset = new Vector2(0, 0);
		relations = new string[1];
		relations[0] = "images";
		SetupRelationLines();
		mainImage.GetComponent<MeshRenderer> ().enabled = false;
	}

	public override void Update()
	{
		base.Update();
		var data = MJAData.archive.imageTags[currentId];
		headerLine.GetComponent<TextMesh>().text = "ImageTag: " + data.name;
		infoBlock.GetComponent<TextMesh>().text = "";
	}

	public override MJAListView CreateListView()
	{
		Debug.Log("MJADetailViewImage CreateListView ID:" + currentId);
		var data = MJAData.archive.imageTags[currentId];
		MJAListView list = null;
		switch (currentIndex)
		{
		case 0: // images
			list = new MJAListView("image list (imageTag)");
			list.SetData(MJAData.Field.Image, data.images, "Images with tag " + data.name);
			break;
		}
		return list;
	}
}

public class MJADetailViewPerson : MJADetailView
{
    public MJADetailViewPerson(string name) : base(name, MJAData.Field.Person)
    {
        relationOffset = new Vector2(MJAData.mainImageSize * 1.1f, -MJAData.textScale - MJAData.lineSpacing);
        relations = new string[2];
        relations[0] = "concerts";
        relations[1] = "images";
        SetupRelationLines();
    }

    public override void Update()
    {
        base.Update();
        var data = MJAData.archive.persons[currentId];
        SwapImage(data.mainImage);
        headerLine.GetComponent<TextMesh>().text = data.publicName;
        infoBlock.GetComponent<TextMesh>().text = data.nickname;
    }

    public override MJAListView CreateListView()
    {
        Debug.Log("MJADetailViewPerson CreateListView ID:" + currentId);
        var data = MJAData.archive.persons[currentId];
        MJAListView list = null;
        switch (currentIndex)
        {
            case 0: // concerts
                list = new MJAListView("concert list (person)");
                list.SetData(MJAData.Field.Concert, data.concerts, "Concerts: " + data.fullName);
                break;
            case 1: // images
                list = new MJAListView("image list (person)");
                list.SetData(MJAData.Field.Person, data.images, "Images: " + data.fullName);
                break;
        }
        return list;
    }
}