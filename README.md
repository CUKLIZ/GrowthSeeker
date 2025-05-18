# 🚀 ASP.NET Core MVC Project

Welcome! 🎉  
---

## ✅ Requirements

- .NET SDK (.NET 7.0 or compatible)  
- Visual Studio 2022+ or Visual Studio Code  
- SQL Server (LocalDB or full)

---

## 🛠️ Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/CUKLIZ/GrowthSeeker.git
```

### 2. Configure `appsettings.json`

Edit the `Conn` section inside `appsettings.json`:

```json
"Conn": "Data Source=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE_NAME;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
```

Also add the `Key` for JWT Token if needed:

```json
"Key": "your_jwt_token_key_here"
```

### 3. Restore Dependencies

If you're using Visual Studio, dependencies are usually restored automatically.  
If not, you can run this manually:

```bash
dotnet restore
```

### 4. Apply Migration

Go to:  
`Tools > NuGet Package Manager > Package Manager Console`  
Then run the following command:

```bash
Update-Database
```

---

Now you’re all set! Happy coding 💻🔥
