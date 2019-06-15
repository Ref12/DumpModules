using Ref12.Diagnostics.Extraction;
using System;
using System.IO;

namespace DumpModules
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string dumpPath = @"D:\temp\UnhandledFailure.dmp";
            ModuleDumper.Run(dumpPath, Path.ChangeExtension(dumpPath, "modules"));

        }
    }
}
