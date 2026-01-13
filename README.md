Converter - Phase 1 (Auth scaffold)

This repository includes a minimal ASP.NET Core backend and a small frontend to handle user registration and login (phase-1).

Quick steps (macOS):

1. Start SQL Server in Docker:

```bash
cd Converter
docker compose up -d
```

2. Restore and run the API:

```bash
cd backend
dotnet restore
dotnet tool install --global dotnet-ef --version 7.* || true
dotnet ef migrations add Initial -p ConverterApi.csproj -s ConverterApi.csproj
dotnet ef database update -p ConverterApi.csproj -s ConverterApi.csproj
dotnet run
```

3. Open frontend at frontend/index.html (open directly in browser).

Connect Azure Data Studio to the database at `localhost,1433` using username `sa` and password `Your_password123`.

Notes:
- The API uses JWT for login responses. Passwords are salted+hashed.
- This is a minimal scaffold for phase‑1. Next: file-conversion endpoints and public upload.
