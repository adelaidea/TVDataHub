# TVDataHub

TVDataHub is a modern, scalable application designed to aggregate and manage TV show data.

## Project Structure

The solution follows a clean architecture approach with the following components:

```
src/
├── TVDataHub.Api/           # Web API layer
├── TVDataHub.Core/          # Application core with business logic and contracts
├── TVDataHub.DataAccess/    # Data access, persistence and scraping service
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

At the Project startup the migrations will be applied and the application will be ready to be used.
The application will be available at:

- API: http://localhost:5276
- PgAdmin: http://localhost:8081
  - Email: admin@tvdatahub.com
  - Password: admin123


3. Creating a new Migration 

If required, the folowing command can be used to create a new migration to configure the database:

```bash
dotnet ef migrations add [MigrationName] --project src/TVDataHub.DataAccess --startup-project src/TVDataHub.Api
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
- TVDataHub.DataAccess: Data persistence layer and External data scraping services from TVMaze Api 

### API Design

- RESTful API implementation using ASP.NET Core 8
- Swagger/OpenAPI integration for API documentation
- Example endpoint: GET /api/TVShows with pagination support
- DTOs are used for data transfer between layers

### Background Jobs & Data Synchronization

Two main background services for data synchronization:
- SyncTVShowsToBeUpsertedJob: Executed every 12 hours to identify outdated records that need to be refreshed
- SyncUpdatedTVShowsJob: Processes items from a queue that is populated when new updates are available
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

The project includes a comprehensive unit-test and acceptance test implementation.

Run tests using:

```bash
dotnet test
```

## Assumptions

- The application uses PostgreSQL as its primary database for data persistence.
- Docker and Docker Compose are used for containerization and development environment setup.
- The system implements concurrent request handling with retry policies to manage external API rate limiting.
- All data scraping operations are performed asynchronously to maintain system responsiveness.
- TVShow and Cast data updates are performed in bulk without periodicity filters, this decision was taken considering the actual payload sizes.
- Cast data updates follow an upsert pattern: new Person entities are created if they don't exist, while existing Person entities only receive new TVShow relationships or remove TVShow relatioships.
- The API endpoints for listing TVShows and Characters implement pagination with a default page size of 10 items. The total count of records is included in the response to enable clients to implement complete data retrieval strategies.

### TV Maze API Assumptions

- All Shows are present in the `updates/shows` API, including new shows that haven't been updated
- When a Person (cast member) is updated, the associated Show's `updated` attribute is also updated, which we use for synchronization
- Character information is not persisted in the current implementation. The many-to-many relationship between Person and TVShow entities can be extended in the future to include character-specific information if required.


## Future Improvements

- Implement API Authentication and Authorization
- Implement caching layer
- Add monitoring and logging visualization 
- Implement CI/CD pipeline
- Change repository concerns of when updating `Person` and `TVShow` 
- Use UoW in the use case

## Considered Solutions

### Option 1: Initial Seed with Incremental Inserts and Updates
This approach involves 3 distinct phases:

1. Initial Data Load:
   - Perform a one-time bulk load of all TV shows and their cast from TVMaze API
   - Store the initial state in the database

2. Incremental Inserts
   - Job that runs daily to track the last `TVShowId` and fetch new TVShows using the `\shows?page=` API
   - New ShowIds are added to a Queue that triggers a Job to fetch Cast information
   - When triggered for a new Show, use the `shows/{showId}/cast` API to fetch and persist Cast/Person information
     
3. Incremental Updates:
   - Job that executes twice daily, using the `updates/shows?since=day` API to get TVShow updates
   - Assumes that any Cast member changes will trigger updates to the Show's `updated` attribute
   - Compares `updated` values with local storage
   - Uses the `shows/{showId}?embed=cast` API to fetch updated TVShows with embedded cast and update local values

Advantages:
- Reduced API calls after initial load
- Lower bandwidth usage
- Less strain on TVMaze API
- Better control over the initial data load process

Disadvantages:
- Initial load can be time-consuming
- More API calls required
- More complex with multiple jobs accessing different APIs to maintain
- May miss updates if an issue occurs during daily update checks
- Requires additional state tracking for the last processed show ID
- More complexity across multiple API calls
- Higher risk of data inconsistency due to distributed updates
- More complex error recovery scenarios

### Option 2: Full Data Synchronization
This approach focuses on complete data synchronization through regular full updates:

1. Periodic Full Sync:
   - Job that executes twice daily to fetch all TV shows updates using `updates/shows` API
   - Assumes that any Cast member changes will trigger updates to the Show's `updated` attribute
   - Assumes this API returns all Shows, including new and updated ones
   - Compares with local storage to identify new and updated records
   - Adds values to upsert to a Queue

2. Change Detection:
   - Job triggered when new values are added to the upsert queue
   - Uses the `shows/{showId}?embed=cast` API to fetch updated TVShows with embedded cast
   - Updates TVShow and Person local values
   - Uses database transactions to ensure atomic updates

Advantages:
- Complete data synchronization in each cycle
- Simpler implementation logic
- Fewer jobs and rules to maintain
- Atomic updates through database transactions
- Simpler error handling and recovery
- Consistent data state after each sync
- Easier to implement and maintain
- Better handling of concurrent updates
- Simpler monitoring and debugging

Disadvantages:
- First execution can be time-consuming
- Higher bandwidth usage
- More pressure on TVMaze API during sync
- Higher risk of rate limiting
- Less granular control over individual show updates
- Higher memory usage during sync operations

### Selected Approach
The current implementation follows `Option 2` (Full Data Synchronization) because:
- TVMaze API payload size is manageable (<1MB)
- Simpler to maintain and debug
- Reduces complexity in handling edge cases
- Provides better data consistency guarantees
- Easier to implement and maintain
- Better handling of concurrent updates
- Simpler monitoring and debugging
- More resilient to failures