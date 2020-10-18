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

Scenario: Create a new issue without mandatory fields
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Solutions' under the 'Develop' area
	And I select the 'New' command
	And I save the record
	Then the field error 'Required fields must be filled in.' is displayed on the following fields
		| Field                     |
		| devhub_displayname        |
		| devhub_uniquename         |
		| devhub_stagingenvironment |