using InferenceEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InferenceEngine
{
    public partial class Program
    {
        static List<KnowledgeBase> KBTell(string line)
        {
            List<KnowledgeBase> KB = new List<KnowledgeBase>();                                             //Create a new List of Knowledge Bases
            List<string> list = new List<string>();
            char[] delimiters = { '=', '>'};                                                                //Current Delimiters of "=>"

            foreach (string phrase in line.Split(';', StringSplitOptions.RemoveEmptyEntries))               //Loop through all string in the current line, spliting by ;
            {
                list.Add(phrase);                                                                           //Add to each sentance to a list of strings
            }

            foreach (string l in list)                                                                      //For every string in the list of strings, do the following:
            {
                List<string> strings = new List<string>();

                foreach (string quaries in l.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))      //Attempt to split the sentance by =>
                {
                    strings.Add(quaries);
                }

                if (strings.Count > 1)                                                                      //If it create more than 1 string, it is a full sentance
                {
                    KB.Add(new KnowledgeBase(strings[0], strings[1]));                                      //[0] is the left side of the sentance, [1] is the right side
                }
                else
                {
                    KB.Add(new KnowledgeBase(strings[0], ""));                                              //If there is not 2 strings, then it is just a variable. [0] is the variable, set the result to ""
                }
            }

            for (int t = 0; t < KB.Count; t++)                                                              //Loop through everything in Knowledge Base and Trim any white spaces
            {
                KB[t].percept = KB[t].percept.Trim();
                KB[t].result = KB[t].result.Trim();
            }
            return KB;                                                                                      //Return Knowledge Base
        }
    }
}
