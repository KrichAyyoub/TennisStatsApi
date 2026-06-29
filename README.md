# Tennis Stats API (.NET 10 / ASP.NET Core)

This project is a complete REST API built from scratch using **.NET 10 (ASP.NET Core / C#)** to expose and manage tennis player statistics. The application loads an initial dataset in memory upon startup and allows thread-safe CRUD operations.

---

## 🛠️ Tech Stack

- **Framework**: .NET 10.0 (ASP.NET Core Web API)
- **Language**: C# 14
- **API Documentation**: OpenAPI / Swagger via `Swashbuckle.AspNetCore`
- **Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing (for E2E integration testing)

---

## 📁 Project Architecture

The project follows a clean, decoupled layered architecture:

```
TennisStats.slnx                       # .NET 10 Solution file (XML format)
global.json                            # Pins the .NET SDK version to 10.0.203
nuget.config                           # NuGet configuration limiting feeds to official nuget.org
TennisStats.Api/                       # Main Web API project
  ├── Controllers/                     # HTTP Controllers (PlayersController, StatsController, HealthController)
  ├── Models/                          # POCO Data Models (Player, CountryInfo, PlayerData)
  ├── DTOs/                            # Data Transfer Objects (Requests/Responses)
  ├── Services/                        # Business Logic (sorting, average BMI, median height, country win ratios)
  ├── Repositories/                    # In-memory thread-safe data access (InMemoryPlayerRepository)
  ├── Exceptions/                      # Custom Exceptions (PlayerNotFoundException, ValidationException)
  ├── Middleware/                      # Global Exception Handling Middleware
  ├── data/
  │    └── players.json                # Seed dataset JSON file
  └── Program.cs                       # App entry point, dependency injection, and middleware configuration
TennisStats.Tests/                     # Testing project
  ├── Services/                        # Unit tests for PlayerService
  ├── Controllers/                     # Unit tests for PlayersController
  └── IntegrationTests/                # End-to-end integration tests (WebApplicationFactory)
```

---

## 🚀 Running Locally

### Prerequisites
- .NET 10.0 SDK installed (`dotnet --version` should display `10.0.x`).

### Running the API
1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
2. Run the API:
   ```bash
   dotnet run --project TennisStats.Api --urls "http://localhost:5000"
   ```
3. The application will start and expose:
   - The REST API: `http://localhost:5000`
   - Interactive Swagger UI: `http://localhost:5000/swagger`

---

## 🧪 Running Tests

To run the complete suite of 25 unit and integration tests:
```bash
dotnet test
```

---

## 📞 API Endpoints & Curl Examples

### 1. Healthcheck
- **Method**: `GET`
- **Route**: `/health`
- **Description**: Returns the API health status.
- **Request**:
  ```bash
  curl -s http://localhost:5000/health
  ```
- **Response**:
  ```json
  {"status":"ok"}
  ```

---

### 2. Retrieve All Players (Sorted by points descending)
- **Method**: `GET`
- **Route**: `/api/players`
- **Query Parameters** (Optional): `sex` (values: `M` or `F`)
- **Request**:
  ```bash
  curl -s http://localhost:5000/api/players
  ```
- **Response** (extract):
  ```json
  [
    {
      "id": 102,
      "firstname": "Serena",
      "lastname": "Williams",
      "shortname": "S.WIL",
      "sex": "F",
      "country": { "picture": "https://tenisu.latelier.co/resources/USA.png", "code": "USA" },
      "picture": "https://tenisu.latelier.co/resources/Serena.png",
      "data": { "rank": 10, "points": 3521, "weight": 72000, "height": 175, "age": 37, "last": [0, 1, 1, 1, 0] }
    }
  ]
  ```

---

### 3. Retrieve Global Statistics
- **Method**: `GET`
- **Route**: `/api/players/stats` *(or via the alias `/api/stats`)*
- **Request**:
  ```bash
  curl -s http://localhost:5000/api/players/stats
  ```
- **Response**:
  ```json
  {
    "countryWithBestWinRatio": { "countryCode": "SRB", "winRatio": 1 },
    "averageBMI": 23.36,
    "medianHeight": 185
  }
  ```

---

### 4. Retrieve a Player by ID
- **Method**: `GET`
- **Route**: `/api/players/{id}`
- **Request**:
  ```bash
  curl -s http://localhost:5000/api/players/17
  ```
- **Response**:
  ```json
  {
    "id": 17,
    "firstname": "Rafael",
    "lastname": "Nadal",
    "shortname": "R.NAD",
    "sex": "M",
    "country": { "picture": "https://tenisu.latelier.co/resources/Espagne.png", "code": "ESP" },
    "picture": "https://tenisu.latelier.co/resources/Nadal.png",
    "data": { "rank": 1, "points": 1982, "weight": 85000, "height": 185, "age": 33, "last": [1, 0, 0, 0, 1] }
  }
  ```

- **Unknown ID Error (404)**:
  ```bash
  curl -i http://localhost:5000/api/players/9999
  ```
  ```json
  {"statusCode":404,"error":"Not Found","message":"Player with id 9999 was not found."}
  ```

---

### 5. Retrieve a Player's Win Ratio
- **Method**: `GET`
- **Route**: `/api/players/{id}/win-ratio`
- **Request**:
  ```bash
  curl -s http://localhost:5000/api/players/65/win-ratio
  ```
- **Response**:
  ```json
  { "id": 65, "winRatio": 0.8 }
  ```

---

### 6. Create a Player
- **Method**: `POST`
- **Route**: `/api/players`
- **Request**:
  ```bash
  curl -i -X POST http://localhost:5000/api/players -H "Content-Type: application/json" -d "{
    \"firstname\": \"Roger\",
    \"lastname\": \"Federer\",
    \"shortname\": \"R.FED\",
    \"sex\": \"M\",
    \"countryCode\": \"SUI\",
    \"countryPicture\": \"https://tenisu.latelier.co/resources/Suisse.png\",
    \"picture\": \"https://example.com/federer.png\",
    \"rank\": 5,
    \"points\": 2000,
    \"weight\": 85000,
    \"height\": 185,
    \"age\": 41,
    \"last\": [1, 1, 0, 1, 1]
  }"
  ```
- **Response** (`201 Created` with a `Location` header pointing to `/api/players/{id}`):
  ```json
  {"id":103,"firstname":"Roger","lastname":"Federer","shortname":"R.FED","sex":"M","country":{"picture":"https://tenisu.latelier.co/resources/Suisse.png","code":"SUI"},"picture":"https://example.com/federer.png","data":{"rank":5,"points":2000,"weight":85000,"height":185,"age":41,"last":[1,1,0,1,1]}}
  ```

---

### 7. Delete a Player
- **Method**: `DELETE`
- **Route**: `/api/players/{id}`
- **Request**:
  ```bash
  curl -i -X DELETE http://localhost:5000/api/players/103
  ```
- **Response**:
  `HTTP/1.1 204 No Content`

---

## ⚙️ Technical Choices & Business Rules

1. **InMemory & Thread-safety**:
   - The data is kept in-memory in a `List<Player>` wrapped with a `lock` statement in `InMemoryPlayerRepository`. This guarantees that concurrent writes/reads won't cause collection corruption or `InvalidOperationException`.
2. **Average BMI Calculation**:
   - Weight is supplied in grams and height in centimeters.
   - They are converted to kg (`weight / 1000`) and meters (`height / 100`) before applying the formula: `BMI = kg / m²`.
   - The average BMI is rounded to 2 decimal places.
3. **Median Height Calculation**:
   - If the sorted list of heights has an odd count, we return the middle element.
   - If the count is even, we return the average of the two middle elements.
4. **Country Win Ratio & Ties**:
   - All matches (`last`) of all players belonging to the same country are aggregated.
   - Win ratio is computed as: `Wins (1) / Total matches`.
   - If multiple countries tie for the best win ratio, the winner is decided alphabetically based on the country code (e.g., "SUI" before "USA").

---

## 🌐 Cloud Deployment

This project is ready to be deployed on PaaS providers like **Render**, **Railway** (using native .NET buildpack), or **Azure App Service**:
- Standard, non-containerized .NET 10 deployment.
- The `dotnet publish -c Release` command builds the ready-to-run release assets.
- Swagger UI documentation is enabled in production on `/swagger` for easy testing.
- Live App URL (Demo Example): `https://tennis-stats-api-demo.azurewebsites.net/swagger` *(Placeholder)*

---

## 🔮 Future Improvements

1. **Authentication**: Add JWT or API Key authentication to protect write/delete operations in production.
2. **Database Integration**: Swap `InMemoryPlayerRepository` with a real database context using Entity Framework Core (e.g., PostgreSQL) by implementing the `IPlayerRepository` interface.
3. **Pagination**: Introduce `page` and `pageSize` parameters on query endpoints to handle large collections of players.
4. **Enriched Validation**: Integrate FluentValidation for clean separate payload validation.
