using System;
using CommandLine;

namespace SwaggerConverter {
  public class Options {
    [Option('s', "swagger", Required=true, HelpText = "Path to swagger file")]
    public String SwaggerFile {get; set;}

    [Option('o', "output", Required=true, HelpText = "Output directory for TypeScript files")]
    public String OutputPath {get;set;}
  }
}