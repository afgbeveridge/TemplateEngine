using Infra.BTA.Templates;
using System;
using System.Collections.Generic;
using System.IO;

namespace TemplatesTest {

    public class Program {

        private static string TemplateContent { get; set; }

        public static void Main(string[] args) {
            ExecutableObjectFactory.RegisterFromSelf();
            DumpUnParsedFile();
            GenerateTemplatedContent();
            GenerateTemplatedContentWithEscaping();
            GenerateTemplatedContentWithContexts();
            GenerateTemplatedContentWithDefaultNamedContexts();
            Console.ReadLine();
        }

        private static void GenerateTemplatedContent() {
            ParseResult res = new Parser().Parse(new ParseContext(TemplateContent));
            EvaluationContext ec = new EvaluationContext(Client);
            ExecutionContext ctx = new ExecutionContext(new StringWriter(), ec);
            res.Execute(ctx);
            Console.WriteLine("******************** Substituted content");
            Console.WriteLine(ctx.ToString());
        }

        private static void GenerateTemplatedContentWithContexts() {
            ParseResult res = new Parser().Parse(new ParseContext(File.ReadAllText("Test-1.txt")));
            EvaluationContext ec = new EvaluationContext(Client, "client");
            ec.AddNamedContext(new CompanyDetails { Address = "10, The Lane, Somewhere", RegisteredName = "ABC Ltd" }, "company");
            ExecutionContext ctx = new ExecutionContext(new StringWriter(), ec);
            res.Execute(ctx);
            Console.WriteLine("******************** Substituted content");
            Console.WriteLine(ctx.ToString());
        }

        private static void GenerateTemplatedContentWithDefaultNamedContexts() {
            ParseResult res = Parser.Parse(File.ReadAllText("Test-2.txt"));
            EvaluationContext ec = EvaluationContext.From(Client, new CompanyDetails {
                Address = "10, The Lane, Somewhere",
                RegisteredName = "ABC Ltd"
            });
            var ctx = res.Execute(ExecutionContext.Build(ec));
            Console.WriteLine("******************** Substituted content in ms " + ctx.elapsed);
            Console.WriteLine(ctx.context.ToString());
            Console.WriteLine("******************** Substituted content in ms " + ctx.elapsed);
        }

        private static void GenerateTemplatedContentWithEscaping() {
            var escapedPattern = ParseContext.GenerateEscapedPattern('@');
            ParseResult res = Parser.Parse(TemplateContent, escapedPattern);
            Console.WriteLine("******************** Substituted content");
            var ctx = res.Execute(ExecutionContext.Build(Client)).context;
            Console.WriteLine(ctx.ToString());
        }

        private static ClientContainer Client {
            get {
                return new ClientContainer {
                    Goals = new[] { "Walk", "Then Run" },
                    UserName = "XX2",
                    Exercises = new[] { new Exercise { Region = "Neck" }, new Exercise { Region = "Back" } },
                    Client = new Client {
                        FirstName = "Toe"
                    }
                };
            }
        }

        private static void DumpUnParsedFile() {
            TemplateContent = File.ReadAllText("Test-0.txt");
            Console.WriteLine("******************** Unparsed content");
            Console.WriteLine(TemplateContent);
        }

    }

    public class ClientContainer {
        public string UserName { get; set; }
        public IEnumerable<string> Goals { get; set; }

        public IEnumerable<Exercise> Exercises { get; set; }

        public Client Client { get; set; }
    }

    public class Client {
        public string FirstName { get; set; }
    }

    public class Exercise {
        public string Region { get; set; }
    }

    public class CompanyDetails {
        public string Address { get; set; }
        public string RegisteredName { get; set; }
    }

    public class Container {
        public int Value { get; set; }
        public String StringValue { get; set; }
        public bool BooleanValue { get; set; }
        public string[] Goals { get; set; }
        public Child X { get; set; }
    }

    public class Child {
        public GrandChild Y { get; set; }
    }

    public class GrandChild {
        public string Z { get; set; }
    }

}
