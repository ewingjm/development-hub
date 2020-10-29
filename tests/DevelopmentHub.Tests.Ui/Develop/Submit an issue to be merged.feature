Feature: Submit an issue to be merged
	In order to have my completed development merged
	As a developer
	I want to submit an issue that has been developed 

Scenario: Create a solution merge
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Solution Merges' under the 'Develop' area
	And I select the 'New' command
	Then I can edit the following fields
		| Field                        |
		| devhub_issue                 |
		| devhub_targetsolution        |
		| devhub_manualmergeactivities |

Scenario: Create a new solution merge without mandatory fields
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Solution Merges' under the 'Develop' area
	And I select the 'New' command
	And I save the record
	Then the field error 'Required fields must be filled in.' is displayed on the following fields
		| Field                 |
		| devhub_issue          |
		| devhub_targetsolution |