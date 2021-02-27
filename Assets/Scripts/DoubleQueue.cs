using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DoubleQueue<T>
{
    private readonly Queue<T> first = new Queue<T>();
    private readonly Queue<T> second = new Queue<T>();
    private bool isFirst = true;

    public int Count
    {
        get => first.Count + second.Count;
    }
    public int CurrentCount
    {
        get => isFirst ? first.Count : second.Count;
    }
    public Queue<T> Current
    {
        get => isFirst ? first : second;
    }
    public void ClearCurrent()
    {
        var cur = isFirst ? first : second;
        cur.Clear();
    }
    public void Clear()
    {
        first.Clear();
        second.Clear();
    }
    public void Enqueue(T t)
    {
        var next = isFirst ? second : first;
        next.Enqueue(t);
    }
    public T Dequeue()
    {
        var cur = isFirst ? first : second;
        return cur.Dequeue();
    }
    public bool Contains(T t)
    {
        return first.Contains(t) && second.Contains(t);
    }
    public T Peek()
    {
        var cur = isFirst ? first : second;
        return cur.Peek();
    }
    public void Swap()
    {
        isFirst = !isFirst;
    }
}

