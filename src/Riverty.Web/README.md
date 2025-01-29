**4. `WebUI` (ASP.NET Core MVC)**

# WebUI

## Overview

`WebUI` is an ASP.NET Core MVC application that provides a simple user interface for interacting with the `CurrencyApi` service. It allows users to:

-   View the latest exchange rates.
-   View historical exchange rates for a specific currency and date range.
-   Visualize historical exchange rate trends using a line chart (powered by Chart.js).

## Features

-   ASP.NET Core MVC framework.
-   Razor views for UI rendering.
-   Uses `IHttpClientFactory` to make API calls to the `CurrencyApi` service.
-   Displays a form with inputs for:
    -   Source currency
    -   Target currency
    -   Rate type (Latest or Historical)
    -   Date (for historical rates, using a date picker)
    -   Amount
-   Displays exchange rate calculation results.
-   Renders a line chart for historical rates using Chart.js.
-   Basic input validation using Data Annotations.

## Prerequisites

-   .NET 9
-   The `CurrencyApi` service must be running and accessible.

## Setup

1. **API URL:**
    -   Ensure that the `CurrencyApi` service is running.
    -   Update the API URL (`http://localhost:5000` or similar) in the `HomeController` if necessary.
2. **Build:**
    -   Build the solution using `dotnet build`.

## Running the Application

1. Navigate to the `WebUI` project directory in your terminal.
2. Run the application using `dotnet run`.

The application will start, and you can access it in your browser (usually at `http://localhost:5237` based on `launchSettings.json`).

## Usage

1. **Currency Conversion:**
    -   Enter the source and target currencies (3-letter codes).
    -   Select the rate type (Latest or Historical).
    -   If selecting Historical, choose a date from the date picker.
    -   Enter the amount to convert.
    -   Click "Calculate".
    -   The converted amount will be displayed below the form.

2. **Historical Rates Chart:**
    -   Click on the "Historical Rates" link (you might need to add this link to your layout or navigation).
    -   Enter the currency code, start date, and end date.
    -   Click "Get Rates".
    -   A line chart will be displayed showing the historical exchange rate trends for the selected currency and period.