use below command for EF and DB migration

dotnet ef migrations add InitialIdentityServerMigration -c PersistedGrantDbContext
dotnet ef migrations add InitialIdentityServerMigration -c ConfigurationDbContext

dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext

dotnet ef migrations add InitialIdentityServerMigration -c ApplicationDbContext
dotnet ef database update -c ApplicationDbContext