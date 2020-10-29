Feature: Create an issue
	In order to track issues to be resolved
	As a developer
	I want to create issues

Scenario: Create a new issue
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Issues' under the 'Issues' area
	And I select the 'New' command
	Then I can edit the following fields
		| Field              |
		| devhub_name        |
		| devhub_type        |
		| devhub_description |
		| devhub_url         |

Scenario: Create a new issue without mandatory fields
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Issues' under the 'Issues' area
	And I select the 'New' command
	And I save the record
	Then the field error 'Required fields must be filled in.' is displayed on the following fields
		| Field              |
		| devhub_name        |
		| devhub_type        |
		| devhub_description |