using System;
using System.IO;
using System.Linq;
using CommandLine;


namespace SwaggerConverter
{
  class Program
  {
    static void Main(string[] args)
    {

      Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
      {
        var finalSwaggerPath = Path.IsPathFullyQualified(o.SwaggerFile) ? o.SwaggerFile : Path.GetRelativePath(Environment.CurrentDirectory, o.SwaggerFile);
        var finalOutput = Path.IsPathFullyQualified(o.OutputPath) ? o.OutputPath : Path.GetRelativePath(Environment.CurrentDirectory, o.OutputPath);
        if (File.Exists(finalSwaggerPath) == false)
        {
          Console.Error.WriteLine("File does not exist: " + o.SwaggerFile + ".  Resolved to: " + finalSwaggerPath);
          return;
        }

        var sf = SwaggerFile.LoadSwaggerFile(finalSwaggerPath);
        var controllerRoutes = sf.Routes.GroupBy((x) => x.Controller);
        foreach (var group in controllerRoutes)
        {
          var controllerName = group.Key;
          ControllerWriter.WriteController(controllerName, group.ToList(), finalOutput);
        }
      });
    }
  }
}
