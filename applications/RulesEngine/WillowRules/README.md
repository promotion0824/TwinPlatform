Willow Rules Assembly
====

This contains the model classes. 

To generate the migrations for SQL run ...

    dotnet ef migrations add "...name of migration..." --project ../WillowRules

Do not use `dotnet ef database update` to update the local database, let the code handle this for each database it creates.

You may need to install or update your ef tools:

    dotnet tool update --global dotnet-ef
