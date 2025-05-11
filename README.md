# TVDataHub

TVDataHub is a modern, scalable application designed to aggregate and manage TV show data.

## Project Structure

The solution follows a clean architecture approach with the following components:

```
src/
├── TVDataHub.Api/           # Web API layer
├── TVDataHub.Core/          # Application core with business logic and contracts
├── TVDataHub.DataAccess/    # Data access and persistence
└── TVDataHub.Scraper/       # Data scraping services
```

## Features

- RESTful API for TV show data management
- Data scraping capabilities
- PostgreSQL database integration
- Docker containerization
- Clean Architecture implementation
- Domain-Driven Design principles

## Technology Stack

- .NET 8
- PostgreSQL 16
- Docker & Docker Compose
- PgAdmin 4

## Getting Started

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK
- dotnet cli and dotnet ef extensions
- Git

### Running the Application

1. Clone the repository:
```bash
git clone https://github.com/adelaide/TVDataHub.git
cd TVDataHub
```

2. Start the application using Docker Compose:

```bash
docker compose up -d
```

The application will be available at:

- API: http://localhost:5001
- PgAdmin: http://localhost:8080
  - Email: admin@tvdatahub.com
  - Password: admin123


3. Creating a new Migration 

If required, the folowing command can be used to create a new migration to configure the database:

```bash
dotnet ef migrations add InitialCreate --project src/TVDataHub.DataAccess --startup-project src/TVDataHub.Api
```

### Database Connection

- Host: localhost
- Port: 5432
- Database: tvdatahub
- Username: root
- Password: {configured at docker-compose.yml}

## Architecture & Design Decisions

### Clean Architecture Implementation

The solution follows a strict layered architecture with clear separation of concerns:

- TVDataHub.Api: Web API layer handling HTTP requests
- TVDataHub.Core: Business logic and use cases
- TVDataHub.DataAccess: Data persistence layer
- TVDataHub.Scraper: External data scraping services from TVMaze Api 

### API Design

- RESTful API implementation using ASP.NET Core 8
- Swagger/OpenAPI integration for API documentation
- Example endpoint: GET /api/TVShows with pagination support
- DTOs are used for data transfer between layers

### Background Jobs & Data Synchronization

Three main background services for data synchronization:
- SyncNewTVShowsJob: Daily sync for new TV shows
- SyncTVShowsCastJob: Daily sync for cast information
- SyncUpdatedTVShowsJob: 12-hour sync for updated shows
- Jobs use dependency injection and proper scoping
- Logging is implemented throughout the jobs


### Exception Handling

- Application-level exception handling in use cases
- Proper logging of exceptions with context
- Custom exception handling in the application layer.


### Database Integration
- PostgreSQL database integration
- Entity Framework Core for data access
- Automatic database migrations on startup
- Repository pattern implementation


### Dependency Injection

- Proper use of DI throughout the application
- Service registration through extension methods
- Scoped lifetime for use cases
- Background services registration

### Logging & Monitoring

- Structured logging implementation
- Detailed logging in background jobs

## Testing

The project includes a comprehensive unit-test implementation.

Run tests using:

```bash
dotnet test
```

## Assumptions

- The application assumes a PostgreSQL database for data persistence.
-  Docker is used for containerization and to simplify the development environment. 
-  The system is designed to handle concurrent requests and be able to handle with the external API rate limiting by using retry policy. 
-  Data scraping operations are performed asynchronously.
- TVShow and Cast data update operations are done with all data without priodicity filter to simplify the operation. The decision was based on the current payload size(<1mb).

### TV Maze API Assumptions

- A update in the Cast will trigger a update in the `updated` attribute of the Show, then the information will be udpated in the database. 


## Future Improvements

- Implement API Authentication and Authorization
- Implement caching layer
- Add monitoring and logging visualization 
- Implement CI/CD pipeline


