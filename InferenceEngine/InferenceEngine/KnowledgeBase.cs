using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InferenceEngine
{
    internal class KnowledgeBase
    {
        public string percept;                                  //Left Side of =>, or Singular variable
        public string result;                                   //Right side of =>, or "" if associated to singular variable

        public KnowledgeBase(string Percept, string Result)
        {
            percept = Percept;
            result = Result;
        }
    }
}
