# Development Hub ![logo](./docs/images/logo.svg)

The Development Hub provides an enhanced development workflow for teams building Power Apps. Enables continuous integration by allowing developers to submit their changes for review and automated merging to source control.

For more context, read [this](https://medium.com/capgemini-microsoft-team/continuous-integration-for-power-apps-the-development-hub-7f1b4320ecfd?source=friends_link&sk=c5034f278e70bfd9aa2dce502dd490d9) introductory blog post.

## Table of contents

* [Prerequisites](#prerequisites)
* [Installation](#installation)
  + [Register an app](#register-an-app)
  + [Configure Azure DevOps](#configure-azure-devops)
  + [Create flow connections](#create-flow-connections)
  + [Deploy the package](#deploy-the-package)
* [Configuration](#configuration)
  + [Environments](#environments)
  + [Solutions](#solutions)
* [Usage](#usage)
  + [Create an issue](#create-an-issue)
  + [Develop a solution](#develop-a-solution)
  + [Merge a solution](#merge-a-solution)
  + [Merge source code](#merge-source-code)
  + [Perform manual merge activities](#perform-manual-merge-activities)
  + [Handle a failed merge](#handle-a-failed-merge)
* [Contributing](#contributing)
  + [Build a development environment](#build-a-development-environment)
  + [Set environment variables](#set-environment-variables)
  + [Run build tasks](#run-build-tasks)

## Prerequisites

- At least two Dataverse environments
- An Azure DevOps organisation

One Dataverse environment acts as the development environment. The other environment(s) are referred to as 'master' environments. For more information on the purpose of a master environment, refer to the [Solution Lifecycle Management](https://www.microsoft.com/en-us/download/details.aspx?id=57777) document published by Microsoft. The relevant information can be found in the _environment topologies for development approaches_ table.

## Installation

### Register an app

An Azure AD app and application users with the System Administrator role must be registered in the development and master environments.

Follow Microsoft's guide [here](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/use-single-tenant-server-server-authentication#azure-application-registration).

### Configure Azure DevOps

Navigate to _Project Settings -> Repositories_ in your Azure DevOps project. Select the relevant repository and assign the following privilges to the project Build Service user:

* Bypass policies when pushing
* Contribute
* Create branch 

A build definition capable of extracting solutions is required. There are several files in the [samples](./samples) folder to help you with this. 

If you use the sample files as is, copy the _scripts_ folder and _azure-pipelines-extract.yml_ file into your repository. The sample build script assumes that your repository structure is that you have a _src_ folder at the root containing a _solutions_ folder, which then contains folders that match your solutions' unique names. Within each solution folder, you should have a _solution.json_ file that provides the development environment URL (see the _solution.json_ in the samples folder), and an _extract_ folder alongside it to contain the unpacked solution zip file fragments. 

Lastly, you must create a _Development Hub_ variable group on Azure DevOps that contains three variables - `Client ID` , `Tenant ID` , and `Client Secret` . These should be taken from the app registration created earlier.

If you have an existing folder structure which is different, the _Merge-SolutionVersion.ps1_ script will require tweaking - but the _azure-pipelines-extract.yml_ file shouldn't need to be changed.

### Create flow connections

You will need to create two flow connections in your development environment.

Within the [Maker Portal](https://make.powerapps.com), go to your environment and navigate to _Data -> Connections_. Create a new _Approvals_ connection and a new _Azure DevOps_ connection. Ensure that the Azure DevOps connection is signed in as a user with access to the Azure DevOps project. 

Note the connection names for both of the created connections. You can find this by opening the connection and taking it from the URL, which should be in the format 'environments/environmentid/connections/apiname/_connectionname_/details'.

### Deploy the package

The package can be deployed to your development environment using the Package Deployer. Download the package files from the Releases tab and follow Microsoft's guide to using the Package Deployer [here](https://docs.microsoft.com/en-us/power-platform/admin/deploy-packages-using-package-deployer-windows-powershell). 

You must provide several package deployer settings parameters. Refer to the helper PowerShell script below:

```powershell
$settings = [PSCustomObject]@{
  ApprovalsConnectionName = '<the connection name of the Approvals connection>'
  AzureDevOpsConnectionName = '<the connection name of the Azure DevOps connection>'
  AzureDevOpsOrganisation = '<the name of the Azure DevOps organisation>'
  AzureDevOpsPipelineId = '<the ID of the Azure DevOps extract pipeline>'
  AzureDevOpsProject = '<the name of the Azure DevOps project>'
  SolutionPublisherPrefix = '<the prefix of the publisher (without leading underscore)>'
}
$settingsArray = $obj.PSObject.Properties | ForEach-Object { "$($_.Name)=$($_.Value)" }
$runtimePackageSettings = [string]::Join("|", $settingsArray)

Import-CrmPackage -PackageInformation $packages[0] -CrmConnection $conn -RuntimePackageSettings $runtimePackageSettings
```

## Configuration

### Environments

Master environments must be configured by with an Environment record. You must enter a URL and name for the environment as well as details about the app registration used to authenticate.

### Solutions

Ensure you have created or imported your unmanaged solution(s) for extraction in the master environment. Once this is done, they can be registered within the Development Hub app by creating a `Solution` record. 

Do not change the version numbers if the solution is new. If it is an existing solution, update the version numbers to match the solution in the master environment. The version will from then on be managed by the Development Hub when merging changes.

## Usage

This section details the usage of the Development Hub's core functionality. 

### Create an issue

`Issue`s must be created within the Development Hub in order for a developer to begin working on a new feature or bug fix. 

The issue records in the Development Hub are used to group solution merge records and aid in applying semantic versioning to solutions. The Development Hub is not intended to replace a more conventional issue tracking system (e.g Azure Boards). It is suggested to either create issue records on-the-fly, at the beginning of a sprint, or by integrating Azure DevOps through a tool such as Power Automate. 

If your issues are on Azure Boards, you can set the `Work Item ID` field on the corresponding Development Hub issue. The commit will then be linked with the Azure DevOps work item. 

An issue with a 'To Do' status will have a `Develop` button in the ribbon. Clicking this will create a development solution and transition the issue to `In Progress`. The `Development` tab will show details about the development solutions and solution merge history.

### Develop a solution

The developer must add any new components or components to be modified into their development solution. It is important that only one developer makes changes to a component at a time. If a component appears in more than one development solution, it will result in either incomplete work being merged or solution merges failing due to missing dependencies. 

Development solutions should contain just the components created or updated for that issue and no more. Adding all assets to a development solution will add all assets to the target solution when merged.

### Merge a solution

Once the issue has been developed, a `Solution Merge` record can be created. This will transition the issue to `Developed`. The solution merge is created in an `Awaiting Review` status. Review comments can be added to the solution merge in the form of notes and the solution merge either approved or rejected. 

Once approved, the development solution will be merged into the target solution. If multiple solution merges have been approved, they will enter a queue. This means that an `Approved` solution merge will transition to either a `Merging` or `Queued` status.

A successful solution merge will transition to an inactive `Merged` status. The `Version History` tab on the target solution record will also contain a new record with the post-merge unmanaged and managed solution zips available. The new solution version is based on the type of issue merged. A `Feature` issue will increment the minor version and a `Bug` issue will increment the patch version. Major version changes must be done manually. 

### Merge source code

If the solution to be merged has associated source code (e.g. you have made changes to plugin assemblies or web resources) then you must specify the `Source Branch`. Ensure that you perform any manual merging required in Git on your source branch before creating the solution merge. This branch will be merged automatically.

### Perform manual merge activities

Specifying that there are `Manual Merge Activities` on the solution merge record will cause the merging process to pause before extracting to source control. This is useful where you are merging changes by hand e.g. components that need to be updated frequently by multiple developers or where you need to delete components from the solution. 

When the merging process is in a state where manual merge activities can begin, the solution merge will transition to an `Awaiting Manual Merge Activities` status. If you are deleting components from the solution in the master environment, it is recommended to update the major version of the solution record in the Development Hub during this period.

To notify the flow that the manual merge activities are complete, navigate to _Action items -> Approvals_ within Power Automate and set the approval status to merged.

### Handle a failed merge

If the merging process failed (e.g. due to missing dependencies) then the solution merge will transition to a `Failed` status. A note will be attached with a link to the failed flow run which can be used to diagnose the failure reason. A `Retry` button is available to retry the merge after the necessary steps have been taken.

## Resources

- [Blog post](https://medium.com/capgemini-microsoft-team/continuous-integration-for-power-apps-the-development-hub-7f1b4320ecfd)
- [Introduction video](https://youtu.be/p-z1iTxtaag)
- [Usage video](https://www.youtube.com/watch?v=co1zCvureiM)

## Contributing

Refer to the contributing [guide](./CONTRIBUTING.md).
