Feature: Create an issue
	In order to track issues to be resolved
	As a developer
	I want to create issues in Dynamics 365

Scenario: Create a new issue
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Issues sub area of the Issues area
	When I select the New command
	Then I can edit the following fields 
	| Field           |
	| cap_name        |
	| cap_type        |
	| cap_description |
	| cap_url         |

Scenario: Create a new issue without mandatory fields
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Issues sub area of the Issues area
	When I select the New command
	And I save the record
	Then a mandatory field error is displayed on the following fields
	| Field |
	| cap_name        |
	| cap_type        |
	| cap_description |
