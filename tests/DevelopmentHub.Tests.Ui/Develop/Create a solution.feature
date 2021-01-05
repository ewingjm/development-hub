Feature: Create a solution
	In order to automate activities related to a solution
	As a developer
	I want to create solutions in Common Data Service

Scenario: Create a new solution
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Solutions' under the 'Develop' area
	And I select the 'New' command
	Then I can edit the following fields
		| Field                     |
		| devhub_displayname        |
		| devhub_uniquename         |
		| devhub_stagingenvironment |
		| devhub_description        |
	When I select the 'Azure DevOps' tab
	Then I can edit the following fields
		| Field                                      |
		| devhub_azuredevopsproject                  |
		| devhub_azuredevopsextractbuilddefinitionid |

Scenario: Create a new issue without mandatory fields
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Solutions' under the 'Develop' area
	And I select the 'New' command
	And I save the record
	Then I can see error form notifications stating the following
		| Message                                                                       |
		| Display Name : Required fields must be filled in.                             |
		| Unique Name : Required fields must be filled in.                              |
		| Staging Environment : Required fields must be filled in.                      |
		| Azure DevOps Project : Required fields must be filled in.                     |
		| Azure DevOps Extract Build Definition ID : Required fields must be filled in. |