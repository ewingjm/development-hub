Feature: Create an environment
	In order to automate activities related to an environment
	As a developer
	I want to create environments in Dynamics 365

Scenario: Create a new environment
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Environments sub area of the Develop area
	When I select the New command
	Then I can edit the following fields 
	| Field    |
	| cap_name |
	| cap_url  |

Scenario: Create a new issue without mandatory fields
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Environments sub area of the Develop area
	When I select the New command
	And I save the record
	Then a mandatory field error is displayed on the following fields
	| Field    |
	| cap_name |
	| cap_url  |
