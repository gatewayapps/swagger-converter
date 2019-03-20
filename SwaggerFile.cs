using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SwaggerConverter
{
  public class SwaggerFile
  {


    public class SecurityDefinition
    {
      public string Name { get; set; }
      public string HeaderKey { get; set; }
    }

    public class Permission
    {
      public string Role { get; set; }
      public string Action { get; set; }

    }

    public class Parameter
    {
      public String Name { get; set; }
      public Boolean Required { get; set; }
      public String In { get; set; }
    }

    public class Route
    {
      public String Path { get; set; }

      public List<Action> Actions { get; set; } = new List<Action>();

      public String Controller { get; set; }



      public Route(string Path, string Controller)
      {
        this.Path = Path;

        this.Controller = Controller;
      }
    }

    public class Action
    {
      public string Method { get; set; }
      public string ActionName { get; set; }
      public String SecurityMethod { get; set; }
      public List<Permission> Permissions { get; set; } = new List<Permission>();

      public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }

    public List<SecurityDefinition> SecurityDefinitions { get; set; } = new List<SecurityDefinition>();
    public List<Route> Routes { get; set; } = new List<Route>();


    public static SwaggerFile LoadSwaggerFile(String swaggerFile)
    {
      var sf = new SwaggerFile();

      var knownMethods = new String[] { "get", "post", "head", "patch", "delete", "put" };

      using (StreamReader reader = File.OpenText(swaggerFile))
      {
        JObject swagger = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
        JObject securityDefinitions = (JObject)swagger["securityDefinitions"];
        JObject paths = (JObject)swagger["paths"];

        var basePath = (string)swagger["basePath"];
        if (basePath == null)
        {
          basePath = "/";
        }

        foreach (var s in securityDefinitions)
        {
          sf.SecurityDefinitions.Add(new SecurityDefinition()
          {
            Name = s.Key,
            HeaderKey = (string)s.Value["name"]
          });
        }

        foreach (var p in paths)
        {
          var finalPath = Path.Join(basePath, p.Key);
          finalPath = Regex.Replace(finalPath, @"{(\w+)}", ":$1");

          var controller = (string)p.Value["x-swagger-router-controller"];
          var route = new Route(finalPath, controller);


          foreach (string m in knownMethods)
          {
            JObject a = (JObject)p.Value[m];
            if (a != null)
            {
              var action = new Action() { Method = m, ActionName = (string)a.GetValue("operationId") };
              var securityOptions = (JArray)a.GetValue("security");
              var requiredPermissions = (JArray)a.GetValue("x-required-permissions");
              var parameters = (JArray)a.GetValue("parameters");
              if (securityOptions != null && securityOptions.Count > 0)
              {
                var so = securityOptions[0];
                var vals = so.ToArray();
                foreach (JProperty s in vals)
                {
                  action.SecurityMethod = s.Name;



                }

              }

              if (requiredPermissions != null && requiredPermissions.Count > 0)
              {
                var perms = requiredPermissions.ToArray();
                foreach (JObject perm in perms)
                {
                  var role = (string)perm.GetValue("role");
                  var permission = (string)perm.GetValue("action");

                  action.Permissions.Add(new Permission() { Action = permission, Role = role });
                }
              }

              if (parameters != null && parameters.Count > 0)
              {
                foreach (JObject par in parameters)
                {
                  action.Parameters.Add(new Parameter() { In = (string)par.GetValue("in"), Name = (string)par.GetValue("name"), Required = (bool)par.GetValue("required") });
                }
              }


              route.Actions.Add(action);

            }
          }
          sf.Routes.Add(route);

        }

      }


      return sf;
    }
  }
}
