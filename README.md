# API Tennis Stats (.NET 10 / ASP.NET Core)

Ce projet est une API REST complète construite à partir de zéro avec **.NET 10 (ASP.NET Core / C#)** pour exposer et gérer des statistiques de joueurs de tennis. L'application charge un jeu de données initial en mémoire au démarrage et permet d'ajouter, de consulter, et de supprimer des joueurs de manière thread-safe.

---

## 🛠️ Stack Technique

- **Framework** : .NET 10.0 (ASP.NET Core Web API)
- **Langage** : C# 14
- **Documentation API** : OpenAPI / Swagger via `Swashbuckle.AspNetCore`
- **Tests** : xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing (pour les tests d'intégration)

---

## 📁 Architecture du Projet

Le projet respecte une architecture en couches propre et découplée :

```
TennisStats.slnx                       # Solution .NET 10 (XML Solution format)
global.json                            # Épinglage du SDK .NET 10.0.203
nuget.config                           # Configuration NuGet pour limiter les packages au flux officiel
TennisStats.Api/                       # Projet Web API principal
  ├── Controllers/                     # Contrôleurs HTTP (PlayersController, StatsController, HealthController)
  ├── Models/                          # Modèles de données POCO (Player, CountryInfo, PlayerData)
  ├── DTOs/                            # Objets de transfert de données (Requêtes/Réponses)
  ├── Services/                        # Logique métier (tri, calcul IMC moyen, médiane, win ratio par pays)
  ├── Repositories/                    # Accès aux données en mémoire (InMemoryPlayerRepository - thread-safe)
  ├── Exceptions/                      # Exceptions personnalisées (PlayerNotFoundException, ValidationException)
  ├── Middleware/                      # Middleware de capture globale des exceptions
  ├── data/
  │    └── players.json                # Fichier de données initiales (Seed)
  └── Program.cs                       # Point d'entrée de l'application & configuration des services
TennisStats.Tests/                     # Projet de Tests Unitaires et d'Intégration
  ├── Services/                        # Tests unitaires pour PlayerService
  ├── Controllers/                     # Tests unitaires pour PlayersController
  └── IntegrationTests/                # Tests d'intégration de bout en bout (WebApplicationFactory)
```

---

## 🚀 Lancement Local

### Prérequis
- SDK .NET 10.0 installé (`dotnet --version` doit afficher `10.0.x`).

### Lancement de l'API
1. Restaurez les packages NuGet :
   ```bash
   dotnet restore
   ```
2. Lancez l'API en local :
   ```bash
   dotnet run --project TennisStats.Api --urls "http://localhost:5000"
   ```
3. L'application démarre et expose :
   - L'API REST : `http://localhost:5000`
   - La documentation interactive Swagger UI : `http://localhost:5000/swagger`

---

## 🧪 Lancement des Tests

Pour exécuter la suite complète de 25 tests unitaires et d'intégration :
```bash
dotnet test
```

---

## 📞 Endpoints API & Exemples Curl

### 1. Healthcheck
- **Méthode** : `GET`
- **Route** : `/health`
- **Description** : Retourne le statut de l'application.
- **Requête** :
  ```bash
  curl -s http://localhost:5000/health
  ```
- **Réponse** :
  ```json
  {"status":"ok"}
  ```

---

### 2. Récupérer tous les joueurs (triés par points décroissants)
- **Méthode** : `GET`
- **Route** : `/api/players`
- **Paramètres Query** (Optionnel) : `sex` (valeurs possibles : `M` ou `F`)
- **Requête** :
  ```bash
  curl -s http://localhost:5000/api/players
  ```
- **Réponse** (extrait) :
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

### 3. Récupérer les statistiques globales
- **Méthode** : `GET`
- **Route** : `/api/players/stats` *(ou via l'alias `/api/stats`)*
- **Requête** :
  ```bash
  curl -s http://localhost:5000/api/players/stats
  ```
- **Réponse** :
  ```json
  {
    "countryWithBestWinRatio": { "countryCode": "SRB", "winRatio": 1 },
    "averageBMI": 23.36,
    "medianHeight": 185
  }
  ```

---

### 4. Récupérer un joueur par ID
- **Méthode** : `GET`
- **Route** : `/api/players/{id}`
- **Requête** :
  ```bash
  curl -s http://localhost:5000/api/players/17
  ```
- **Réponse** :
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

- **Erreur ID inconnu (404)** :
  ```bash
  curl -i http://localhost:5000/api/players/9999
  ```
  ```json
  {"statusCode":404,"error":"Not Found","message":"Player with id 9999 was not found."}
  ```

---

### 5. Récupérer le ratio de victoires d'un joueur
- **Méthode** : `GET`
- **Route** : `/api/players/{id}/win-ratio`
- **Requête** :
  ```bash
  curl -s http://localhost:5000/api/players/65/win-ratio
  ```
- **Réponse** :
  ```json
  { "id": 65, "winRatio": 0.8 }
  ```

---

### 6. Ajouter un joueur
- **Méthode** : `POST`
- **Route** : `/api/players`
- **Requête** :
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
- **Réponse** (`201 Created` avec en-tête `Location` pointant vers `/api/players/{id}`) :
  ```json
  {"id":103,"firstname":"Roger","lastname":"Federer","shortname":"R.FED","sex":"M","country":{"picture":"https://tenisu.latelier.co/resources/Suisse.png","code":"SUI"},"picture":"https://example.com/federer.png","data":{"rank":5,"points":2000,"weight":85000,"height":185,"age":41,"last":[1,1,0,1,1]}}
  ```

---

### 7. Supprimer un joueur
- **Méthode** : `DELETE`
- **Route** : `/api/players/{id}`
- **Requête** :
  ```bash
  curl -i -X DELETE http://localhost:5000/api/players/103
  ```
- **Réponse** :
  `HTTP/1.1 204 No Content`

---

## ⚙️ Choix Techniques & Règles Métier

1. **InMemory & Thread-safety** :
   - Le stockage en mémoire est géré par une `List<Player>` encapsulée dans un `lock` dans `InMemoryPlayerRepository`. Cela garantit qu'aucune écriture ou modification concurrente n'entraîne de corruption de données ou d'exceptions `InvalidOperationException`.
2. **Calcul de l'IMC Moyen** :
   - Les poids sont fournis en grammes et les tailles en cm dans le fichier JSON.
   - Ils sont convertis respectivement en kg (`poids / 1000`) et en mètres (`taille / 100`) avant d'appliquer la formule : `IMC = kg / m²`.
   - La moyenne des IMC individuels est ensuite arrondie à 2 décimales.
3. **Calcul de la Médiane** :
   - Si la liste triée des hauteurs a un nombre impair d'éléments, nous retournons l'élément du milieu.
   - Si le nombre d'éléments est pair, nous prenons la moyenne des deux éléments centraux.
4. **Calcul du Ratio par Pays & Gestion des Égalités** :
   - Tous les matchs (`last`) des joueurs appartenant à un même pays sont agrégés.
   - Le ratio est : `Total des victoires (1) / Total de matchs joués`.
   - Si deux pays ont exactement le même ratio de victoires, le premier pays par ordre alphabétique de son code pays (ex: "SUI" avant "USA") est choisi comme gagnant.

---

## 🌐 Déploiement Cloud

Ce projet est prêt à être déployé sur des PaaS comme **Render**, **Railway** (avec détection automatique .NET), ou **Azure App Service** :
- Le déploiement est **natif** (.NET Runtime standard).
- La commande `dotnet publish -c Release` génère l'artefact compilé prêt à l'exécution.
- La documentation interactive Swagger est activée en production à l'adresse `/swagger` pour simplifier la recette et les démos.
- URL Déployée (Exemple de démo) : `https://tennis-stats-api-demo.azurewebsites.net/swagger` *(Placeholder)*

---

## 🔮 Pistes d'Amélioration

1. **Authentification** : Ajouter une authentification API Key ou JWT pour sécuriser l'ajout/suppression de joueurs en production.
2. **Persistance réelle** : Remplacer l'implémentation `InMemoryPlayerRepository` par une implémentation Entity Framework Core (ex: PostgreSQL ou SQL Server) en tirant profit de l'injection de dépendances sur `IPlayerRepository`.
3. **Pagination** : Ajouter des paramètres `page` et `pageSize` sur l'endpoint de récupération de liste pour gérer de larges volumes de joueurs.
4. **Validation enrichie** : Intégrer FluentValidation pour séparer la logique de validation des DTOs.
