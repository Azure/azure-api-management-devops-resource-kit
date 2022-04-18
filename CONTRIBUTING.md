# Contributing to the Azure API Management DevOps Resource Kit

We welcome community contributions to the DevOps Resource Kit.  We do, however, ask that you follow some basic rules
when contributing.

## Filing Issues

Filing issues is a great way to contribute to the SDK. Here are some guidelines:

* If filing a bug or feature request for Creator or Extractor, ensure you use the appropriate template.
* Include as much detail as you can be about the problem or feature.
* Github supports markdown, so when filing bugs make sure you check the formatting before clicking submit.

## Contributing Code

We welcome code contributions via GitHub Pull Request (PR).

* All code contributions must have an associated issue that provides detail on what your pull request is fixing.
* All code contributions must include tests for the new or fixed functionality.

## Development Environment Setup

The Extractor and Creator are written in .NET Core.  Our basic environment is:

* .NET Core 3.1 LTS (we are intending on updating to .NET 6 in the near future).
* Visual Studio 2022

You can open the solution as you would any other Visual Studio solution.

## Coding Guidelines

Just follow the existing patterns already present in the code.  If we object to the style, we will say so during the code review.

## Submitting Pull Requests

Before we can accept your pull-request you'll need to sign a [Contribution License Agreement (CLA)](http://en.wikipedia.org/wiki/Contributor_License_Agreement). 
However, you don't have to do this up-front. You can simply clone, fork, and submit your pull-request as usual.  

The pull request must meet the following requirements:

* One pull request per issue - do not fix multiple issues in the same pull request without talking to us.
* Include tests for the new or updated functionality.
* Title: `(#<issue>) <functionality>` - this ensures the issue and pull request are linked by GitHub.
* Content: must include `Closes #<issue>` - this ensures the issue is closed when the PR is merged.

Once the pull request is submitted, there are three phases:

1. The build and test is automatically run.  If the build/test fails, you need to correct the issues before proceeding.
1. A human reviewer will ensure that you have added tests to cover the new functionality.  We will ask for tests before proceeding.
1. A human reviewer will do a code review, identifying any coding issues.  Feel free to discuss and resolve any issues within the PR.

Once a maintainer approves the PR, it will be merged.

## Releases

We release whenever there are significant changes. To release, a maintainer will generate a release tag. This will set off a
build and test process that automatically uploads the artifacts to GitHub releases.  You can always find the latest release on
GitHub releases.

If your issue is fixed but has not been released yet, you can run a build yourself.  The Azure Pipelines configuration we use to
build and release is in the `.azure-pipelines` directory within the repository.
