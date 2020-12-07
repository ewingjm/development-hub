Feature: Create an environment
	In order to automate activities related to an environment
	As a developer
	I want to create environments

	Scenario: Create a new environment
		Given I am logged in to the 'Development Hub' app as 'an admin'
		When I open the sub area 'Environments' under the 'Develop' area
		And I select the 'New' command
		Then I can edit the following fields
			| Field               |
			| devhub_name         |
			| devhub_url          |
			| devhub_clientid     |
			| devhub_tenantid     |
			| devhub_clientsecret |

	Scenario: Create a new environment without mandatory fields
		Given I am logged in to the 'Development Hub' app as 'an admin'
		When I open the sub area 'Environments' under the 'Develop' area
		And I select the 'New' command
		And I save the record
		Then the field error 'Required fields must be filled in.' is displayed on the following fields
			| Field       |
			| devhub_name |
			| devhub_url  |
			| devhub_clientid     |
			| devhub_tenantid     |
			| devhub_clientsecret |