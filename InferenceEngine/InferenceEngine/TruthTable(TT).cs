using InferenceEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace InferenceEngine
{
    public partial class Program
    {
        static void TT(string line, List<KnowledgeBase> KB)
        {
            List<KnowledgeBase> changeKnowledge = new List<KnowledgeBase>();            //Create a new Knowledge Base List (changeKnowledge)

            for (int k = 0; k < KB.Count; k++)                                          //Add in the same values contained in our original Knowledge Base List (KB)
            {
                changeKnowledge.Add(KB[k]);
            }

            changeKnowledge = SetupKnowledgeBase(changeKnowledge);                      //Run SetupKnowledgeBase function to add in all unique variables
            List<KnowledgeBase> list = new List<KnowledgeBase>();                       //Create a new Knowledge Base List (list)


            for (int j  = 0; j < changeKnowledge.Count; j++)                            //Add in the same values contained in our changeKnowledge
            {
                list.Add(changeKnowledge[j]);
            }


            TT_Checker(list, line, changeKnowledge, KB);                                //Run TT_Checker to return the amount of times the sentence line entails the Knowledge Base KB
        }

        static void TT_Checker(List<KnowledgeBase> list, string line, List<KnowledgeBase> changeKnowledge, List<KnowledgeBase> KB)
        {
            KnowledgeBase knowBase = null;                                                                      //Create the current base as null

            List<List<Tuple<KnowledgeBase, bool>>> finalList = new List<List<Tuple<KnowledgeBase, bool>>>();    //Create two copies of a List of List of Tuples made up of a Knowledge Base and a Bool

            List<List<Tuple<KnowledgeBase, bool>>> tempList = new List<List<Tuple<KnowledgeBase, bool>>>();

            List<KnowledgeBase> negativeList = new List<KnowledgeBase>();                                       //Create a List used for our second set of searches

            for (int j = 0; j < list.Count; j++)                                                                //Make negative list a copy of list
            {
                negativeList.Add(list[j]);
            }


            for (int i = 0; i < list.Count; i++)                                                                //Loop through every element of list
            {
                if (list[i].result == "")                                                                       //The first time it finds a sentence without a result (E.G. just a;), copy it to knowBase and leave
                {
                    knowBase = list[i];
                    list.RemoveAt(i);
                    negativeList.RemoveAt(i);
                    break;
                }
            }

            if (knowBase != null)                                                                               //As long as a a sentence was found, continue
            {
                for (int i = 0; i < list.Count; i++)
                {
                    List<Tuple<KnowledgeBase, bool>> currentFound = new List<Tuple<KnowledgeBase, bool>>();     //Create a List of Tuples called currentFound
                    finalList = TTTrue(knowBase, list, changeKnowledge, currentFound, true, finalList, KB);     //Use the current found sentence (knowBase) to find all results where it starts as true. 

                    List<Tuple<KnowledgeBase, bool>> negativeFound = new List<Tuple<KnowledgeBase, bool>>();    //Create a list of tuples called negativeFound
                    tempList = TTTrue(knowBase, negativeList, changeKnowledge, negativeFound, false, tempList, KB); //Do the same as before, except with false and using negativeFound and negativeList instead

                    finalList.AddRange(tempList);                                                               //Combine list's together to create a finalList
                }
            }

            for (int j = 0; j < finalList.Count; j++)                                                           //loop through every element of finalList
            {
                if (finalList[j].Count > changeKnowledge.Count + 1)                                             //If the current element contains more lists of tuples then it is meant to, remove that element
                {
                    finalList.RemoveAt(j);                                                                      //While this isn't the best way to solve the bug, it does make it run correctly
                }
            }

            EvaluateLine(line, finalList);                                                                      //Do the final evaluation
        }

        static List<List<Tuple<KnowledgeBase, bool>>> TTTrue(KnowledgeBase knowBase, List<KnowledgeBase> removeBase, List<KnowledgeBase> changeKnowledge, List<Tuple<KnowledgeBase, bool>> currentFound, bool foundState, List<List<Tuple<KnowledgeBase, bool>>> finalList, List<KnowledgeBase> KB)
        {
            currentFound.Add(new Tuple<KnowledgeBase, bool> (knowBase, foundState));                            //Add the current knowBase to the Tuple List, bool is the found state.

            List<Tuple<KnowledgeBase, bool>> negativeFound = new List<Tuple<KnowledgeBase, bool>>();            //Create another list called negativeFound

            for (int i = 0; i < currentFound.Count; i++)                                                        //Make negativeFound a copy of the currentFound list
            {
                negativeFound.Add(currentFound[i]);
            }

            if (removeBase.Count > 0)                                                                           //If the List of Knowledge Bases till has objects, more paths are needed to follow
            {
                knowBase = null;                                                                                //Make knowBase null

                for (int i = 0; i < removeBase.Count; i++)
                {
                    if (removeBase[i].result == "")                                                             //Run through all elements in the current removeBase. If any results are ""
                    {
                        knowBase = removeBase[i];                                                               //Make knowBase equal to that current sentence, then remove it from removeBase
                        removeBase.RemoveAt(i);
                        break;                                                                                  //Break out of for loop
                    }
                }

                if (knowBase != null)                                                                           //If something was found above
                {
                    for (int i = 0; i < removeBase.Count; i++)                                                  //Run through every object still in removeBase
                    {
                        List<KnowledgeBase> tempBase = new List<KnowledgeBase>();                               //Create a temporary list of knowledgeBase

                        for (int j = 0; j < removeBase.Count; j++)
                        {
                            tempBase.Add(removeBase[j]);                                                        //for every object still in removeBase, add it to the tempBase
                        }

                        TTTrue(knowBase, removeBase, changeKnowledge, currentFound, true, finalList, KB);       //Run this again, where the state found is true

                        TTTrue(knowBase, tempBase, changeKnowledge, negativeFound, false, finalList, KB);       //Run this again, where the removeBase is tempBase and the found state is false
                    }
                }
                else                                                                                            //If nothing was found, then sentence needs to be found
                {
                    knowBase = removeBase[0];                                                                   //knowBase is equal to whatever node is at the start of removeBase, then remove it from removeBase
                    removeBase.RemoveAt(0);
                    bool solvedBool = false;                                                                    //create bool defaulted to false

                    if (!knowBase.percept.Contains("&"))                                                        //if the knowBase.percept does not contain "&", then continue
                    {
                        string solvePercept = knowBase.percept;                                                 //Create objects of both the knowBase perpect and result.
                        string solveResult = knowBase.result;

                        bool perceptBool = false;                                                               //Default both found bools to false
                        bool resultBool = false;

                        for (int i = 0; i < currentFound.Count; i++)                                            //For every Knowledge Base we have previously seen
                        {
                            if (currentFound[i].Item1.percept == solvePercept && currentFound[i].Item1.result == "")    //Find the Knowledge Base where it's percept is the same as Solve Percept, and its result is ""
                            {
                                perceptBool = currentFound[i].Item2;                                            //Copy it's bool                                          
                            }

                            if (currentFound[i].Item1.percept == solveResult && currentFound[i].Item1.result == "")     //Same as above, but for solveResult
                            {
                                resultBool = currentFound[i].Item2;
                            }
                        }

                        if ((perceptBool == resultBool) || (perceptBool == false && resultBool == true))        //Follow rules of implication (if both are the same, or if it's False => True, then it's true)
                        {
                            solvedBool = true;
                        }
                    }
                    else if(knowBase.percept.Contains("&"))                                                     //If it does contain "&", then more work needed
                    {
                        List<string> solvePerceptList = new List<string>();                                     //Create an empty percept list. Other variables same as above
                        string solveResult = knowBase.result;

                        bool perceptBool = true;                                                                //Set perceptBool to true, since defaulting to false causes it to always be false
                        bool resultBool = false;

                        foreach (string quary in knowBase.percept.Split("&", StringSplitOptions.RemoveEmptyEntries))        //Loop through all strings in percept, saving them to solvePerceptList
                        {
                            solvePerceptList.Add(quary);
                        }

                        for (int i = 0; i < currentFound.Count; i++)                                            //Same loop as above for currentFound
                        {
                            for(int j = 0; j < solvePerceptList.Count; j++)                                     //Check through all possible strings in solvePerceptList
                            {
                                if (currentFound[i].Item1.percept == solvePerceptList[j].Trim() && currentFound[i].Item1.result == "")      //if one of the objects in solvePerceptList matches the currentFound percept, and currentFound result is ""
                                {
                                    perceptBool = perceptBool && currentFound[i].Item2;                         //Make percept bool equal to a check to see if it is currently true and the found object is true. Since & needs all to be true, if one is false then it is always false.
                                }
                            }

                            if (currentFound[i].Item1.percept == solveResult && currentFound[i].Item1.result == "")     //Same as above for resultBool
                            {
                                resultBool = currentFound[i].Item2;
                            }
                        }

                        if((perceptBool == resultBool) || (perceptBool == false && resultBool == true))                 //Same as Above
                        {
                            solvedBool = true;
                        }
                    }

                    TTTrue(knowBase, removeBase, changeKnowledge, currentFound, solvedBool, finalList, KB);             //Repeat loop, where the state is whatever was found by the above loops.

                }
            }
            else
            {
                finalList.Add(TruthTableTuple(currentFound, KB));                                                       //if removeBase has no more possible paths, run TruthTableTuple to add the last Tuple (the combination of the truth table in it's current state)
            }

            return finalList;                                                                                           //Return finalList, this path has closed
        }

        static void EvaluateLine(string line, List<List<Tuple<KnowledgeBase, bool>>> finalList)                         //Ones all combinations of True and False have been found
        {
            int counter = 0;                                                                                            //Setup counter

            for (int i = 0; i < finalList.Count; i++)                                                                   //For every list of tuples comprised of a KnowledegeBase and bool contained in finalList
            {
                for (int j = 0; j < finalList[i].Count; j++)                                                            //For every Tuple found in that list
                {
                    if ((finalList[i][j].Item1.percept == line && finalList[i][j].Item1.result == "") && finalList[i][j].Item2)     //if the variable as a singular propisition was true
                    {
                        if (finalList[i][(finalList[i].Count - 1)].Item2 == true  && finalList[i][(finalList[i].Count - 1)].Item1.result == "TT")   //if the last added Tuple (combination tuple added from below) is true (making sure the tuple being checked is in fact "TT" for no possible buggy interactions)
                        {
                            counter++;                                                                                  //Entailment found
                        }
                    }
                }
            }

            if(counter > 0)                                                                                             //If at least one entailment was found, print that number
            {
                Console.WriteLine("YES: " + counter);
            }
            else
            {
                Console.WriteLine("NO");                                                                                //Otherwise, the line never entails the truthTable
            }
        }

        static List<KnowledgeBase> SetupKnowledgeBase(List<KnowledgeBase> kb)                                           //Add in all the unique symbol not currently contained
        {
            char[] delimiters = { '&', ' ' };                                                                           //Remove white spaces and "&", not needed for this function
            HashSet<string> keys = new HashSet<string>();                                                               //Create a HashSet of keys to make sure there is no double up. Means when we add strings we don't need to check if key's already contains the string

            for (int j = 0; j < kb.Count; j++)                                                                          //Go through every sentence in Knowledge Base
            {
                if (kb[j].percept.Contains("&"))                                                                        //If percept contains "&"
                {
                    keys.Add(kb[j].result.Trim());                                                                      //Add a trimmed Result

                    List<string> split = new List<string>();
                    foreach (string quaries in kb[j].percept.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))  //For every letter in the percept, add it to a list of strings called split
                    {
                        split.Add(quaries.Trim());
                    }

                    for (int k = 0; k < split.Count; k++)
                    {
                        keys.Add(split[k]);                                                                             //for every found letter in split, add it to keys
                    }
                }
                else                                                                                                    //If no "&" symbol
                {
                    if (kb[j].result != "")                                                                             //As long as result is not "", add it to keys
                    {
                        keys.Add(kb[j].result.Trim());
                    }
                    keys.Add(kb[j].percept.Trim());                                                                     //Add percept to keys
                }
            }

            List<string> tempKeys = keys.ToList();                                                                      //Convert HashSet to List, and put it into tempKeys

            for (int i = 0; i < kb.Count; i++)                                                                          //Going through every sentence in our Knowledge Base
            {
                if (tempKeys.Contains(kb[i].percept.Trim()) && kb[i].result == "")                                      //If tempKeys contains the current sentences percept, and the current sentence's result is ""
                {
                    tempKeys.Remove(kb[i].percept);                                                                     //Remove the current sentence from tempKeys, since it's already in the Knowledge Base as a unique variable
                }
            }

            foreach (string value in tempKeys)                                                                          //For all strings still in tempKeys
            {
                kb.Add(new KnowledgeBase(value, ""));                                                                   //Add a know sentence to the Knowledge Base, where it's percept is the tempKeys string, and result is ""
            }

            return kb;                                                                                                  //Return altered Knowledge Base
        }

        static List<Tuple<KnowledgeBase, bool>> TruthTableTuple(List<Tuple<KnowledgeBase, bool>> currentFound, List<KnowledgeBase> KB)  //Create the Truth Table combination Tuple
        {
            string problems = "";                                                                   //Create empty string problems
            List<Tuple<KnowledgeBase, bool>> finalTuple = new List<Tuple<KnowledgeBase, bool>>();   //Create a new List of Tuples called finalTuple

            for (int i = 0; i < currentFound.Count; i++)                                            //Loop through all found Tuples
            {
                if (KB.Contains(currentFound[i].Item1))                                             //As long as the Knowledge Base exists in the orginial Knowledge Base (The unchanged one), add it to finalTuple
                {
                    finalTuple.Add(currentFound[i]);
                }
            }

            bool boolResult = finalTuple.All(x => x.Item2);                                         //Checks if all Tuples result's are true, only looking at the orginal truth table symbols, all linked with "&" symbol

            List<Tuple<KnowledgeBase, bool>> result = new List<Tuple<KnowledgeBase, bool>>(currentFound);   
            result.Add(new Tuple<KnowledgeBase, bool>(new KnowledgeBase(problems, "TT"), boolResult));      //Create the final result. Make it's percept "" and the result "". It's state is true when eveything in the truth table is true. Else it's false

            return result;                                                                          //Return found Result
        }

    }
}
