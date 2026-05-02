using MyApp.Management.Commands;
using System.CommandLine;

var rootCommand = new RootCommand("開発者・運用担当者向け管理コマンド");

rootCommand.Subcommands.Add(GenerateGeneralLookupViewsSqlCommand.Create());

return rootCommand.Parse(args).Invoke();
