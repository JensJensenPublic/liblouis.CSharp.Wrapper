using System;
using System.Collections.Generic;
using liblouis.CSharp.Wrapper;

namespace LibLouisWrapperTestCmd
{
    /// <summary>
    /// Simple class for manipulating a list of Diff objects, used for counting various types of differences found
    /// </summary>
    internal class Diff
    {
        internal static Diff Create(string s)
        {
            return new Diff(s);
        }

        private readonly string description;
        internal string Description { get { return description; } }
        private int count = 0;
        internal int Count { get { return count; } }

        internal void IncrementCount()
        {
            count++;
        }

        private Diff(string s)
        {
            description = s;
            count = 1;
        }
    }


    internal class DiffList
    {
        private readonly List<Diff> diffs = new List<Diff>();
        internal List<Diff> Diffs { get { return diffs; } }  

        private DiffList()
        { }

        internal static DiffList Create()
        {
            return new DiffList();
        }

        internal int AddRange(DiffList that)
        {   
            foreach (Diff diff in that.diffs)
            {
                this.diffs.Add(diff);  
            }
            return this.diffs.Count; 
        }

        internal int Add(String s)
        {
            int index = diffs.FindIndex(d => d.Description.Equals(s));
            bool found = ( -1 != index);
            Diff diff;
            if (found)
            {
                diff = diffs[index];
                diff.IncrementCount();
            }
            else
            {
                diff = Diff.Create(s);
                diffs.Add(diff);
            }
            return diff.Count;          
        }
    }
}
