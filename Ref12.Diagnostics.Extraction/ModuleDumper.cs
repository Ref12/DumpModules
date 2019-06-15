using Microsoft.Diagnostics.Runtime;
using Mono.Cecil;
using System;
using System.IO;

namespace Ref12.Diagnostics.Extraction
{
    public class ModuleDumper
    {
        public static void Run(string dumpPath, string modulesDirectory)
        {
            DataTarget target = DataTarget.LoadCrashDump(dumpPath);
            var dacLocation = target.ClrVersions[0];
            ClrRuntime runtime = dacLocation.CreateRuntime();

            foreach (var module in runtime.AppDomains[0].Modules)
            {
                if (string.IsNullOrEmpty(module.FileName))
                {
                    continue;
                }

                //AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                //    new ReadVirtualStream(
                //        target.DataReader,
                //        (long)module.ImageBase,
                //        int.MaxValue));

                //var types = assembly.MainModule.Types;

                //ulong offset = 3984;
                //var size = module.MetadataLength + (module.MetadataAddress - module.ImageBase);

                //var moduleBytes = new byte[size];
                //if (target.DataReader.ReadMemory(module.ImageBase, moduleBytes, moduleBytes.Length, out var bytesRead))
                //{
                //    File.WriteAllBytes(Path.Combine(managedModulesDirectory, Path.GetFileName(module.FileName)), moduleBytes);
                //}
                //else
                //{

                //}
            }

            Directory.CreateDirectory(modulesDirectory);

            foreach (var module in target.EnumerateModules())
            {
                var moduleBytes = new byte[module.FileSize];
                if (target.DataReader.ReadMemory(module.ImageBase, moduleBytes, moduleBytes.Length, out var bytesRead))
                {
                    File.WriteAllBytes(Path.Combine(modulesDirectory, Path.GetFileName(module.FileName)), moduleBytes);
                }
                else
                {

                }
            }
        }
    }
}
