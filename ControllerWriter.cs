using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SwaggerConverter
{
  public class ControllerWriter
  {
    public static void WriteController(string controllerName, List<SwaggerFile.Route> routes, string outputPath)
    {
      var fileName = Path.Join(outputPath, controllerName + ".ts");
      if (File.Exists(fileName) == false)
      {
        Console.Error.WriteLine("Could not find " + fileName);
        return;
      }
      var output = new StringBuilder();
      var hasWrittenClassHeader = false;
      var className = controllerName.Substring(0, 1).ToUpper() + controllerName.Substring(1);
      using (var stream = File.OpenText(fileName))
      {
        output.AppendLine("import { HttpMethod, route } from '@decorators/routeDecorator'");
        output.AppendLine("import {hasPermission} from '@decorators/permissionDecorator'");
        output.AppendLine("import ControllerBase from './ControllerBase'");
        output.AppendLine("import { Request, Response } from 'express'");
        while (!stream.EndOfStream)
        {
          var currentLine = stream.ReadLine();
          if (currentLine.Contains("export default"))
          {
            var foundEnd = false;
            while (!foundEnd)
            {
              currentLine = stream.ReadLine();
              foundEnd = currentLine.Contains("}");
            }
            currentLine = stream.ReadLine();
          }
          if (!hasWrittenClassHeader && currentLine.Contains("function "))
          {
            output.AppendLine($"export class {className} {{");
            hasWrittenClassHeader = true;
          }
          if (currentLine.Contains(" function "))
          {
            foreach (var route in routes)
            {
              foreach (var action in route.Actions)
              {
                if (currentLine.Contains(action.ActionName))
                {
                  output.AppendLine($"@route(HttpMethod.{action.Method.ToUpper()}, '{route.Path}')");
                  if (action.Permissions.Count > 0)
                  {
                    output.AppendLine($"@hasPermission('{action.Permissions[0].Role}', '{action.Permissions[0].Action}')");
                  }
                }
              }
            }
          }

          output.AppendLine(currentLine
          .Replace("export function ", "public async ")
          .Replace("export async function ", "public async ")
          .Replace(" function ", " public async ")
          .Replace("req, res", "req: Request, res: Response"));
        }

      }
      output.AppendLine("}");
      output.AppendLine("");
      output.AppendLine($"export default new {className} ()");
      File.WriteAllText(fileName, output.ToString());


    }
  }
}