# RUNE GAMES LEADERBOARD CASE

## Tech Stack / Libraries:
- **.NET**
- **PostgreSQL**
- **Redis**
- **Dapper**
- **Hangfire**

SQL scripts can be found at: `/Data/Postgres/Migrations/1.sql`

## Scoring and Ranking System:

Players submit their score to the `/api/Leaderboard/MatchResult` endpoint.

- If a player has a high score or it is their first score, the new score is saved to the **LeaderboardScores** table.
- The player's new rank is calculated and retrieved from the **LeaderboardRankingsView** view.
- The **LeaderboardScores** table has a composite index for the **Score** and **CreatedAt** columns.
- If two players have the same score, the player with the oldest record will have a higher rank.

Player rank data can be retrieved/calculated in three ways:

1. **LeaderboardRankingsView** on PostgreSQL:
   - Calculates the rank in real-time.
   - Used by `/api/Leaderboard/MatchResult` to calculate and return the player's new rank immediately.

2. **Redis** (Holds top 500 ranking data by default):
   - Refreshed after a new player enters the top 500 or every 5 minutes by a Hangfire job.
   - Used by `/api/Leaderboard/GetRankByRange` and `/api/Leaderboard/GetMyRanking` endpoints when cache hits.

3. **LeaderboardRankingsMat** (Materialized View on PostgreSQL):
   - Refreshed every 5 minutes by a Hangfire job.
   - Used by `/api/Leaderboard/GetRankByRange` and `/api/Leaderboard/GetMyRanking` endpoints in case of cache misses.
   - If Redis fails to retrieve cached data, it is retrieved from this view.

Player rankings are only calculated on PostgreSQL; although a SortedSet is used on Redis, Redis is only for caching.

## Scalability and Performance:
### Kubernetes:
- **PostgreSQL:**
  - PostgreSQL can be deployed using a StatefulSet in Kubernetes.
  - Read replicas can be utilized to distribute read requests, allowing the primary database to focus on write operations.
  
- **Redis:**
  - Similar to PostgreSQL, StatefulSets can be used to deploy Redis on Kubernetes.
  - During high traffic conditions, this will enhance read performance and data availability.

### Batch Processing: 
- During peak traffic times, new score data can be buffered in a message queue (Kafka, RabbitMQ, etc.), and a Hangfire job can be scheduled to process these buffered scores at regular intervals.

## Error Handling and Data Consistency:
- If the saving of new score data to PostgreSQL fails, a new job can be created in Hangfire to retry the operation until it succeeds.
- PostgreSQL serves as the primary source of ranking data, while Redis continuously replicates this data.
- If a new score cannot be saved in PostgreSQL, it will not be reflected in Redis, ensuring that PostgreSQL remains the single source of truth and preventing any data consistency issues.

## Security:
- The `/api/Leaderboard/MatchResult` endpoint is secured using a JWT token.
- This endpoint typically uses the **MatchResultService**, but a second service, **MatchResultEncryptedService** is created for production.
  - When a player submits their score to this endpoint, the game client encrypts the data using the AES algorithm with a key known only to the server and the game client. This ensures that even if the server endpoints are discovered, only the game client can send score data to them.

## Monitoring and Logging:
- I used **Serilog** to log every exception that occurs, but this is just standard practice. 
- Serilog should be integrated with a modern log management solution to track logs effectively.
- Message channels can be opened on Slack, Discord, etc., to log critical errors and performance metrics.
  - Specifically, the duration of real-time rank calculations performed by the view can be logged to a performance channel for monitoring.
- The Hangfire Dashboard is enough for monitoring Redis and materialized view refreshing jobs.

## Notes:
- When using a JWT token to authorize on the Swagger UI, remember to add the Bearer prefix to the token.
