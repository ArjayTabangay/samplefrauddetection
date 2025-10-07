# Lead Fraud Detection API

A comprehensive fraud detection system for lead validation using rule-based engines and machine learning models.

## Project Structure

```
LeadFraudDetection.Api/
├── Controllers/           # REST API endpoints
│   ├── LeadsController.cs
│   ├── BlacklistController.cs
│   └── AuthController.cs
├── Services/
│   ├── Rules/            # Rule-based fraud detection
│   │   ├── IRuleEngine.cs
│   │   └── RuleEngine.cs
│   ├── ML/               # Machine learning fraud detection
│   │   ├── IMLFraudDetectionService.cs
│   │   ├── MLFraudDetectionService.cs
│   │   └── LeadData.cs
│   ├── Blacklist/        # Blacklist management
│   │   ├── IBlacklistService.cs
│   │   └── BlacklistService.cs
│   ├── IFraudDetectionService.cs
│   └── FraudDetectionService.cs
├── Data/                 # Entity Framework DbContext
│   └── FraudDetectionDbContext.cs
├── Auth/                 # JWT authentication
│   ├── JwtSettings.cs
│   └── JwtTokenService.cs
└── Models/               # Data models
    ├── Lead.cs
    ├── FraudScore.cs
    ├── BlacklistEntry.cs
    └── AuditLog.cs

LeadFraudDetection.Tests/
└── Services/             # Unit tests
    ├── BlacklistServiceTests.cs
    └── RuleEngineTests.cs

.github/
└── workflows/
    └── ci-cd.yml         # CI/CD pipeline
```

## Technology Stack

| Layer | Purpose | Technology |
|-------|---------|------------|
| API | REST endpoints, JWT auth | ASP.NET Core (.NET 9) |
| ML Model | Fraud scoring/prediction | ML.NET (Random Forest, LightGBM) |
| Rule Engine | Blacklist, pattern logic | C# |
| Data Store | Lead/score/audit/model/meta | SQL Server |
| Logging | Diagnostics, monitoring | Serilog, App Insights |

## Features

### 1. Rule-Based Detection
- **Blacklist Checking**: Email, IP, Phone validation
- **Pattern Analysis**: Disposable email detection, suspicious name patterns
- **Behavioral Analysis**: User agent validation, email addressing patterns

### 2. Machine Learning Detection
- **ML.NET Integration**: Random Forest and LightGBM algorithms
- **Feature Engineering**: Text featurization of lead attributes
- **Model Training**: Train on historical lead data
- **Prediction**: Real-time fraud probability scoring

### 3. Authentication & Authorization
- **JWT Authentication**: Secure token-based authentication
- **Role-Based Access Control**: Admin and User roles
- **Secure Endpoints**: Protected API endpoints

### 4. Data Management
- **Entity Framework Core**: SQL Server integration
- **Audit Logging**: Track all fraud detection activities
- **Score History**: Maintain fraud score history

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB for development)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone https://github.com/ArjayTabangay/samplefrauddetection.git
cd samplefrauddetection
```

2. Restore dependencies:
```bash
dotnet restore LeadFraudDetection.sln
```

3. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeadFraudDetectionDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

4. Run database migrations (when created):
```bash
cd LeadFraudDetection.Api
dotnet ef database update
```

5. Run the application:
```bash
dotnet run --project LeadFraudDetection.Api
```

### Testing

Run all tests:
```bash
dotnet test LeadFraudDetection.sln
```

Run with detailed output:
```bash
dotnet test LeadFraudDetection.sln --logger "console;verbosity=detailed"
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login and get JWT token

### Leads
- `GET /api/leads` - Get all leads (requires authentication)
- `GET /api/leads/{id}` - Get specific lead
- `POST /api/leads` - Create new lead (triggers fraud detection)
- `POST /api/leads/{id}/check-fraud` - Re-run fraud detection
- `DELETE /api/leads/{id}` - Delete lead

### Blacklist (Admin only)
- `GET /api/blacklist` - Get all blacklist entries
- `GET /api/blacklist/check?type=Email&value=test@example.com` - Check if blacklisted
- `POST /api/blacklist` - Add blacklist entry
- `DELETE /api/blacklist/{id}` - Remove blacklist entry

## Authentication

### Test Credentials

**Admin User:**
- Email: `admin@example.com`
- Password: `Admin123!`
- Role: Admin

**Regular User:**
- Email: `user@example.com`
- Password: `User123!`
- Role: User

### Example Login Request:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}'
```

### Using JWT Token:
```bash
curl -X GET http://localhost:5000/api/leads \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Configuration

### JWT Settings (`appsettings.json`)
```json
"JwtSettings": {
  "Secret": "YourVeryLongSecretKeyForJWTTokenGeneration123456789",
  "Issuer": "LeadFraudDetection.Api",
  "Audience": "LeadFraudDetection.Client",
  "ExpirationMinutes": 60
}
```

### Serilog Configuration
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" }
  ]
}
```

## Fraud Detection Rules

### Blacklist Rules (Score: 100)
- Blacklisted Email
- Blacklisted IP Address
- Blacklisted Phone Number

### Pattern Rules
- Disposable Email (Score: 50)
- Suspicious Name Pattern (Score: 30)
- Repeating Pattern in Email (Score: 20)

### Behavioral Rules
- Missing User Agent (Score: 15)
- Missing Company (Score: 10)
- Email Plus Addressing (Score: 10)

### Scoring Threshold
- **Final Score >= 70**: Marked as fraudulent
- **Calculation**: (Rule Score × 0.6) + (ML Score × 0.4)

## CI/CD Pipeline

The project includes a GitHub Actions workflow that:
1. Builds the solution on push/PR
2. Runs all unit tests
3. Checks code quality
4. Publishes artifacts on main branch

## Development

### Adding New Rules
1. Implement rule logic in `RuleEngine.cs`
2. Add corresponding tests in `RuleEngineTests.cs`
3. Update documentation

### Training ML Model
The ML model can be trained using historical lead data:
```csharp
var leads = await _context.Leads.ToListAsync();
await _mlService.TrainModelAsync(leads);
```

## License

This project is licensed under the MIT License.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request