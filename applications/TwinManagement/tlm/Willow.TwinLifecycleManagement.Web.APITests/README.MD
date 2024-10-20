# Prerequisites

- Alba
- FakeAuth
- FluentAssertions
- Microsoft.NET.Test.Sdk
- RestSharp
- WireMock.Net
- WireMock.Net.StandAlone
- xunit
- xunit.runner.visualstudio

# Install dependencies

Open NuGet Packadge Manager and istall latest version of the dependency

# Running the test

- Build the project
- On the left side of the VS there is a Test Explorer
- Select test you want to run
- Click on play button to run the test
- More detalis : https://willow.atlassian.net/wiki/spaces/WCP/pages/2193818652/How+to+run+service+tests+locally

# Writing own tests

House rules for writing tests:
- The test code is located in `Willow.TwinLifecycleManagement.Web.APITests` folder
- If static files are required then add them to `./TestData` folder. Its content gets copied to the destination folder (`dist`) along with compiled scripts.
