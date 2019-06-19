using Microsoft.Diagnostics.Runtime;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.SymbolStore;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ref12.Diagnostics.Extraction
{
    public class ModuleDumper
    {
        public static void Run(string dumpPath, string modulesDirectory)
        {
            Directory.CreateDirectory(modulesDirectory);

            DataTarget target = DataTarget.LoadCrashDump(dumpPath);
            var dacLocation = target.ClrVersions[0];
            ClrRuntime runtime = dacLocation.CreateRuntime();

            var resolver = new DumpAssemblyResolver(target.DataReader, runtime.AppDomains[0].Modules);

            foreach (var module in runtime.AppDomains[0].Modules)
            {
                if (string.IsNullOrEmpty(module.FileName) || module.MetadataAddress == 0)
                {
                    continue;
                }

                AssemblyDefinition assembly = resolver.Resolve(Path.GetFileNameWithoutExtension(module.Name));

                assembly.MainModule.Attributes |= ModuleAttributes.ILOnly;

                assembly.Write(Path.Combine(modulesDirectory, Path.GetFileName(module.FileName)));
            }
        }

        public class DumpAssemblyResolver : IAssemblyResolver
        {
            public Dictionary<string, ClrModule> ModuleMap = new Dictionary<string, ClrModule>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, AssemblyDefinition> AssemblyMap = new Dictionary<string, AssemblyDefinition>(StringComparer.OrdinalIgnoreCase);
            private readonly IDataReader dataReader;

            public DumpAssemblyResolver(IDataReader dataReader, IEnumerable<ClrModule> modules)
            {
                this.dataReader = dataReader;

                foreach (var module in modules)
                {
                    if (!string.IsNullOrEmpty(module.AssemblyName) && module.MetadataAddress != 0)
                    {
                        var name = Path.GetFileNameWithoutExtension(module.AssemblyName);
                        ModuleMap[name] = module;
                    }
                }
            }

            public void Dispose()
            {
            }

            public AssemblyDefinition Resolve(string name)
            {
                if (!AssemblyMap.TryGetValue(name, out var definition))
                {
                    if (ModuleMap.TryGetValue(name, out var module))
                    {
                        definition = AssemblyDefinition.ReadAssembly(
                            new ReadVirtualStream(
                                dataReader,
                                (long)module.ImageBase,
                                int.MaxValue),
                            new ReaderParameters(ReadingMode.Deferred)
                            {
                                AssemblyResolver = this
                            });

                        AssemblyMap[name] = definition;
                    }
                }

                return definition;
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name.Name);
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                return Resolve(name.Name);
            }
        }

        public class VirtualStreamModuleReader : StreamModuleReader, IModuleReader
        {
            public VirtualStreamModuleReader(Stream peStream) : base(peStream)
            {
            }

            PEStreamFormat IModuleReader.Format => PEStreamFormat.MemoryLayoutFormat;
        }
    }
}
