using System;
using System.Collections;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public readonly struct TokenRange(int start, int end) : IEnumerable<int>
    {
        public readonly int Start = start;
        public readonly int End = end;
        public int Count => End - Start + 1;


        public static TokenRange operator +(TokenRange current, TokenRange other)
        {
            return new TokenRange(Math.Min(current.Start, other.Start), Math.Max(current.End, other.End));
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (var i = Start; i <= End; i++)
            {
                yield return i;
            }
        }

        public override string ToString()
        {
            return $"[{Start},{End}]";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}