# Invite Administrator

Create an endpoint in AdministrationModule to invite a new administrator

It should be a minimal api endpoint, and it will need an EventCommandRouter injected into it.

It will be a POST endpoint (no auth for now, this will come in later stories)

It will accept an InviteAdministratorRequest which is already defined in the AdministrationContracts project.

Step 1, create an InviteAdministratorCommand which is already defined in the AdministrationModule map the properties over

Execute the EventCommandRouter and return a 201 if success or a 400 with the error messages if the result comes back as a failure.

Let's add integration tests here as well covering validation scenarios.

Right now there is no way to check inviting the same email twice so leave that for now.