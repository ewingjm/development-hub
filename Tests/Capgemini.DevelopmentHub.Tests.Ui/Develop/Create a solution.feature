Feature: Create a solution
	In order to automate activities related to a solution
	As a developer
	I want to create solutions in Dynamics 365

Scenario: Create a new solution
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Solutions sub area of the Develop area
	When I select the New command
	Then I can edit the following fields 
	| Field                  |
	| cap_displayname        |
	| cap_uniquename         |
	| cap_stagingenvironment |
	| cap_description        |

Scenario: Create a new issue without mandatory fields
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Solutions sub area of the Develop area
	When I select the New command
	And I save the record
	Then a mandatory field error is displayed on the following fields
	| Field    |
	| cap_displayname        |
	| cap_uniquename         |
	| cap_stagingenvironment |