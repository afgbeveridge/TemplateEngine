using System;
using Infra.BTA.Templates;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.BTA.Tests {

    [TestClass]
    public class BasicTests {

        [ClassInitialize]
        public static void Init(TestContext ctx) {
            ExecutableObjectFactory.RegisterFromSelf();
        }

        [TestMethod]
        public void Given_ConfiguredParser_When_GivenNumericComparison_ShouldFollowEarlyPath() {
            ExecuteNumericComparison(8, "Less");
        }

        [TestMethod]
        public void Given_ConfiguredParser_When_GivenNumericComparison_ShouldFollowAlternatePath() {
            ExecuteNumericComparison(10, "Greater");
        }

        private void ExecuteNumericComparison(int val, string expect) {
            IExecutableObject exe = new Parser().Parse(new ParseContext("  [if Value > 9] Greater than [else] Less than [end] ")).Executable;

            var c = new Container {
                Value = val
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exe.Execute(ec);
            Assert.IsTrue(ec.ToString().IndexOf(expect) >= 0);
        }

        [TestMethod]
        public void Given_ConfiguredParser_When_GivenSimpleIteration_ShouldWork() {

            IExecutableObject exe = new Parser().Parse(new ParseContext("  [foreach Goals] [_] [end] ")).Executable;

            var c = new Container {
                Goals = new[] { "a", "b" }
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exe.Execute(ec);
            Assert.IsTrue(c.Goals.All(s => ec.ToString().IndexOf(s) >= 0));
        }

        [TestMethod]
        public void Given_ConfiguredParser_When_GivenMissingScopeClosure_ShouldFail() {

            ParseResult res = new Parser().Parse(new ParseContext("  [if Value > 9] Not greater than [else] Greater than"));
            Assert.IsFalse(res.Success);
            Assert.IsTrue(res.RecordedErrors.Any());
        }

        [TestMethod]
        public void Given_ConfiguredParser_When_GivenGarbage_ShouldFail() {

            ParseResult res = new Parser().Parse(new ParseContext("  [zif Value > 9] Not greater than [else] Greater than") { PerfectMatching = true });
            Assert.IsFalse(res.Success);
            Assert.IsTrue(res.RecordedErrors.Any());
        }

        private static IEnumerable<string> BasicContent = new[] {
                "Dear Toe,<p>",
                "<li>Walk</li>",
                "<li>Then Run</li>",
                "<li>Neck</li>",
                "<li>Back</li>",
                "your user name is XX2,"
            };

        [TestMethod]
        public void Given_ValidParsedExecutable_When_SuppliedContext_Then_SubstitutionsMustBeApplied() {
            IEnumerable<string> expected = new[] { "Found a neck region" }.Concat(BasicContent);
            ParseResult res = Parser.Parse(File.ReadAllText("Test-0.txt"));
            EvaluationContext ec = EvaluationContext.From(Client);
            ExecuteAndExpect(res, ec, expected);
        }

        [TestMethod]
        public void Given_ValidParsedExecutable_When_SuppliedSwitchableContexts_Then_SubstitutionsMustBeApplied() {
            ParseResult res = Parser.Parse(File.ReadAllText("Test-2.txt"));
            EvaluationContext ec = EvaluationContext.From(Client, new CompanyDetails {
                Address = "10, The Lane, Somewhere",
                RegisteredName = "ABC Ltd"
            });
            IEnumerable<string> expected = new[] {
                "prescription from ABC Ltd",
                "10, The Lane,"
            }.Concat(BasicContent);
            ExecuteAndExpect(res, ec, expected);
        }

        [TestMethod]
        public void Given_ValidParsedExecutable_When_SuppliedNamedSwitchableContexts_Then_SubstitutionsMustBeApplied() {
            ParseResult res = Parser.Parse("From client [Client.FirstName], [context company] at [Address][context client], user name is [UserName]");
            EvaluationContext ec = new
                EvaluationContext(Client, "client")
                .AddNamedContext(new CompanyDetails {
                    Address = "10, The Lane, Somewhere",
                    RegisteredName = "ABC Ltd"
                }, "company");
            IEnumerable<string> expected = new[] {
                "Toe",
                "10, The Lane,",
                "XX2"
            };
            ExecuteAndExpect(res, ec, expected);
        }

        [TestMethod]
        public void Given_ValidParsedExecutable_When_UsingEscapedScanPattern_Then_PreserveEscapedSequences() {
            var escapedPattern = ParseContext.GenerateEscapedPattern('@');
            ParseResult res = Parser.Parse("Hello, [Client.FirstName], this is a weird sequence @[Address]@, but your user name is still [UserName] ", escapedPattern);
            EvaluationContext ec = EvaluationContext.From(Client);
            IEnumerable<string> expected = new[] {
                "Toe",
                "@[Address]@",
                "XX2"
            };
            ExecuteAndExpect(res, ec, expected);
        }

        private void ExecuteAndExpect(ParseResult res, EvaluationContext ec, IEnumerable<string> expected) {
            var ctx = res.Execute(ExecutionContext.Build(ec)).context;
            var doc = ctx.ToString();
            Assert.IsTrue(expected.All(s => doc.IndexOf(s) >= 0));
        }

        [TestMethod]
        public void Given_ContextObject_When_ValidPathIsSpecified_Then_ShouldNotifySo() {
            var obj = new ContextObject(new Container());
            Assert.IsTrue(obj.CanNavigate("X.Y.Z"));
        }

        [TestMethod]
        public void Given_ContextObject_When_NonExistentPathIsSpecified_Then_ShouldNotifySo() {
            var obj = new ContextObject(new Container());
            Assert.IsFalse(obj.CanNavigate("X.y.Z"));
        }

        [TestMethod]
        public void Given_ContextObject_When_NullObjectInPathExists_Then_ShouldResultInNull() {
            var obj = new ContextObject(new Container { X = new Child { } });
            Assert.IsNull(obj.GetObject("X.Y.Z"));
        }

        [TestMethod]
        public void Given_ContextObject_When_NoNullObjectInPathExists_Then_ShouldResultInValue() {
            var obj = new ContextObject(new Container { X = new Child { Y = new GrandChild { Z = "endofchain" } } });
            Assert.AreSame(obj.GetObject("X.Y.Z"), "endofchain");
        }

        [TestMethod]
        public void Given_ValidNumericGreaterThanTest_When_Executed_ShouldNotFail() {
            IExecutableObject exe = new Parser().Parse(new ParseContext("  [if 7 > 9]  ")).Executable;

            var c = new Container {
                Value = 10
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exe.Execute(ec);

            Console.WriteLine(((StringWriter)ec.Sink).ToString());
        }

        [TestMethod]
        public void Given_ValidNumericLessThanTest_When_Executed_ShouldNotFail() {

            var cur = "if Value < 10";
            var exeObject = ExecutableObjectFactory.Realize(cur, cur.Split(' '));
            exeObject.Content = cur;
            var c = new Container {
                Value = 10
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exeObject.Execute(ec);

        }

        [TestMethod]
        public void Given_ValidFalseEqualBooleanTest_When_Executed_ShouldNotFail() {

            var cur = "if false == BooleanValue";
            var exeObject = ExecutableObjectFactory.Realize(cur, cur.Split(' '));
            exeObject.Content = cur;
            var c = new Container {
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exeObject.Execute(ec);

        }

        [TestMethod]
        public void Given_ValidBooleanEqualFalseTest_When_Executed_ShouldNotFail() {

            var cur = "if BooleanValue == false";
            var exeObject = ExecutableObjectFactory.Realize(cur, cur.Split(' '));
            exeObject.Content = cur;
            var c = new Container {
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exeObject.Execute(ec);

        }

        [TestMethod]
        public void Given_ValidStringEqualNullTest_When_Executed_ShouldNotFail() {

            var cur = "if StringValue == \"Test\"";
            var exeObject = ExecutableObjectFactory.Realize(cur, cur.Split(' '));
            exeObject.Content = cur;
            var c = new Container {
                StringValue = "Test"
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exeObject.Execute(ec);

        }

        [TestMethod]
        public void Given_ValidStringNotEqualTest_When_Executed_ShouldNotFail() {

            var cur = "if StringValue != \"Test\"";
            var exeObject = ExecutableObjectFactory.Realize(cur, cur.Split(' '));
            exeObject.Content = cur;
            var c = new Container {
                StringValue = "Test"
            };

            ExecutionContext ec = ExecutionContext.Build(c);
            exeObject.Execute(ec);

        }

        

        private ClientContainer Client {
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
    }

    // Supporting classes
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
}
