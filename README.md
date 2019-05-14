# DevelopmentHub for Dynamics 365

## Introduction

DevelopmentHub Dynamics 365 package - generated using the [package generator](https://capgeminiuk.visualstudio.com/Capgemini%20Reusable%20IP/_git/generator-cdspackage).

## What is a Dynamics 365 package?

A Dynamics 365 package is comprised of:

- One or more solutions and associated source code
- Data
- A PackageDeployer import config

A greater number of more granular solutions is beneficial when working in shared development environments and having a monolithic repository per package allows for greater productivity and more atomic pull requests.

Dynamics 365 packages are deployed using the [PackageDeployer](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/admin/deploy-packages-using-package-deployer-windows-powershell)

## Contributing

Please ensure that pull requests are atomic and do not contain partially built functionality. This allows for holistic code reviews, cleaner git history and a more stable package. The repository contains all of the dependencies required to develop Dynamics 365 functionality.

### Create environment variables

Two environment variables are required to enable you to authenticate with the development and staging environments:

- CAKE_CONN_DEV: `Url=https://<org>.<region>.dynamics.com; Username=<email>; Password=<password>; AuthType=Office365;`
- CAKE_CONN_STAGING: `Url=https://<org>.<region>.dynamics.com; Username=<email>; Password=<password>; AuthType=Office365;`

Ensure you have replaced `<email>` and `<password>` with your own details.

### Create a git branch

Create a git branch from master using the following naming convention:

`<category>/<key>-<description>`

- All characters should be lowercase and spaces should be separated by hyphens.
- Category should be either: `feature` for new functionality, `bug` for bug fixes, or `tech` for any technical changes (e.g. updating builds etc.).
- Key will be the numeric portion of the story, bug, or task's key/ID (e.g. 1722).
- Description will be a summary of the story, bug or task. This will possibly be the same as the Jira issue name but it may have to be made more succinct.

For example, `feature/1722-view-and-maintain-accounts`.

### Create a development solution

A development solution should be created in the development environment which will exist until your branch is merged with master. The solution should be created with the following convention:

- Unique Name: `ds_<key>_<description>`
- Display Name: `<description>`

Using the above branch as an example, we would create a solution with the following values:

- Unique Name: `ds_1722_ViewAndMaintainAccounts`
- Display Name: `View and Maintain Accounts`

The following rules need to be adhered to when working in your development solution:

- Only one development solution can make changes to a component (e.g. relationship, field, view, form, assembly or process) at a time. Check other development solutions if you are unsure. Multiple development solutions modifying the same components will mean that either:
  - Unfinished customisations will be added to the build
  - Developent solutions cannot be merged because of dependencies added by other development solutions
- Avoid locking out shared components by modifying them last if possible (e.g. add your components to the app as the last step in your story)
- Add only the components that you require to your solution. Do not check 'Add all assets' or 'Include entity metadata' when adding entities.
- Do not add dependencies when prompted. These should already exist in the target system (the staging environment).
- Avoid changes to managed components where possible. There may be a couple of exceptions.

### Processes and plugins

- Workflows should generally be created with scope set to Parent: Child Business Units. This allows us to isolate our processes to DevelopmentHub - especially important when dealing with out-of-the-box entities.
- Plugin steps can't be scoped so alternatives should be considered when dealing with out-of-the-box entities and messsages.

### Deleting components

To delete a component:

- Ensure that any dependencies are removed in your development solution (e.g. remove a field from any forms or views that reference it)
- Delete any dependent components if no longer required and make a note of these components
- Delete the component
- Import the development solution into the staging environment when ready
- Delete the component from the staging environment

## Tools

Visual Studio is recommended for .NET development (i.e. plugins assemblies) while Visual Studio Code is recommended for most other tasks.

- Visual Studio

  - NPM Task Runner
  - Cake for Visual Studio
  - SpecFlow for Visual Studio

- Visual Studio Code

  - Cake
  - npm
  - Azure Repos

- Fiddler

## Cake

Cake is a build automation tool that can be integrated with Visual Studio and Visual Studio code through extensions. An add-in for Cake has been developed by the Capgemini Dynamics team which automates many of the day-to-day tasks of Dynamics 365 developers. A Cake build script (_build.cake_) and bootstrapper (_build.ps1_) are present in the root of this repository.

It is not recommended to call the Cake build executable directly. The _build.ps1_ bootstrapper script should be used instead. The bootstrapper script handle dependency resolution, negating the need to store Cake dependencies in source control.

**Note:** The Cake extension for Visual Studio does not use the _build.ps1_ bootstrapper. It is recommended that you run your first Cake task through Visual Studio Code to resolve dependencies first.

### Extracting a solution

Solutions can be extracted using the `Extract Solution` task.

### Packing a solution

Solutions can be packed into managed and unmanaged solution zip files using the `Pack Solution` task.

_Note: it is unlikely that developers will need to pack the solutions themselves. This is typically done via CI build_

### Extracting data

Data can be exported using the `ExportData` task.

The data is exported using the [Capgemini Data Migration Engine](https://capgeminiuk.visualstudio.com/Capgemini%20Reusable%20IP/_git/Capgemini.Xrm.DataMigration) and the export config file located in the relevant data folder.

### Deploying web resources

In most instances, developers should [configure Fiddler AutoResponder rules](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/streamline-javascript-development-fiddler-autoresponder) and deploy their web resource via Dynamics 365 UI.

### Deploying plugins

Plugins can be deployed using the `Deploy Plugins` task. This deploys all steps declared via the Spkl attribute.

### Deploying workflow activities

Worklow activities can be deployed using the `Deploy Workflow Activities` task. This deploys all workflow activities declared via the Spkl attribute.

### Generating the early-bound model classes

The early-bound model classes can be generated using the `Generate Model` Cake task. It will use the configuration file located at _Common\Client.Package.Model\DLaB.EarlyBoundGenerator.DefaultSettings.xml_. It is recommended to use the early-bound generator XrmToolbox plugin to update this configuration file.

### Building the package

The entire package can be built using the `Build Package` task. This will pack all solutions and copy them to the _Package_ folder. The PackageDeployer import configuration and reference/configuration data and associated import configurations will also be copied to this folder.
