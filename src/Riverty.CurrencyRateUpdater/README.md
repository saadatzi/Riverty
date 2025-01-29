**2. `Riverty.CurrencyRateUpdater` (Console App)**

# ScheduledRateUpdater

## Overview

`ScheduledRateUpdater` is a .NET console application that periodically updates exchange rate data in a PostgreSQL database. It uses a `BackgroundService` to run a scheduled task that fetches the latest exchange rates from the `ExchangeRateCalculator`'s `CurrencyService` and stores them in the database using Entity Framework Core.

## Features

-   Uses a `BackgroundService` to run a scheduled task.
-   Fetches exchange rates from the `CurrencyService` in the `ExchangeRateCalculator` project.
-   Stores exchange rates in a PostgreSQL database using Entity Framework Core.
-   Configurable schedule via a cron expression in `appsettings.json`.
-   Uses `IHttpClientFactory` for making HTTP requests.
-   Logs information and errors using `ILogger`.

## Prerequisites

-   .NET 9
-   PostgreSQL database server
-   A Fixer.io API key (set in `appsettings.json`)

## Setup

1. **Database:**
    -   Create a PostgreSQL database (e.g., named `Riverty`).
    -   Update the `PostgreSQLConnection` connection string in `appsettings.json` with your database credentials.
2. **API Key:**
    -   Set the `FixerApiKey` in `appsettings.json` to your Fixer.io API key.
3. **Build:**
    -   Build the solution using `dotnet build`.
4. **Run Migrations:**
    -   Navigate to the `ScheduledRateUpdater` project directory in your terminal.
    -   Run `dotnet ef database update` to apply the Entity Framework Core migrations and create the database tables.

## Running the Application

1. Navigate to the `Riverty.CurrencyRateUpdater` project directory in your terminal.
2. Run the application using `dotnet run`.

The application will start the `BackgroundService`, which will periodically fetch and update exchange rates according to the cron schedule defined in `appsettings.json`.

## Configuration

-   **`Scheduler:CronSchedule`:**  A cron expression that defines the schedule for running the rate update task (default: `0 0 * * *` - runs daily at midnight).
-   **`ConnectionStrings:PostgreSQLConnection`:** The connection string for your PostgreSQL database.
-   **`FixerApiKey`:** Your Fixer.io API key.
-   **`FixerBaseUrl`:** The base URL for the Fixer.io API.

## Logging

The application uses `ILogger` to log information and errors. You can configure logging providers in your `Program.cs` or `appsettings.json` to output logs to the console, files, or other destinations.