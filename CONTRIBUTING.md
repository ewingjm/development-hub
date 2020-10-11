# Contributing

Please first discuss the change you wish to make via an issue before making a change.

## Development environment

You will need to setup a development environment before you can start making changes to the Development Hub app.

### Power Apps

A Visual Studio Code task has been defined which will allow you to recreate a development instance for a given solution. If you wanted to contribute to the devhub_DevelopmentHub_Develop solution, you would open the command palette within VS Code (_ctrl + shift + p_) and select _Tasks: Run Task_ followed by _Build Development Environment_ and _devhub_DevelopmentHub_Develop_. This task requires that you have first configured the development environment URL _solution.json_ file in the corresponding solution folder and set up your environment variables (see below).

If you do not have an existing instance, you can create one for free with the [Power Apps Community Plan](https://docs.microsoft.com/en-us/powerapps/maker/dev-community-plan) or by signing up for a [trial](https://trials.dynamics.com/).

### Environment variables

Two environment variables are required to enable you to authenticate with the development and staging environments:

- CAKE_DYNAMICS_USERNAME_DEVELOPMENT_HUB
- CAKE_DYNAMICS_PASSWORD_DEVELOPMENT_HUB

The username in the environment variable is used unless overridden by the username set in the corresponding _solution.json_ file. This is useful where the username for each solution is different (e.g. where you have multiple trials).

### Build tasks

A number of Cake build tasks have been defined to make development easier. It is recommended to call these via the command palette (_ctrl + shift + p_) and selecting _Tasks: Run Task_.

The following tasks are available: 

- Build Development Environment
- Extract Solution
- Pack Solution
- Build Package
- Deploy Plugins
- Deploy Workflow Activities
- Generate Model

### Extract to source control

Before creating a pull request containing your changes, you must extract the solutions into source control. This can be done using the _Extract Solution_ task and specifying which solution(s) to extract.

## Pull requests

A maintainer will merge your pull request once it meets all of the required checks. Please ensure that you:

- Write commit messages that increment the version using [GitVersion](https://gitversion.readthedocs.io/en/latest/input/docs/more-info/version-increments/) syntax
- Write commit messages follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification
- Write automated tests to cover any new or changed functionality 
- Update the README.md with details of any new or changed functionality
