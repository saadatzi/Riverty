**3. `CurrencyApi` (Minimal API)**

# CurrencyApi

## Overview

`CurrencyApi` is a .NET Minimal API that provides endpoints for accessing currency exchange rate data. It exposes two main endpoints:

-   `/rates`: Retrieves the latest or historical exchange rates from the Fixer.io API (using the `CurrencyService` from the `ExchangeRateCalculator` project).
-   `/historical-rates/{currencyCode}`: Retrieves historical exchange rates for a specific currency and date range from a PostgreSQL database.

## Features

-   Minimal API implementation using ASP.NET Core.
-   Exposes endpoints for latest and historical exchange rates.
-   Integrates with the `CurrencyService` from the `ExchangeRateCalculator` project.
-   Uses Entity Framework Core to access data from a PostgreSQL database.
-   Provides API documentation via Swagger/OpenAPI.
-   Accepts date inputs in `yyyy-MM-dd` format (UTC).

## Prerequisites

-   .NET 9
-   PostgreSQL database server
-   A Fixer.io API key (set in `appsettings.json`)

## Setup

1. **Database:**
    -   Ensure that the PostgreSQL database specified in your `appsettings.json` (`ConnectionStrings:PostgreSQLConnection`) exists and is accessible.
    -   The `ScheduledRateUpdater` project should be set up and running to populate the database with exchange rate data.
2. **API Key:**
    -   Set the `FixerApiKey` in `appsettings.json` to your Fixer.io API key.
3. **Build:**
    -   Build the solution using `dotnet build`.

## Running the Application

1. Navigate to the `CurrencyApi` project directory in your terminal.
2. Run the application using `dotnet run`.

The API will start, and you can access the Swagger UI at `http://localhost:5000/swagger` (or the appropriate URL based on your `launchSettings.json` configuration).

## API Endpoints

### `/rates`

-   **Method:** `GET`
-   **Parameters:**
    -   `rateType` (query, required): `Latest` or `Historical`.
    -   `date` (query, optional): Date in `yyyy-MM-dd` format for historical rates.
-   **Returns:**
    -   Latest or historical exchange rates from the Fixer.io API.

### `/historical-rates/{currencyCode}`

-   **Method:** `GET`
-   **Parameters:**
    -   `currencyCode` (path, required): 3-letter currency code (e.g., `USD`, `EUR`, `CAD`).
    -   `startDate` (query, required): Start date in `yyyy-MM-dd` format (UTC).
    -   `endDate` (query, required): End date in `yyyy-MM-dd` format (UTC).
-   **Returns:**
    -   Historical exchange rates for the specified currency and date range from the database.

## Configuration

-   **`ConnectionStrings:PostgreSQLConnection`:** The connection string for your PostgreSQL database.
-   **`FixerApiKey`:** Your Fixer.io API key.
-   **`FixerBaseUrl`:** The base URL for the Fixer.io API.