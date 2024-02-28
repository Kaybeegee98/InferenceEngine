using System.Data;

namespace InferenceEngine
{
    public partial class Program
    {
        static void Main(string[] args)
        {
            List<KnowledgeBase> knowledgebase = new List<KnowledgeBase>();        //List of 2 strings, first being the percept of the world, and second being the result
            string ask = "";

            bool requireTell = false;
            bool requireAsk = false;
            int t = 0;

            foreach (string line in File.ReadLines(args[0]))                     //Loop through every string in the File
            {
                switch (t)
                {
                    case 0:
                        if (line.Contains("TELL"))                              //First line should always be TELL
                        {
                            requireTell = true;                                 //If it was, then tell is true
                        }

                        t++;
                        break;

                    case 1:
                        if (requireTell)                                        //As long as first line was Tell, continue
                        {
                            knowledgebase = KBTell(line);                       //Call KBTell to create the Knowledge Base
                        }
                        t++;
                        break;
                    case 2:
                        if (line.Contains("ASK"))                               //Third line should always be ASK
                        {
                            requireAsk = true;                                  //If it was, then ask is true
                        }
                        t++;
                        break;

                    case 3:
                        if (requireAsk)                                         //As long as third line was ASK, continue
                        {
                            ask = line.Trim();                                  //Trim the current line, and parse it into ask string
                        }
                        t++;
                        break;

                    default:
                        break;
                }
            }
            string command = args[1];

            switch(command.ToLower())                                           //Take command (second arg) in as lower, to ensure caps doesn't matter. Run function associated with command
            {
                case "truthtable" :                                             //Create cases for different style's of spelling Truth Table
                case "tt":
                case "truth table":
                    TT(ask, knowledgebase);                                     //Run Truth Table            
                    break;
                case "forwardchaining":                                         //Create cases for different style's of spelling Forward Chaining
                case "fc":
                case "forward chaining":
                    FC(ask, knowledgebase);                                     //Run Forward Chaining
                    break;
                case "backwardchaining":                                        //Create cases for different style's of spelling Backward Chaining
                case "bc":
                case "backward chaining":
                    BC(ask, knowledgebase);                                     //Run Backward Chaining
                    break;
                default:
                    throw new Exception();                                      //If no valid command, throw Exception
            }
        }
    }
}