CREATE TABLE "Players"
(
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(64) NOT NULL,
    "DeviceId" VARCHAR(255) NOT NULL, 
    "PasswordSalt" BYTEA NOT NULL,               
    "PasswordHash" BYTEA NOT NULL                
);

CREATE TABLE "LeaderboardScores"
(
    "PlayerId" INTEGER NOT NULL PRIMARY KEY,
    "Score" INTEGER NOT NULL CHECK ("Score" >= 0),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP, 
    CONSTRAINT "FK_Player_LeaderboardScore" 
        FOREIGN KEY ("PlayerId") REFERENCES "Players" ("Id")
);

CREATE INDEX "Score_CreatedAt_IX" ON "LeaderboardScores" ("Score", "CreatedAt");

CREATE MATERIALIZED VIEW "LeaderboardRankingsMat" AS
SELECT
    "PlayerId",
    ROW_NUMBER() OVER (ORDER BY "Score" DESC, "CreatedAt" ASC) AS "Rank"
FROM
    "LeaderboardScores";

CREATE UNIQUE INDEX "Player_UX" ON "LeaderboardRankingsMat" ("PlayerId");


CREATE VIEW "LeaderboardRankingsView" AS
SELECT 
    "PlayerId",
    ROW_NUMBER() OVER (ORDER BY "Score" DESC, "CreatedAt" ASC) AS "Rank"
FROM 
    "LeaderboardScores";