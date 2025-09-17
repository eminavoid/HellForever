namespace Unity.FPS.Ours
{
    public class GenericLinkedStack<T>
    {
        public class Node
        {
            public T Value;
            public Node Next;
            public Node(T v) { Value = v; }
        }

        Node _top;
        public int Count { get; private set; }

        public void Push(T item)
        {
            var n = new Node(item) { Next = _top };
            _top = n; Count++;
        }

        public bool TryPop(out T item)
        {
            if (_top == null) { item = default; return false; }
            item = _top.Value;
            _top = _top.Next; Count--; return true;
        }

        public bool TryPeek(out T item)
        {
            if (_top == null) { item = default; return false; }
            item = _top.Value; return true;
        }

        public void Clear() { _top = null; Count = 0; }

        // Utility: remove first match (by predicate) and return it
        public bool TryRemove(System.Predicate<T> pred, out T removed)
        {
            removed = default;
            Node prev = null, cur = _top;
            while (cur != null)
            {
                if (pred(cur.Value))
                {
                    removed = cur.Value;
                    if (prev == null) _top = cur.Next;
                    else prev.Next = cur.Next;
                    Count--;
                    return true;
                }
                prev = cur; cur = cur.Next;
            }
            return false;
        }
    }
}
