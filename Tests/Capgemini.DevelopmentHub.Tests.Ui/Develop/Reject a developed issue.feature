Feature: Reject a developed issue
	In order to ensure that low quality code is not merged
	As a developer
	I want to reject a solution merge

Scenario: Open a solution merge awaiting review
	Given I am logged in to the Development Hub app as an admin
	And I have created an 'Awaiting Review' solution merge
	And I have opened the solution merge
	Then I can see the Reject command

Scenario: Approve a solution merge awaiting review
	Given I am logged in to the Development Hub app as an admin
	And I have created an 'Awaiting Review' solution merge
	And I have opened the solution merge
	When I select the Reject command
	Then I can't see the Reject command