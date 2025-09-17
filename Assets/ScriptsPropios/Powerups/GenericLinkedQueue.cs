namespace Unity.FPS.Ours
{
    public class GenericLinkedQueue<T>
    {
        public class Node
        {
            public T Value;
            public Node Next;
            public Node(T v) { Value = v; }
        }

        Node _head;
        Node _tail;
        public int Count { get; private set; }

        public void Enqueue(T item)
        {
            var n = new Node(item);
            if (_tail == null) _head = _tail = n;
            else { _tail.Next = n; _tail = n; }
            Count++;
        }

        public bool TryDequeue(out T item)
        {
            if (_head == null) { item = default; return false; }
            item = _head.Value;
            _head = _head.Next;
            if (_head == null) _tail = null;
            Count--;
            return true;
        }

        public void Clear()
        {
            _head = _tail = null; Count = 0;
        }
    }
}
