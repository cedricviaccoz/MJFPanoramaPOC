namespace CERNDataset
{

    public class ImageList : DatasetList<Image>
    {
        public Image[] files;

        public override void PrepareData()
        {
            items = files;
            base.PrepareData();
        }
    }
    public class RecordList : DatasetList<Record>
    {
        public Record[] records;

        public override void PrepareData()
        {
            items = records;
            base.PrepareData();
        }
    }
    public class AuthorList : DatasetList<Author>
    {
        public Author[] authors;

        public override void PrepareData()
        {
            items = authors;
            base.PrepareData();
        }
    }
    public class KeywordList : DatasetList<Keyword>
    {
        public Keyword[] keywords;

        public override void PrepareData()
        {
            items = keywords;
            base.PrepareData();
        }
    }
    public class SubjectList : DatasetList<Subject>
    {
        public Subject[] subjects;

        public override void PrepareData()
        {
            items = subjects;
            base.PrepareData();
        }
    }
    public class CopyrightHolderList : DatasetList<CopyrightHolder>
    {
        public CopyrightHolder[] copyrightHolders;

        public override void PrepareData()
        {
            items = copyrightHolders;
            base.PrepareData();
        }
    }
    public class CollectionList : DatasetList<Collection>
    {
        public Collection[] collections;

        public override void PrepareData()
        {
            items = collections;
            base.PrepareData();
        }
    }

}
