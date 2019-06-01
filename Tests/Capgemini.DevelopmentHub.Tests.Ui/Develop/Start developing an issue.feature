Feature: Start developing an issue
	In order to show progress and have a development solution created for me
	As a developer
	I want to start developing an issue that has been created in Dynamics 365

Scenario: An unsaved issue
	Given I am logged in to the Development Hub app as an admin
	And I am viewing the Issues sub area of the Issues area
	When I select the New command
	Then I can't see the Start Developing command

Scenario: A 'to do' issue
	Given I am logged in to the Development Hub app as an admin
	And I have created a 'To Do' issue
	And I have opened the issue
	Then I can see the Start Developing command

Scenario: An issue that is not 'to do'
	Given I am logged in to the Development Hub app as an admin
	And I have created a 'In Progress' issue
	And I have opened the issue
	Then I can't see the Start Developing command
