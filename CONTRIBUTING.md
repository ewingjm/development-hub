# Contributing

Please first discuss the change you wish to make via an issue before making a change.

## Development environment

You will need to setup a development environment before you can start making changes to the Development Hub app. If you do not have an existing instance, you can create one for free with the [Power Apps Community Plan](https://docs.microsoft.com/en-us/powerapps/maker/dev-community-plan) or signing up for a [trial](https://trials.dynamics.com/).


### Power Apps CLI

You must have the [Power Apps CLI](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/powerapps-cli) installed to use some of the Visual Studio Code build tasks. The _solution.json_ in each solution folder has a `developmentProfile` property - this is the Power Apps CLI authenication profile that maps to the development environment for that solution. You must create these authentication profiles using `pac auth create`.

### Power Apps

A Visual Studio Code task has been defined which will allow you to provision a development instance for a given solution. For example, if you wanted to contribute to the devhub_DevelopmentHub_Develop solution, you would open the command palette within VS Code (_ctrl + shift + p_) and select _Tasks: Run Task_ followed by _Prepare Development Environment_ and _devhub_DevelopmentHub_Develop_. This task requires that you have first configured the authentication profile for the solution (as explained above).

### Build tasks

Build tasks have been defined to make development easier. Call these in Visual Studio Code via the command palette (_ctrl + shift + p_) and selecting _Tasks: Run Task_.

The following tasks are available: 

- Clean
- Restore
- Compile
- Compile tests
- Prepare development environment
- Generate early-bound model
- Deploy workflow activities
- Deploy plug-ins
- Extract solution
- Pack solution

A description will be shown for each.

> ⚠ **Building outside of the tasks is not recommended!**. If you are building the solution in Visual Studio, you should set the following environment variable:
`MSBUILDDISABLENODEREUSE=1`. This is a workaround for an apparent bug in the MSBuild tasks used by solutions created with the Power Apps CLI. If you do not disable parallel builds or node reuse using the environment variable above, plug-in assembly files become locked afte a build and subsequent builds will fail.
### Extract to source control

Before creating a pull request containing your changes, you must extract the solutions into source control. This can be done using the _Extract solution_ task and specifying which solution(s) to extract.

## Pull requests

A maintainer will merge your pull request once it meets all of the required checks. Please ensure that you:

- Write commit messages that increment the version using [GitVersion](https://gitversion.readthedocs.io/en/latest/input/docs/more-info/version-increments/) syntax
- Write commit messages follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification
- Write automated tests to cover any new or changed functionality 
- Update the README.md with details of any new or changed functionality
