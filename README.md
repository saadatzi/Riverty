# Currency Exchange Rate Application

## Overview

This solution provides a set of applications for fetching, storing, and displaying currency exchange rates using the Fixer.io API. It consists of the following projects:

-   **ExchangeRateCalculator:** A .NET class library that provides the core logic for calculating currency exchange rates using the Fixer.io API. It also includes a console application for interactive use.
-   **CurrencyRateUpdater:** A .NET console application that runs as a background service to periodically fetch and store the latest exchange rates in a PostgreSQL database.
-   **CurrencyApi:** A .NET Minimal API that exposes endpoints for retrieving the latest and historical exchange rates from the database or Fixer.io API.
-   **Web:** An ASP.NET Core MVC application that provides a simple web UI for interacting with the CurrencyApi, displaying exchange rates, and visualizing historical trends with a chart.

## Prerequisites

-   .NET 9
-   PostgreSQL database server
-   A Fixer.io API key


## Setup and Configuration

1. **Clone the repository:**
    ```bash
    git clone https://github.com/saadatzi/Riverty
    ```
2. **Database:**
    *   Create a PostgreSQL database (e.g., named `Riverty`).
    *   Update the `PostgreSQLConnection` connection string in the `appsettings.json` files of `ExchangeRateCalculator` and `CurrencyApi` with your database credentials.
3. **API Key:**
    *   Obtain a Fixer.io API key from [https://fixer.io/](https://fixer.io/).
    *   Set the `FixerApiKey` in the `appsettings.json` files of `CurrencyRateUpdater`, `ExchangeRateCalculator`, and `CurrencyApi`.
4. **API URL (WebUI):**
    *   Update the `ApiSettings:BaseUrl` in `Web/appsettings.json` to point to the URL where your `CurrencyApi` is running (default: `http://localhost:5000`).

## Building and Running
s
1. **Build the solution:**
    ```bash
    dotnet build
    ```
2. **Run EF Core Migrations (CurrencyRateUpdater):**
    ```bash
    cd CurrencyRateUpdater
    dotnet ef database update
    ```
3. **Run the projects:**
    *   Start the `CurrencyApi` project first.
    *   Then, start the `CurrencyRateUpdater` project (it will run in the background).
    *   Finally, start the `Web` project.

    You can run each project using `dotnet run` from their respective project directories or use an IDE like Visual Studio or VS Code to start them.

## Usage

*   **CurrencyApi:** Access the Swagger UI at `http://localhost:5000/swagger` (or your configured URL) to explore and test the API endpoints.
*   **Web:**
    *   **Currency Conversion:** Fill out the form on the home page to perform currency conversions using the latest or historical rates.
    *   **Historical Rates:** Navigate to the "Historical Rates" page (you might need to add a link in the navigation) to view a chart of historical exchange rate trends. Enter a currency code, start date, and end date, then click "Get Rates."