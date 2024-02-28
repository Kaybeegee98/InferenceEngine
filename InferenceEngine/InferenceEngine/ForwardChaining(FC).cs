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
        static void FC(string line, List<KnowledgeBase> KB)                     //Forward Chaining Employs a BFS tactic to open all sections before continuing
        {
            Queue<KnowledgeBase> q = new Queue<KnowledgeBase>();
            List<string> knowledgeList = new List<string>();

            HashSet<KnowledgeBase> knownBase = new HashSet<KnowledgeBase>();

            for (int i = 0; i < KB.Count; i++)                                  //Search for all instances where the sentance is just a variable, indication starter propositions
            {
                if (KB[i].result == "")
                {
                    q.Enqueue(KB[i]);
                }
            }

            while (q.Count > 0)
            {
                KnowledgeBase tempBase = q.Dequeue();                           //Dequeue the Knowledge Base that has existed the longest (FIFO)
                knownBase.Add(tempBase);                                        //Keep a record of all visited nodes in a HashSet

                if (tempBase.result == "")                                      //Adding the current base to a list of Strings. If Result is "", then only add the Percept
                {
                    knowledgeList.Add(tempBase.percept);
                }
                else if(tempBase.percept.Contains("&"))                         //If percept contains "&", add both percept and result, since "&" percept would not be in there already
                {
                    knowledgeList.Add(tempBase.percept);
                    knowledgeList.Add(tempBase.result);
                }
                else
                {
                    knowledgeList.Add(tempBase.result);                         //Otherwise, just add result
                }

                if (tempBase.result == line)                                    //If current sentence's result is the desired outcome, then we have found the answer
                {
                    Console.Write("YES: ");

                    for (int k = 0; k < knowledgeList.Count; k++)               //loop through everything in our string list (Should be in the correct order)
                    {
                        if (k != knowledgeList.Count - 1)
                        {
                            Console.Write(knowledgeList[k] + ", ");
                        }
                        else
                        {
                            Console.Write(knowledgeList[k]);
                        }
                    }
                    Console.WriteLine();
                    return;                                                     //Exit Program
                }

                for (int j = 0; j < KB.Count; j++)                              //For every sentence in Knowledge Base
                {
                    if (!knownBase.Contains(KB[j]))                             //If the sentence has not been seen before, continue
                    {
                        if (KB[j].percept.Contains("&"))                   //Otherwise if the looped percept contains "&", do extra things
                        {
                            List<string> split = new List<string>();
                            foreach (string quaries in KB[j].percept.Split("&", StringSplitOptions.RemoveEmptyEntries)) //Get all letters in the looped sentence's percept
                            {
                                split.Add(quaries);
                            }

                            if (split.All(x => knowledgeList.Contains(x)) && !q.Contains(KB[j]))   //If we have seen this symbol before, and it's not currently contained in the Queue, then we add it
                            {
                                q.Enqueue(KB[j]);
                            }
                        }
                        else if (KB[j].percept == tempBase.result)              //Otherwise if the looped sentence's percept and our current sentence's result match
                        {
                            q.Enqueue(KB[j]);                                   //Queue current looped sentence
                        }
                        else if (tempBase.result == "")                              //If current sentence is a starter proposition
                        {
                            if (KB[j].percept == tempBase.percept && KB[j].result != "")    //Check if the looped sentence's percept matches our current sentence's percept (Ignoring when looped sentence's result is "")
                            {
                                q.Enqueue(KB[j]);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Failed");                                        //If Queue is ever empty without reaching the target, then it failed. No Path Exists
        }
    }
}
