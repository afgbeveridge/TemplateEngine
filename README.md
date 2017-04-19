## Simple text template engine
This project allows you to apply a context (typically 1..n c# objects) to a parsed input sink. The text stream contains simple markup that is used to identify 
objects within the context supplied, which is then written into the output sink.

This has been used in a startup company to externalise (persist in a database) email message templates (html), and parse and apply user specific context to generate a 
range of personalised email message(s), as requirements dictated.

## Requirements

* Visual studio 2017
* .net core 1.1 or 
* .net 4.6+
* System.Value.Tuple >= 4.3.0 (part of Nuget dependencies - required for a utilised C# 7 feature)

The Template.Engine is multi targeted, .net 4.6+ and .netcoreapp 1.1.

## Example template and code

A fuller reference follows, but an example template text file from the test console app is:

```Dear [Client.FirstName],<p></p>[context CompanyDetails]
You've got a new exercise prescription from [RegisteredName]. As agreed, your goals are:<p></p><ul>
[context ClientContainer]
[foreach Goals]
<li>[_]</li>
[end]
</ul><br/>A summary of the prescription we arrived at:<br/><ul>
<ul>
[foreach Exercises]
<li>[Region]</li>
[if Region == "Neck"]
Found a neck region
[else]
Work with [Region]
[end]
[end]
</ul>
<p>[context CompanyDetails][Address]>/p>
[context ClientContainer]
<p></p>For reference, your user name is [UserName], the password as agreed with your consultant.<p></p>Good luck!
```

We then use a method to parse and apply context to the template (assuming it is in file Test-2.txt):

```
         private static void GenerateTemplatedContentWithDefaultNamedContexts() {
            ParseResult res = Parser.Parse(File.ReadAllText("Test-2.txt"));
            EvaluationContext ec = EvaluationContext.From(Client, new CompanyDetails {
                Address = "10, The Lane, Somewhere",
                RegisteredName = "ABC Ltd"
            });
            var ctx = res.Execute(ExecutionContext.Build(ec));
            Console.WriteLine(ctx.context.ToString());
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
```

Which generates:

```
Dear Toe,<p></p>
You've got a new exercise prescription from ABC Ltd. As agreed, your goals are:<p></p><ul>

<li>Walk</li>
<li>Then Run</li>
</ul><br/>A summary of the prescription we arrived at:<br/><ul>
<ul>
<li>Neck</li>Found a neck region<li>Back</li>Work with Back
</ul>
<p>10, The Lane, Somewhere>/p>
<p></p>For reference, your user name is XX2,
the password as agreed with your consultant.<p></p>Good luck!
```

# Text template - form
As can be seen from the example template file shown above, a template is a series of bytes/characters that contain substitution tokens that
can be changed according to context.

## Subsitutition tokens
The standard token form as a regex is ```\[(?<key>[^\]]+)\]```.

* If this markup conflicts, you can request an escaped form - see the template console for an example or look in the unit tests. 

## Supported behaviours
A few points:
* Text file parsing is simplistic using Regex and lookaheads
* There are a few mechanisms to avoid catastrophic failure


| Keyword        |Other context| Semantics |
|----------------|-------------|------------------------|
| x.y... |N/A|A property reference; _ is an implicit reference (see foreach)|
| if |expression|Evaluate an expression and act on the result|
| else |N/A|an alternate branch of an if|
| end |N/A|ends a scope; used after if, if/else or foreach|
| foreach |property reference|treat the property reference as an enumerable|
| context |context reference|change the current context to be that given|


Notes:
* The implicit property reference, represented by the underscore (_), means emit the object that is currently being examined 'as is, where is'
* Using the context keyword, contexts can have been named, or will be auto-named. See the console app or the unit tests for examples

#### Setup and use
There are very few steps to use the template engine. Assume you have a document template as a string in an argument called ```doc```, and a single object you want to use as the context, called ```ctx```:

* Sometime before processing the template, you should invoke at least ```ExecutableObjectFactory.RegisterFromSelf();``` - this registers all keyword handlers
* Below is then a simple method that can process the template and create necessary transformations. Real world use would cache the parsed template:

```
        private string GenerateMessage<TObject>(string doc, TObject ctx) {
            ParseResult res = Parser.Parse(doc);
            EvaluationContext ec = EvaluationContext.From(ctx);
            var ctx = res.Execute(ExecutionContext.Build(ec));
            return ctx.context.ToString();
        }
```

