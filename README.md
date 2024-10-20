# Before starting
Before cloning this repo on Windows environments, you need to enable `core.longpaths` feature on Git.

```
git config --system core.longpaths true
```

# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

### Build Docker Image for Real Estate Projects
 Our Docker setup is optimized for efficiency by using pre-built binaries. The Dockerfile is configured to copy these pre-built files from either the debug or release folder of the C# project (in local development machine), or from a GitHub workspace where these binaries uploaded in GH build action step. This approach eliminates the need for package restoration and project rebuilding within the Docker image.
 ##### Example of building InsightCore docker image locally using the Debug folder
 ` docker build -t insightcore -f Dockerfile bin\Debug\net8.0`
# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
