﻿Feature: Approve a developed issue
	In order to ensure that high quality code is merged
	As a developer
	I want to approve a solution merge

Scenario: Approve a solution merge awaiting review
	Given I am logged in to the 'Development Hub' app as 'an admin'
	And I have created 'a solution merge awaiting review'
	And I have opened 'the solution merge'
	When I select the 'Approve' command
	Then I can not see the 'Approve' command