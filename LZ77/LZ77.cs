using System.Diagnostics;

namespace LZ77
{
    public class LZ77
    {
        /*   Offset (ushort) |  Length (ushort)  |   Next   */
        /* 00000000 00000000 | 00000000 00000000 | 00000000 */

        private readonly CircularBuffer<byte> dict, buff;
        private uint position;
        public LZ77(ushort dictSize = 16, ushort buffSize = 8)
        {
            dict = new(dictSize);
            buff = new(buffSize);
            position = 0;
        }

        public (byte[], TimeSpan, TimeSpan) Encode(byte[] data)
        {
            TimeSpan forShifts = new();
            TimeSpan forMatching = new();

            Stopwatch watch = new();

            List<byte> result = new();
            buff.EnqueueRange(data[0..buff.Size]);
            while (!buff.IsEmpty)
            {
                watch.Start();
                (ushort offset, ushort length) = FindMatching();
                watch.Stop();
                forMatching += watch.Elapsed;
                watch.Restart();
                Shift(ref data, (ushort)(length + 1));
                watch.Stop();
                forShifts += watch.Elapsed;
                watch.Reset();

                result.AddRange(BitConverter.GetBytes(offset));
                result.AddRange(BitConverter.GetBytes(length));
                if (position - 1 < data.Length)
                    result.Add(data[position - 1]);
            }

            return (result.ToArray(), forMatching, forShifts);
        }

        // TODO: оОптимизация???
        public (ushort, ushort) FindMatching()
        {
            ushort offset = 0, length = 0;
            for (int i = dict.Count - 1; i > 0; i--)
            {
                if (buff.Peek(0) != dict.Peek(i)) continue;
                ushort newLength = 0;

                while (true)
                {
                    if (newLength >= buff.Count || GetInWindow(i + newLength) != buff.Peek(newLength)) break;
                    newLength++;
                }

                if (newLength > length)
                {
                    offset = (ushort)(dict.Count - i);
                    length = newLength;
                }
            }

            return (offset, length);
        }

        // FIXME: Нечётная логика поведения. Может вернуть 0 если ничего не найдено
        private byte GetInWindow(int index)
        {
            byte result = 0;
            for (int i = 0; i < dict.Count + buff.Count; i++)
            {
                if (i == index)
                {
                    result = i < dict.Count ? dict.Peek(i) : buff.Peek(i - dict.Count);
                    break;
                }
            }

            return result;
        }

        public void Shift(ref byte[] data, ushort length)
        {
            for (ushort i = 0; i < length; i++)
            {
                if (dict.IsFull) dict.Dequeue();
                if (!buff.IsEmpty)
                    dict.Enqueue(buff.Dequeue());
                if (position + 1 + buff.Count < data.Length)
                {
                    buff.Enqueue(data[++position + buff.Count]);
                }
                else // FIXME: Чертовщина какая-то
                {
                    position++;
                }
            }
        }

        // FIXME: Сделано на отъебись 
        public byte[] Decode(byte[] data)
        {
            List<byte> result = new();

            for (int i = 0; i < data.Length - 1; i += 5)
            {
                ushort offset = BitConverter.ToUInt16(new byte[] { data[i], data[i + 1] });
                ushort length = BitConverter.ToUInt16(new byte[] { data[i + 2], data[i + 3] });
                if (result.Count == 0)
                {
                    result.Add(data[i + 4]);
                }
                else
                {
                    int start = result.Count;
                    for (ushort j = 0; j < length; j++)
                    {
                        result.Add(result[start - offset + j]);
                    }
                    if (i + 4 < data.Length)
                        result.Add(data[i + 4]);
                }
            }

            return result.ToArray();
        }
    }
}