**1. `Riverty.ExchangeRateCalculator` (console app project)**

# Riverty.ExchangeRateCalculator

## Overview

`Riverty.ExchangeRateCalculator` is a .NET console app project that provides the core logic for calculating currency exchange rates. It fetches exchange rate data from the Fixer.io API and performs conversions based on the latest or historical rates.

## Features

-   **Interactive Console Application:**
    -   Allows users to perform currency conversions directly from the console.
    -   Uses arrow keys (Up/Down) to select source and target currencies from a list.
    -   Prompts users to enter the amount to convert.
    -   Displays conversion results in the console.
    -   Supports "Latest" and "Historical" rate types.
    -   For historical rates, prompts users for a date in `yyyy-MM-dd` format.
-   **Reusable Class Library:**
    -   Provides a `CurrencyService` class that encapsulates the exchange rate logic.
    -   `CurrencyService` can be used as a dependency in other .NET projects.
    -   Fetches exchange rates from the Fixer.io API.
    -   Performs currency conversions.
    -   Reads API keys and base URLs from configuration (`appsettings.json`).

## Prerequisites

-   .NET 9 SDK 
-   A Fixer.io API key (set in `appsettings.json`) the current one if exists is revoked

## Building and Running the Console Application

1. **Clone the repository:**
    ```bash
    git clone https://github.com/saadatzi/Riverty
    ```

2. **Navigate to the project directory:**
    ```bash
    cd Riverty/Riverty.ExchangeRateCalculator
    ```

3. **Restore NuGet packages:**
    ```bash
    dotnet restore
    ```

4. **Build the project:**
    ```bash
    dotnet build
    ```

5. **Run the application:**
    ```bash
    dotnet run
    ```

## Configuration

The `CurrencyService` reads the following configuration values from `IConfiguration`:

-   **`FixerApiKey`:** Your Fixer.io API key.
-   **`FixerBaseUrl`:** The base URL for the Fixer.io API (default: `http://data.fixer.io/api/`).

You can set these values in an `appsettings.json` file in your consuming project.