using System.Collections.Generic;

namespace Unity.FPS.Pathfinding
{
    internal class MinHeapInt
    {
        readonly List<int> items = new List<int>();
        readonly List<float> prio = new List<float>();
        readonly Dictionary<int, int> pos = new Dictionary<int, int>();

        public int Count => items.Count;

        public void Insert(int item, float priority)
        {
            items.Add(item);
            prio.Add(priority);
            int i = items.Count - 1;
            pos[item] = i;
            Up(i);
        }

        public bool TryUpdate(int item, float newPriority)
        {
            if (!pos.TryGetValue(item, out int i)) return false;
            float old = prio[i];
            prio[i] = newPriority;
            if (newPriority < old) Up(i);
            else if (newPriority > old) Down(i);
            return true;
        }

        public int ExtractMin(out float priority)
        {
            priority = 0f;
            int n = items.Count;
            if (n == 0) return -1;

            int m = items[0];
            priority = prio[0];

            int last = items[n - 1];
            float lastP = prio[n - 1];

            items[0] = last; prio[0] = lastP; pos[last] = 0;
            items.RemoveAt(n - 1); prio.RemoveAt(n - 1); pos.Remove(m);

            if (items.Count > 0) Down(0);
            return m;
        }

        void Up(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (prio[i] >= prio[p]) break;
                Swap(i, p);
                i = p;
            }
        }

        void Down(int i)
        {
            int c = items.Count;
            while (true)
            {
                int l = 2 * i + 1, r = 2 * i + 2, s = i;
                if (l < c && prio[l] < prio[s]) s = l;
                if (r < c && prio[r] < prio[s]) s = r;
                if (s == i) break;
                Swap(i, s);
                i = s;
            }
        }

        void Swap(int a, int b)
        {
            int ia = items[a], ib = items[b];
            float pa = prio[a], pb = prio[b];
            items[a] = ib; prio[a] = pb; pos[ib] = a;
            items[b] = ia; prio[b] = pa; pos[ia] = b;
        }
    }
}