Feature: Start developing an issue
	In order to show progress and have a development solution created for me
	As a developer
	I want to start developing an issue that has been created 

Scenario: An unsaved issue
	Given I am logged in to the 'Development Hub' app as 'an admin'
	When I open the sub area 'Issues' under the 'Issues' area
	And I select the 'New' command
	Then I can not see the 'Develop' command

Scenario: A 'to do' issue
	Given I am logged in to the 'Development Hub' app as 'an admin'
	And I have created 'a new issue'
	And I have opened 'the issue'
	Then I can see the 'Develop' command

Scenario: An issue that is not 'to do'
	Given I am logged in to the 'Development Hub' app as 'an admin'
	And I have created 'an in-progress issue'
	And I have opened 'the issue'
	Then I can not see the 'Develop' command