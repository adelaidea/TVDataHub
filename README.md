# TVDataHub

TVDataHub is a modern, scalable application designed to aggregate and manage TV show data.

## Project Structure

The solution follows a clean architecture approach with the following components:

```
src/
├── TVDataHub.Api/           # Web API layer
├── TVDataHub.Core/          # Application core with business logic and contracts
├── TVDataHub.DataAccess/    # Data access, persistence and scraping services
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

At the Project startup the migrations will be applyed and the application will be ready to be used.
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
- SyncTVShowsToBeUpsertedJob: Executed every 12 hours to get the updates and map what is outdated and needs to be    feteched again
- SyncUpdatedTVShowsJob: Connected to a queue that will be trigered when there's something new to be updated
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

- All Shows are present in the `updates/shows` API, even the new ones that wasn't updated
- Person updates trigger updates to the Show's `updated` attribute, which is used to synchronize data in our database.
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
This approach involves two distinct phases:

1. Initial Data Load:
   - Perform a one-time bulk load of all TV shows and their cast from TVMaze API
   - Store the initial state in the database

2. Incremental Inserts
   - Job that runs every day to track the last `TVShowId` to get new TVShows using the `\shows?page=` API and persist TVShow information 
   - New ShowId will be added to a Queue that will trigger a Job to get Cast information
   - When triggered because a new Show was added, use the `shows/{showId}/cast` API to fetch it's `Cast` and persist Cast/Person information
   
2. Incremental Updates:
   - Job that execute twice a day, using the daily updates `updates/shows?since=day` API to get TVShow updates - with the assuption that a change in any Cast member will trigger updates to the Show's `updated` attribute - 
   - Get the updated values from local storage and compere the `updated` values.
   - Use the `shows/{showId}?embed=cast` API to get the updated TVShows with the embeded cast and update the TVShow and Person local values

Advantages:
- Reduced API calls after initial load
- Lower bandwidth usage
- Less strain on TVMaze API

Disadvantages:
- Initial load can be time-consuming
- More API calls required
- More complex with more jobs accessing difrent APIs to mantain 
- May miss updates if an issue ocurr during daily checks for updates

### Option 2: Full Data Synchronization
This approach focuses on complete data synchronization through regular full updates:

1. Periodic Full Sync:
  - Job that execute twice a day to fetch all TV shows updates using `updates/shows` API - with the assuption that a change in any Cast member will trigger updates to the Show's `updated` attribute - 
    - Assume that this API will return all Shows, new ones and updated ones
  - Get the updated values from local storage and compere the `updated` values.
  - If it's not present in the existing database, consider it as new
  - Add the values to upsert in a Queue

2. Change Detection:
  - A Job that is triggered when new values are added to the be upserted in the Queue
  - Use the `shows/{showId}?embed=cast` API to get the updated TVShows with the embeded cast and update the TVShow and  Person local values  
  - Use database transactions to ensure atomic updates

Advantages:
- Complete data synchronization in each cycle
- Simpler implementation logic
- Less Jobs and rules to mantain

Disadvantages:
- First execution can be time-consuming
- Higher bandwidth usage
- Longer sync cycles

### Selected Approach
The current implementation follows Option 2 (Full Data Synchronization) because:
- TVMaze API payload size is manageable (<1MB)
- Simpler to maintain and debug
- Reduces complexity in handling edge cases