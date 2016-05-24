#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeClassifier
{
    public class Parser
    {
        private readonly string[] _source;
        private readonly string _sourceString;

        public Parser(TextReader stream)
        {
            _sourceString = stream.ReadToEnd();
            _source = _sourceString.Split('\n');
        }

        private Dictionary<string, KeyValuePair<int, int>> ListVariables()
        {
            var dict = new Dictionary<string, List<string>>();
            var syntaxTree = CSharpSyntaxTree.ParseText(_sourceString);


            var fieldList =
                syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<FieldDeclarationSyntax>().ToArray();

            dict.Add("private fields", (from fieldDeclarationSyntax in fieldList
                                        where fieldDeclarationSyntax.Modifiers.Any(p => p.ToString() == "private")
                                        from name in
                                            fieldDeclarationSyntax.Declaration.Variables.Select(
                                                variableDeclaratorSyntax => variableDeclaratorSyntax.Identifier.ToString()).ToList()
                                        select name).ToList());
            dict.Add("public fields", (from fieldDeclarationSyntax in fieldList
                                       where fieldDeclarationSyntax.Modifiers.Any(p => p.ToString() == "public")
                                       from name in
                                           fieldDeclarationSyntax.Declaration.Variables.Select(
                                               variableDeclaratorSyntax => variableDeclaratorSyntax.Identifier.ToString()).ToList()
                                       select name).ToList());
            dict.Add("protected fields", (from fieldDeclarationSyntax in fieldList
                                          where fieldDeclarationSyntax.Modifiers.Any(p => p.ToString() == "protected")
                                          from name in
                                              fieldDeclarationSyntax.Declaration.Variables.Select(
                                                  variableDeclaratorSyntax => variableDeclaratorSyntax.Identifier.ToString()).ToList()
                                          select name).ToList());


            var methodList =
                syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>().ToArray();

            dict.Add("public methods", (from methodDeclarationSyntax in methodList
                                        where
                                            (methodDeclarationSyntax.Modifiers.Any(p => p.ToString() == "public") &&
                                             methodDeclarationSyntax.Identifier.ToString() != string.Empty)
                                        select methodDeclarationSyntax.Identifier.ToString()).ToList());

            dict.Add("private methods", (from methodDeclarationSyntax in methodList
                                         where
                                             (methodDeclarationSyntax.Modifiers.Any(p => p.ToString() == "private") &&
                                              methodDeclarationSyntax.Identifier.ToString() != string.Empty)
                                         select methodDeclarationSyntax.Identifier.ToString()).ToList());

            dict.Add("protected methods", (from methodDeclarationSyntax in methodList
                                           where
                                               (methodDeclarationSyntax.Modifiers.Any(p => p.ToString() == "protected") &&
                                                methodDeclarationSyntax.Identifier.ToString() != string.Empty)
                                           select methodDeclarationSyntax.Identifier.ToString()).ToList());

            dict.Add("method parameters", (from param in
                (from methodDeclarationSyntax in methodList
                 select methodDeclarationSyntax.ParameterList.Parameters).Where(p => p.Any())
                                           from parameterSyntax in param
                                           where parameterSyntax.Identifier.ToString() != string.Empty
                                           select parameterSyntax.Identifier.ToString()).ToList());


            dict.Add("classes",
                (from classDeclarationSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                 where classDeclarationSyntax.Identifier.ToString() != string.Empty
                 select classDeclarationSyntax.Identifier.ToString()).ToList());


            dict.Add("local variables", (from varName in
                (from variableDeclarationSyntax in
                    syntaxTree.GetRoot().DescendantNodes().OfType<VariableDeclarationSyntax>()
                 select variableDeclarationSyntax.Variables)
                                         from variableDeclaratorSyntax
                                             in varName
                                         select variableDeclaratorSyntax.Identifier.ToString()).ToList());


            dict.Add("properties",
                (from propertyDeclarationSyntax in
                    syntaxTree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>()
                 select propertyDeclarationSyntax.Identifier.ToString()).ToList());


            try
            {
                return dict.ToDictionary(dictElem => dictElem.Key,
                    dictElem =>
                        new KeyValuePair<int, int>(dictElem.Value.Count(s => char.IsUpper(s[0])),
                            dictElem.Value.Count - dictElem.Value.Count(s => char.IsUpper(s[0]))));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private KeyValuePair<int, int> CountBrackets()
        {
            int numJava = 0, numBrackets = 0;
            foreach (var s in _source)
            {
                var bracketFound = false;

                for (var i = s.Length - 1; i >= 0; i--)
                {
                    if (char.IsWhiteSpace(s[i]))
                    {
                        continue;
                    }
                    if (!bracketFound)
                    {
                        if (s[i] == '{')
                        {
                            numBrackets++;
                            bracketFound = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        numJava++;
                        break;
                    }
                }
            }
            return new KeyValuePair<int, int>(numJava, numBrackets - numJava);
        }

        public Dictionary<string, KeyValuePair<int, int>> Parse()
        {
            var parsedResult = ListVariables();
            parsedResult.Add("brackets", CountBrackets());
            return parsedResult;
        }
    }
}