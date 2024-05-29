using System;
using System.Collections;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public readonly struct TokenRange : IEnumerable<int>
    {
        public readonly int Start;
        public readonly int End;
        public int Count => End - Start + 1;

        public TokenRange(int start, int end)
        {
            Start = start;
            End = end;
        }


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