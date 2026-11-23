# BookMatrix

## Development admin accounts

The development configuration includes two demo admin accounts:

- `Admin1` / `123@`
- `Admin2` / `456@`

Run the application from the project directory:

```powershell
dotnet run
```

Then open `http://localhost:5080/admin-login`.

On startup, `DbInitializer` seeds or updates both accounts from the
`AdminAccounts` configuration section. The database stores password hashes,
not plaintext passwords.

These credentials are intended only for local development. Do not use them
for a public or production deployment.

For another development environment, user secrets override
`appsettings.Development.json`:

```powershell
dotnet user-secrets set "AdminAccounts:Admin1:Username" "Admin1"
dotnet user-secrets set "AdminAccounts:Admin1:Password" "123@"
dotnet user-secrets set "AdminAccounts:Admin2:Username" "Admin2"
dotnet user-secrets set "AdminAccounts:Admin2:Password" "456@"
```

For production, configure both complete admin accounts with environment
variables or a secrets provider instead of adding passwords to source control:

```text
AdminAccounts__Admin1__Username
AdminAccounts__Admin1__Password
AdminAccounts__Admin2__Username
AdminAccounts__Admin2__Password
```
