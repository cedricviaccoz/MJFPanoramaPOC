namespace MJADataset
{

    public class BandList : DatasetList<Band>
    {
        public Band[] bands;

        public override void PrepareData()
        {
            items = bands;
            base.PrepareData();
        }
    }

    public class PersonList : DatasetList<Person>
    {
        public Person[] persons;

        public override void PrepareData()
        {
            items = persons;
            base.PrepareData();
        }
    }

    public class ConcertList : DatasetList<Concert>
    {
        public Concert[] concerts;

        public override void PrepareData()
        {
            items = concerts;
            base.PrepareData();
            for (int n = concerts.Length - 1; n >= 0; n--)
            {
                concerts[n].PrepareData();
            }
        }
    }

    public class ConcertGroupList : DatasetList<ConcertGroup>
    {
        public ConcertGroup[] concertGroups;

        public override void PrepareData()
        {
            items = concertGroups;
            base.PrepareData();
        }
    }

    public class ImageList : DatasetList<Image>
    {
        public Image[] images;

        public override void PrepareData()
        {
            items = images;
            base.PrepareData();
        }
    }

    public class ImageTagList : DatasetList<ImageTag>
    {
        public ImageTag[] imagetags;

        public override void PrepareData()
        {
            items = imagetags;
            base.PrepareData();
        }
    }

    public class LocationList : DatasetList<Location>
    {
        public Location[] locations;

        public override void PrepareData()
        {
            items = locations;
            base.PrepareData();
        }
    }

}