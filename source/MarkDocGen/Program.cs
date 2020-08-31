using CommandLine;
using DefaultDocumentation.Model;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MarkDocGen
{
   // TODO PP (2020-08-25): Handle compiler generated members, and accessibility configuration (i.e. only export public members etc)
   class Options
   {
      [Value(0, MetaName = "<input files>", HelpText = "The assemblies to process", Required = true)]
      public IEnumerable<string> InputFiles { get; set; }

      [Option('o', "output-directory", HelpText = "The directory in which to place output files. Note that all files in this directory will be deleted before new files are generated.", Required = true, MetaValue = "dir")]
      public string OutputDirectory { get; set; }
      
      [Option("sidebar-link-prefix", HelpText = "An optional prefix to add in front of all links placed in the Sidebar.json file.", Required = false)]
      public string SideBarLinkPrefix { get; set; }
   }

   class Program
   {
      static void HandleParseError(IEnumerable<Error> errs)
      {       
      }

      static void Run(Options opts)
      {
         // TODO PP (2020-08-31): Modify logger format to be easily parseable by build environment, i.e. lines starting with "Error:", "Warning:" etc.
         var logger = new LoggerConfiguration()
            .WriteTo.Console().MinimumLevel.Is(Serilog.Events.LogEventLevel.Information).CreateLogger();

         Microsoft.Extensions.Logging.ILogger msLog = new SerilogLoggerProvider(logger).CreateLogger("log");
         DocProject project = new DocProject(msLog);

         msLog.LogInformation("Using output directory {Directory}", opts.OutputDirectory);

         bool hasErrors = false;
         foreach (var file in opts.InputFiles)
         {
            if (!File.Exists(file))
            {
               hasErrors = true;
               msLog.LogError("The specified file \"{File}\" does not exist.", file);
            }

            var docFile = Path.ChangeExtension(file, ".xml");

            if (!File.Exists(file))
            {
               hasErrors = true;
               msLog.LogError("The documentation file \"{File}\" does not exist.", file);
            }

            project.Add(file, docFile);
         }

         if (hasErrors)
            return;

         if (Directory.Exists(opts.OutputDirectory))
         {
            msLog.LogInformation("Cleaning output directory \"{Directory}\".", opts.OutputDirectory);
            Empty(new DirectoryInfo(opts.OutputDirectory));
         }
         else
         {
            msLog.LogDebug("Creating directory {Directory}", opts.OutputDirectory);
            Directory.CreateDirectory(opts.OutputDirectory);
         }

         DefaultLinkResolver resolver = new DefaultLinkResolver();
         IFileNameStrategy fileNameStrategy = DefaultFileNameStrategy.Instance;

         DocusaurusTemplate template = new DocusaurusTemplate(fileNameStrategy);
         DocumentationGenerator generator = new DocumentationGenerator(DefaultFileNameStrategy.Instance, resolver, msLog);

         generator.Generate(project, template, opts.OutputDirectory);

         DocusaurusSideBarTemplate sbt = new DocusaurusSideBarTemplate(template, opts.SideBarLinkPrefix);
         generator.Generate(project, sbt, opts.OutputDirectory);
      }

      static void Main(string[] args)
      {
         var parser = Parser.Default;
         var res = parser.ParseArguments<Options>(args)
            .WithParsed(Run)
            .WithNotParsed(HandleParseError);
                        
      }

      public static void Empty(DirectoryInfo directory)
      {         
         foreach (FileInfo file in directory.GetFiles()) 
            file.Delete();

         foreach (DirectoryInfo subDirectory in directory.GetDirectories()) 
            subDirectory.Delete(true);
      }
   }

}
