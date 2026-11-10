# Parisal

ASP.NET Core applications for the storefront (Authorization) and admin panel.

Database passwords in both appsettings.json files are placeholders. Set ConnectionStrings__AuthorizationDatabase in your environment to your PostgreSQL connection string before running.

Run the applications with:

    dotnet run --project Authorization/Authorization.csproj
    dotnet run --project "admin panel/AdminPanel.csproj"
