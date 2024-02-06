using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibLouisWrapperTestCmd
{
    internal class TestResult
    {
        private bool result = true;
        public bool Result { get { return result; } set { result = value;} }

        private readonly List<string> errorList = new List<string>(); 
        public List<string> ErrorList { get { return errorList; } } 
        private int successes = 0;
        public int Successes { get { return successes; } set { successes = value; } }
        private readonly DiffList allDiffs = DiffList.Create();
        public DiffList AllDiffs {  get { return allDiffs; } }

        public void AddRange(TestResult that)
        {
            this.errorList.AddRange(that.errorList);
            this.successes += that.successes;
            this.allDiffs.AddRange(that.allDiffs);
        }


        private TestResult()
        { }

        public static TestResult Create()
        {
            return new TestResult();
        }
    }
}
