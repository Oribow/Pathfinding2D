using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FreeList<T> : IEnumerable<T> where T : System.IEquatable<T>
{
    public int Count { get; private set; }
    private List<FreeElement<T>> elements;
    private int firstFree = -1;
    private T nullElement;

    public FreeList(T nullElement)
    {
        elements = new List<FreeElement<T>>();
        this.nullElement = nullElement;
    }

    public FreeList(T nullElement, int initialCapacity)
    {
        elements = new List<FreeElement<T>>(initialCapacity);
        this.nullElement = nullElement;
    }

    public int Add(T element)
    {
        Count++;
        FreeElement<T> fe = new FreeElement<T>(element);
        if (firstFree != -1)
        {
            int index = firstFree;
            firstFree = elements[firstFree].next;
            elements[index] = fe;
            return index;
        }
        else
        {

            elements.Add(fe);
            return elements.Count - 1;
        }
    }

    public void Remove(int index)
    {
        Count--;
        elements[index] = new FreeElement<T>(nullElement, firstFree);
        firstFree = index;
    }

    public void Clear()
    {
        Count = 0;
        elements.Clear();
        firstFree = -1;
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var elem in elements)
        {
            if (!elem.element.Equals(nullElement))
                yield return elem.element;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var elem in elements)
        {
            if (!elem.element.Equals(nullElement))
                yield return elem.element;
        }
    }

    public T this[int index]
    {
        get { return elements[index].element; }
        set { elements[index] = new FreeElement<T>(value, elements[index].next); }
    }

    struct FreeElement<G>
    {
        public G element;
        public int next;

        public FreeElement(G element)
        {
            this.element = element;
            this.next = -1;
        }

        public FreeElement(G element, int next)
        {
            this.element = element;
            this.next = next;
        }
    }
}
