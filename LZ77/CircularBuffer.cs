namespace LZ77
{
    // Простейший кольцевой буфер
    public class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private int _head, _tail, _count;

        public int Count => _count;
        public bool IsEmpty => _count == 0;
        public bool IsFull => _count == _buffer.Length;
        public int Size => _buffer.Length;

        public CircularBuffer(int size)
        {
            if (size < 0) throw new IndexOutOfRangeException($"Индекс \"{size}\" меньше минимально возможной длины \"0\"");

            _buffer = new T[size];
            _tail = 0;
            _head = _buffer.Length - 1;
        }

        public void Enqueue(T value)
        {
            if (IsFull) throw new IndexOutOfRangeException("Переполнение очереди");

            _head = Loop(_head + 1);
            _buffer[_head] = value;
            _count++;
        }

        public void EnqueueRange(T[] values)
        {
            if (values.Length > _buffer.Length || values.Length + _count > _buffer.Length)
                throw new IndexOutOfRangeException("Попытка поместить в очередь больше элементов чем она может уместить");

            for (int i = 0; i < values.Length; i++)
                Enqueue(values[i]);
        }

        public T Dequeue()
        {
            if (IsEmpty) throw new InvalidOperationException("Попытка получить значение из пустой очереди");

            T result = _buffer[_tail];
            _tail = Loop(_tail + 1);
            _count--;

            return result;
        }

        public T[] DequeueCount(int count)
        {
            ExceptOutOfRange(count);
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = Dequeue();

            return result;
        }

        public T Peek(int index)
        {
            ExceptOutOfRange(index);
            return _buffer[Loop(_tail + index)];
        }

        public void Clear()
        {
            _tail = 0;
            _head = _buffer.Length - 1;
        }

        private int Loop(int val) => (val) % _buffer.Length;

        private void ExceptOutOfRange(int val)
        {
            if (val >= _count) throw new IndexOutOfRangeException($"Индекс \"{val}\" больше количества элементов очереди \"{_count}\"");
            if (val < 0) throw new IndexOutOfRangeException($"Индекс \"{val}\" меньше минимально возможной длины \"0\"");
        }
    }
}
