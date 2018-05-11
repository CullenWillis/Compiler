using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser
{
    class Program
    {
        private static Manager manager;
        private static StreamReader file;

        static void Main(string[] args)
        {
            manager = new Manager();
            file = null;

            try
            {
                file = new StreamReader(args[0]);
            }
            catch (IOException e)
            {
                Console.Write(e);
            }

            manager.startCompiler(file);

            file.Close();
        }
    }
}
