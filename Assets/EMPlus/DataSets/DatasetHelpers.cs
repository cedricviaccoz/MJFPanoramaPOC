using System;
using System.Collections;

public abstract class DatasetReferenced
{
	public int id;
}

public class DatasetList<T> where T : DatasetReferenced
{
	public string exportDate;
	public T[] items;
	protected SparseArray byId;
	public T this[int id]
	{
		get { return byId.GetValue(id) as T; }
	}
	public virtual void PrepareData()
	{
		byId = new SparseArray();
		for (int n = items.Length - 1; n >= 0; n--)
			byId[items[n].id] = items[n];
	}
	public int[] GetAllIds()
	{
		int[] ids = new int[items.Length];
		for (int n = 0; n < items.Length; n++)
			ids[n] = items[n].id;
		return ids;
	}
    public IEnumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }
}

