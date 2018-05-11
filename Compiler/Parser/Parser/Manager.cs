using AllanMilne.Ardkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Parser
{
    class Manager
    {
        private MyParser parser;

        public Manager()
        {
            parser = new MyParser();
        }

        public void startCompiler(StreamReader file)
        {
            startParser(file);
        }

        private void startParser(StreamReader file)
        {
            try
            {
                parser.Parse(file);

                Console.Write("\n");
                foreach (CompilerError err in parser.Errors)
                {
                    Console.WriteLine(err);
                }

                Console.WriteLine("{0} errors found. \n", parser.Errors.Count);
            }
            catch (IOException e)
            {
                Console.WriteLine("IOException: " + e);
            }
        }
    }
}
