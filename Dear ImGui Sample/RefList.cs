using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dear_ImGui_Sample
{
    class RefList<T>
    {
        private T[] Data;
        public int Count { get; private set; }

        public RefList()
        {
            Data = Array.Empty<T>();
            Count = 0;
        }

        public void EnsureSize(int size)
        {
            if (Data.Length < size)
            {
                int newLength = Data.Length + (Data.Length / 2);
                if (newLength < size) newLength = size;
                
                var oldData = Data;
                var newData = new T[newLength];
                Array.Copy(oldData, newData, oldData.Length);

                Data = newData;
            }
        }

        public ref T Add()
        {
            EnsureSize(Count + 1);
            return ref Data[Count++];
        }

        public void Add(T val)
        {
            EnsureSize(Count + 1);
            Data[Count++] = val;
        }

        public void Clear()
        {
            // If this is a list of reference types we want to release the refrences
            if (typeof(T).IsValueType == false)
            {
                Array.Clear(Data, 0, Count);
            }

            Count = 0;
        }

        public bool Contains(T val)
        {
            var data = Data;
            for (int i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(val, data[i]))
                    return true;
            }

            return false;
        }

        public bool TryGetIndexOf(T val, out int index)
        {
            var data = Data;
            for (int i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(val, data[i]))
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }

        public ref T this[int i]
        {
            get
            {
                if (i < 0 || i >= Count) throw new IndexOutOfRangeException();
                return ref Data[i];
            }
        }
    }
}
