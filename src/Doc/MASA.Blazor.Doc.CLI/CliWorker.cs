﻿using MASA.Blazor.Doc.CLI.Commands;
using MASA.Blazor.Doc.CLI.Interfaces;
using MASA.Blazor.Doc.CLI.Wrappers;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor.Doc.CLI
{
    public class CliWorker
    {
        public int Execute(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = ConfigWrapper.Config.Name,
                FullName = ConfigWrapper.Config.FullName,
                Description = ConfigWrapper.Config.Descrption,
            };

            app.HelpOption();
            app.VersionOptionFromAssemblyAttributes(typeof(Program).Assembly);

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            new List<IAppCommand>()
            {
                new GenerateDemoJsonCommand(),
                new GenerateDocsToHtmlCommand(),
                new GenerateIconsToJsonCommand(),
                new GenerateMenuJsonCommand(),
                new GenerateApiJsonCommand(),
            }
            .ToList()
            .ForEach(cmd =>
            {
                app.Command(cmd.Name, cmd.Execute);
            });

            return app.Execute(args);
        }
    }
}
