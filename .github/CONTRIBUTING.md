# Contributing to WoWs-ShipBuilder

Thank you for your interest in the project!

This file is meant to show some guidelines for contributing to this project. These guidelines are not strict rules but also not irrelevant.
If you feel like something important is wrong or missing, feel free to submit a pull request to propose changes.

## Table of contents

- [Getting started](#getting-started)
- [How to contribute](#how-to-contribute)
- [Style Guide](#style-guide)


## Disclaimer

This project has no connection to Wargaming, it is a project by the community trying to provide some useful tools for players of the game World of Warships.
Please do not contact us for game issues, we are only responsible for issues directly related to this application.


## Getting started

The project consists of three main components: The WoWs-ShipBuilder application itself, the DataConverter project used to convert extracted game data from Wargaming into a format we can use and the gameparam-extractor.
The main application and the data converter are written in C# with .NET 5 while the gameparam-extractor is written in Python.

To start contributing to this repo (the main application), make sure you have a C# IDE and the .NET 5 SDK set up on your machine. 
If you do not have any of this, it might be best to check out the docs from Microsoft on how to set up the .NET SDK.
Dependencies are managed using the nuget package manager.


## How to contribute?

There are different ways to contribute to the project:

- Helping with translating the project
- Reporting bugs and creating feature requests
- Take part in the actual development of the project (Code Contribution)

### Helping with translating the project

If you wish to help with the translation of the project, please check out the translation section in this project's README and head over to [Crowdin](https://crowdin.com/project/wows-shipbuilder).
It is also recommended to join our [Discord server](https://discord.gg/C8EaepZJDY) for updates regarding localizations.

### Bug reports

If you encounter a bug, you can report it using the [issues](https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder/issues) section of the repository.
When reporting a bug, please consider the following steps:

1. Check the list of existing issues and ensure that your bug has not been reported yet. If it was reported already, it might be better to comment on that issue instead of opening a new one.
2. Check whether you can reproduce the bug. What steps are necessary to trigger it?
3. Get your log files. The application writes logs to `%APPDATA%\WoWsShipBuilder\logs`. 
In case of an application crash or an error message, please attach the corresponding log file or send us the log file using discord in a PM if you do not want it to be public.
You may also just search the section that is related to your issue, we will ask for additional details if necessary.
4. Report the bug using the correct issue template.

### Feature requests

If you have an idea for a new feature or want to change a feature, create a new issue with the feature request template.

When proposing a new feature, please check the following list:

1. Does it provide additional value to users?
2. How does it impact the application's UI/UX and performance?
3. Does it fit to the application's profile? We do not want to show player statistics or create toxicity. 
The application is meant to help evaluating and visualizing various ship and captain builds as well as provide utilities related to this.

### Code Contribution

If you want to contribute to the application's source code, please go to the project's [issues](https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder/issues) section and find an unassigned issue.
If an issue is already assigned to someone but you want to help with it, make sure to contact the assignee first!

Issues with lots of comments are usually more important issues and should be dealt with first.
When contributing code, please make sure to check our [Style Guide](#style-guide) first.
Those styling rules are not always enforced but help keeping a consistent code style in the project, improving readability and maintainability.
Additionally, the C# projects also use Analyzers that are integrated in the build process and will trigger build warnings or errors in case of violations.
When a change is committed, try to ensure that it does not show any new warnings during build as this may result in the build failing.


## Style Guide

### Commit messages

- Ensure correct spelling
- Use the present tense ("Add new feature" not "Added new feature")
- Keep it as short as possible but as long as necessary
  - If the change cannot be summarized within a single line or sentence, put the description in the message body (after the summary line)
- Do not use Emojis in the commit message
- If your commit handles an issue or pull request, reference it in the commit message using `#ID`
  - If your commit resolves an issue, reference it using `fixes #ID` or `closes #ID`

### C# and XAML Code Style

Basic C# code style rules are already enforced through the use of StyleCop.Analyzers. If you think a StyleCop warning is wrong, DO NOT SUPPRESS it!
Check with the repository maintainers first whether this rule should be changed or if it is really a situation where it should be suppressed only once.

If your IDE supports editorconfig files, make sure that feature is turned on. Some basic style rules are configured in editorconfig files within the project.

- Use camelCase syntax for private variables (`private int testVariable`)
- Use UpperCamelCase syntax for properties (`int TestVariable { get; }`)
- Do not prefix private members with an underscore.
- Do not use snake_case for variable, class or method names except in unit test methods.
  - In unit test methods, name them TestedMethodName_Precondition_Postcondition
- Do not expose public variables. Use properties instead.
- Follow the element ordering rules of StyleCop.
- Use an indentation of 4 spaces. One tab equals 4 spaces.
- Document public methods and classes! Documentation for private members is not bad either but not always required.
- `using` directives are placed outside of namespaces.

Other style rules:

- Try to avoid hardcoded references to specific instances of objects. You may use them as fallback but there should be a way to substitute the reference in unit tests.
- ViewModels must expose a parameterless constructor in order for the Avalonia XAML previewer to work.
  - If you need custom constructor parameters, call that constructor from the parameterless one with default values that work with your code.
  - For the ShipBuilder UI project, you can access the DataHelper class to retrieve example data. Modify that class if you need complex example data that cannot be created with a single method or constructor call.
- There is a "soft" line length limit of 160 characters. This limit is not enforced but it is preferred to add line breaks if it improves readability.
  - For chained method calls, place each method call on a new line if you add a line break.

**The project uses C# 9 with the Nullable Reference Types feature enabled. Do not ignore warnings regarding this feature as they will cause the production build to fail.**

If you need additional nuget dependencies, please contact the repository maintainers first before adding them.
We want to keep the application as small as possible. If you add dependencies, ensure that they work in a trimmed release.
To test the release-compatibility of your local changes, run the local release test script `Tools\LocalReleaseTest.ps1`, specify a version (needs to be in format X.X.X) and install the created executable locally.
