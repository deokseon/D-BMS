using System.Collections.Generic;

public class ListExtension<T> : List<T>
{
	/*public T Peek
	{
		get { return this[Count - 1]; }
	}*/

	public T Peek() { return this[Count - 1]; }

	public void RemoveLast() => RemoveAt(Count - 1);

}
