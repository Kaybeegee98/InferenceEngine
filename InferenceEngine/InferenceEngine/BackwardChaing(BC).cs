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
        static void BC(string line, List<KnowledgeBase> KB)                                     //Backwards Chaining employs DFS to find out how d exists
        {
            Stack<KnowledgeBase> stack = new Stack<KnowledgeBase>();
            List<string> knowledgeList = new List<string>();

            HashSet<KnowledgeBase> knownBase = new HashSet<KnowledgeBase>();

            List<string> multiplePropositions = new List<string>();                             //Used for when the & symbol is spotted

            for (int i = 0; i < KB.Count; i++)
            {
                if (KB[i].result == line)                                                       //search for the target in KB. Assume that there is always a target
                {
                    stack.Push(KB[i]);
                }
            }

            if(stack.Count == 0)                                                                //Quick check to make sure it was found, if not then print the error
            {
                Console.WriteLine("NO: Target is not a possible solution in given File");
            }

            while (stack.Count > 0)
            {
                KnowledgeBase tempBase = stack.Pop();                                           //LIFO: Pop out the most recent object

                if (multiplePropositions.Contains(tempBase.result))                             //Check if multiplePropositions contains the tempBase
                {
                    multiplePropositions.Remove(tempBase.result);
                }
                else if (tempBase.result == "" && multiplePropositions.Contains(tempBase.percept))
                {
                    multiplePropositions.Remove(tempBase.percept);
                }

                if (knownBase.Contains(tempBase))                                               //Catch duplicate Result
                {
                    bool fixedIssue = false;                                                    //Create a loop until a solution is found

                    while(!fixedIssue)
                    {
                        if (!tempBase.percept.Contains("&"))                                    //if no & in percept, simple fix
                        {
                            if (knownBase.Any(x => x.result == "" && x.percept == tempBase.percept))                            //if knowbase already contains a propisition that is just the percept, then the soultion is easy
                            {
                                tempBase = knownBase.ToList().Find(x => x.result == "" && x.percept == tempBase.percept);
                                fixedIssue = true;
                            }
                            else if (knownBase.Any(x => x.result == tempBase.percept))                                          //Otherwise, check if there is anything in knownBase that matches the current base
                            {
                                tempBase = knownBase.ToList().Find(x => x.result == tempBase.percept);                          //if yes, t
                            }
                            else                                                                //if no, then find in the orignal KB where the result would be
                            {
                                tempBase = KB.Find(x => x.result == tempBase.percept);
                                fixedIssue = true;
                            }
                        }
                        else                                                                    //if it contains & in percept, then lengthy solution
                        {
                            KnowledgeBase backupBase = tempBase;                                //create backup of tempbase

                            List<string> divide = new List<string>();
                            foreach (string s in tempBase.percept.Split("&", StringSplitOptions.RemoveEmptyEntries))        //split percept into it's component parts
                            {
                                divide.Add(s);
                            }
                            foreach (string s in divide)                                        //loop through each string in the divide list
                            {
                                if (knownBase.Any(x => x.result == "" && x.percept == s))
                                {
                                    backupBase = knownBase.ToList().Find(x => x.result == "" && x.percept == s);
                                }
                                else if (knownBase.Any(x => x.result == s))                     //first two if's are mostly to see if knowbase contains it.
                                {
                                    backupBase = knownBase.ToList().Find(x => x.result == s);   //if it does, than just set backup based to the next node
                                }
                                else
                                {
                                    tempBase = KB.Find(x => x.result == s);                     //if it does not contain a solution, then grab one from the original KB
                                    fixedIssue = true;                                          //Then make fixed issue true, since there's no longer a duplicate
                                    break;
                                }
                            }

                            if (fixedIssue)                                         //If it was fixed, then leave loop
                            {
                                break;
                            }
                            else
                            {
                                tempBase = backupBase;                              //if not, then just make tempBase the last backUp base, since all paths were already solved
                            }
                        }
                    }
                }

                if (tempBase.result != "")                                          //Adding the neccessary text to the knowledgeList to aid with printing later
                {
                    knowledgeList.Add(tempBase.result);
                    if (tempBase.percept.Contains("&"))
                    {
                        knowledgeList.Add(tempBase.percept);                        //Add percept if & is present, mostly for extra context
                    }
                }
                else
                {
                    knowledgeList.Add(tempBase.percept);
                }
                knownBase.Add(tempBase);                                            //Add the base into knowBase, to try and prevent duplicates

                if (tempBase.result == "" && multiplePropositions.Count == 0)       //If tempBase is at a starter propistion, and there is no other paths to go done
                {
                    Console.Write("YES: ");

                    knowledgeList = Enumerable.Reverse(knowledgeList).ToList();     //Reverse the List
                    HashSet<string> keys = new HashSet<string>();                   //Prevent Duplicate Symbols being add

                    for (int i = 0; i < knowledgeList.Count; i++)                   //Loop through the entire string list, and print each one if not already printer
                    {
                        if (i == knowledgeList.Count - 1)
                        {
                            if (!keys.Contains(knowledgeList[i]))
                            {
                                Console.Write(knowledgeList[i]);
                                keys.Add(knowledgeList[i]);
                            }
                        }
                        else
                        {
                            if (!keys.Contains(knowledgeList[i]))
                            {
                                Console.Write(knowledgeList[i] + ", ");
                                keys.Add(knowledgeList[i]);
                            }
                        }
                    }
                    Console.WriteLine();
                    return;                                                         //Exit out once all strings are printed. Job Well Done!!
                }

                if(tempBase.percept.Contains("&"))                                  //If the percept contains an & symbol, extra work is needed
                {
                    List<string> split = new List<string>();
                    foreach (string quaries in tempBase.percept.Split("&", StringSplitOptions.RemoveEmptyEntries))  //Split all strings out of percept
                    {
                        split.Add(quaries);
                    }

                    for (int i = 0; i < KB.Count; i++)                              //Go through ever object in KB
                    {
                        if (!knownBase.Contains(KB[i]))                             //Make sure there's no duplicates
                        {
                            if (KB[i].result == "" && split.Contains(KB[i].percept))                //Check if split contains a node that would fit into the KB.percept
                            {
                                int index = split.FindIndex(s => s == KB[i].percept);               //Find the above node
                                multiplePropositions.Add(split[index]);                             //Add node the multiplePropositions to track paths to follow
                                stack.Push(KB[i]);
                                split.Remove(KB[i].percept);                                        //Remove the found path from possible symbols
                            }
                            else if (split.Contains(KB[i].result))                  //Check if split contains a node that matches KB.result
                            {
                                int index = split.FindIndex(s => s == KB[i].result);                //Same as Above
                                multiplePropositions.Add(split[index]);
                                stack.Push(KB[i]);
                                split.Remove(KB[i].result);
                            }
                            else if(!KB.Any(k =>  (split.Contains(k.result)) || (k.result == "" && split.Contains(k.percept))) && (split.Count > 0))        //Make sure that the split actually exists in other sections of the knowledgebase as either a starter proposition or result from a sentance
                            {
                                Console.WriteLine("Failed");                        //If split doesn't exist anywhere else, then path is unsolvable
                                return;
                            }
                        }
                        else if(split.Any(s => s == KB[i].result))                  //If knowBase does contain, check if any string in split is not a starter proposition
                        {
                            bool finished = false;                                  //If yes, start lengthy loop to add in the path needed to finish up to this point
                            KnowledgeBase current = KB[i];
                            List<KnowledgeBase> paths = new List<KnowledgeBase>();  //Create a paths, make sure it doesn't start finished, and assing current to KB[i]

                            while (!finished && paths.Count == 0)                   //If finished is true, and all paths are completed
                            {
                                if (paths.Count == 0)                               //If all paths have been explored
                                {
                                    if (current.result == "")                       //Check if current is a starter proposition
                                    {
                                        knowledgeList.Add(current.percept);         //If yes, then add the percept  to string list and finish.
                                        finished = true;
                                    }
                                    else
                                    {
                                        knowledgeList.Add(current.result);          //If no, add the result and check if percept contains & symbol
                                        if (current.percept.Contains("&"))
                                        {
                                            List<string> swap = new List<string>(); //If it does, then split it into it's parts
                                            foreach (string s in tempBase.percept.Split("&", StringSplitOptions.RemoveEmptyEntries))
                                            {
                                                swap.Add(s);
                                            }

                                            foreach (string symbols in swap)        //Loop through all symbols in swap, and add them to paths if they can be found in knowBas
                                            {
                                                paths.Add(knownBase.ToList().Find(k => (k.result == symbols) || (k.result == "" && k.percept == symbols)));
                                            }
                                            current = paths[0];                     //Make current equal to the first in the varying paths
                                        }
                                        else                                        //If no & Symbol, simply search for matching result in knowbase.
                                        {
                                            if(knownBase.Any(k => (k.result == current.percept) || (k.result == "" && k.percept == current.percept)))
                                            {
                                                current = knownBase.ToList().Find(k => (k.result == current.percept) || (k.result == "" && k.percept == current.percept));
                                            }
                                            else                                    //If none exist, then the longest path known to us has been found                                
                                            {
                                                finished = true;
                                            }
                                        }
                                    }
                                }
                                else                                                //If there are differing paths, then do this
                                {
                                    if (current.result == "")                       //Check if current is a starter proposition
                                    {
                                        knowledgeList.Add(current.percept);         //If yes, then remove current path and, add percept to string list
                                        paths.RemoveAt(0);

                                        if(paths.Count == 0)                        //If no more paths, than adding is completed
                                        {
                                            finished = true;
                                        }
                                    }
                                    else                                            //If still more paths, repeat what we did above. Check for & symbol, move down knowbase
                                    {
                                        knowledgeList.Add(current.result);
                                        if (current.percept.Contains("&"))
                                        {
                                            List<string> swap = new List<string>();
                                            foreach (string s in tempBase.percept.Split("&", StringSplitOptions.RemoveEmptyEntries))
                                            {
                                                swap.Add(s);
                                            }

                                            foreach (string symbols in swap)
                                            {
                                                paths.Add(knownBase.ToList().Find(k => (k.result == symbols) || (k.result == "" && k.percept == symbols)));
                                            }
                                            current = paths[0];
                                        }
                                        else
                                        {
                                            if (knownBase.Any(k => (k.result == current.percept) || (k.result == "" && k.percept == current.percept)))
                                            {
                                                current = knownBase.ToList().Find(k => (k.result == current.percept) || (k.result == "" && k.percept == current.percept));
                                            }                                       
                                            else
                                            {
                                                paths.RemoveAt(0);
                                                if(paths.Count == 0)
                                                {
                                                    finished = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if(multiplePropositions.Count == 0 && stack.Count == 0)         //Backup check, in case it finds a point where everything has been added correctly, and all paths are full implemented
                    {
                        Console.Write("YES: ");                                     //Repeats print sequence as seen above

                        knowledgeList = Enumerable.Reverse(knowledgeList).ToList();
                        HashSet<string> keys = new HashSet<string>();

                        for (int i = 0; i < knowledgeList.Count; i++)
                        {
                            if (i == knowledgeList.Count - 1)
                            {
                                if (!keys.Contains(knowledgeList[i]))
                                {
                                    Console.Write(knowledgeList[i]);
                                    keys.Add(knowledgeList[i]);
                                }
                            }
                            else
                            {
                                if (!keys.Contains(knowledgeList[i]))
                                {
                                    Console.Write(knowledgeList[i] + ", ");
                                    keys.Add(knowledgeList[i]);
                                }
                            }
                        }
                        Console.WriteLine();
                        return;
                    }
                }
                else                                                            //If no & symbol, do this
                {
                    for (int i = 0; i < KB.Count; i++)
                    {
                        if (!knownBase.Contains(KB[i]))                         //Loop through everything in KB, try to find the next possible node.
                        {
                            if ((KB[i].result == "") && (KB[i].percept == tempBase.percept))
                            {
                                stack.Push(KB[i]);
                            }
                            else if (KB[i].result == tempBase.percept)
                            {
                                stack.Push(KB[i]);
                            }
                        }
                    }
                }         
            }

            Console.WriteLine("Failed");                                        //If there is a node with no future connections, BC has failed
        }
    }
}
