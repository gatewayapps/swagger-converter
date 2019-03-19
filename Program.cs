using System;
using System.IO;
using CommandLine;


namespace SwaggerConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => {
                var finalSwaggerPath = Path.IsPathFullyQualified(o.SwaggerFile) ? o.SwaggerFile : Path.GetRelativePath(Environment.CurrentDirectory, o.SwaggerFile);
                if(File.Exists(finalSwaggerPath) == false){
                    Console.Error.WriteLine("File does not exist: " + o.SwaggerFile + ".  Resolved to: " + finalSwaggerPath);
                    return;
                }
                
                var sf = SwaggerFile.LoadSwaggerFile(finalSwaggerPath);
                
            });
        }
    }
}
