# Currency Converter

This project is an ASP.NET Core Web API for fetching and converting exchange rates from frankfurter. It includes caching, retry mechanism and error handling features.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/irfanonly/CurrencyConverter.git
```

## Run the application
```bash
dotnet build
dotnet run
```
Once application started, we can make sure by opening swagger url [http://localhost:5177/swagger/index.html](http://localhost:5177/swagger/index.html) and we can able to test the endpoints

## Run the unit tests
```base
dotnet test
```
## Configuration
```base
"FRANK_API": "<url>BASE URL OF CONSUME FROM</url>",
"CONVERT_EXCLUSION_LIST": "[\"TRY\",\"PLN\",\"THB\",\"MXN\"]",
"CachDurationInSeconds" : 300
```
+ FRANK_API : The backend base URL of hosted frankfurter app
+ CONVERT_EXCLUSION_LIST : Exclusion list of currency codes on capital letters in stringified json format
+ CachDurationInSeconds : the cache duration in memory, default 60 seconds if this is not set

# API Documentation

## Endpoint: Get Latest Exchange Rates

### Request

**Method:** `GET`

**URL:** `/Currency/latest`

**Parameters:**

| Name         | Type   | Location | Default Value | Description                     |
|--------------|--------|----------|---------------|---------------------------------|
| baseCurrency | string | query    | EUR           | The base currency code.         |

**Example Request:**

```http
GET /Currency/latest?baseCurrency=USD
```
**Example response:**
```json
{"amount":1.0,"base":"EUR","date":"2024-05-17","rates":{"AUD":1.6281,"BGN":1.9558,"BRL":5.5645,"CAD":1.4784,"CHF":0.9855}}

```

## Endpoint: Convert Currency Amount

### Request

**Method:** `GET`

**URL:** `/Currency/convert`

**Parameters:**

| Name         | Type    | Location | Default Value | Description                           |
|--------------|---------|----------|---------------|---------------------------------------|
| amount       | number  | query    | 1             | The amount of currency to convert.    |
| fromCurrency | string  | query    | EUR           | The currency code to convert from.    |
| toCurrency   | string  | query    | USD           | The currency code to convert to.      |

**Example Request:**

```http
GET /Currency/convert?amount=100&fromCurrency=EUR&toCurrency=USD
```

**Example response:**
```json
[
  {
    "key": "2024-05-02T00:00:00",
    "value": {
      "AUD": 1.6386
    }
  },
  {
    "key": "2024-05-03T00:00:00",
    "value": {
      "AUD": 1.633
    }
  }
]

```
## Endpoint: Get Historical Exchange Rates

### Request

**Method:** `GET`

**URL:** `/Currency/history`

**Parameters:**

| Name      | Type     | Location | Default Value | Description                               |
|-----------|----------|----------|---------------|-------------------------------------------|
| fromDate  | string   | query    | 2024-05-01    | The start date for the historical data.   |
| toDate    | string   | query    | 2024-05-17    | The end date for the historical data.     |
| currency  | string   | query    | AUD           | The currency code to get the rates for.   |
| page      | integer  | query    | 1             | The page number for pagination.           |
| pageSize  | integer  | query    | 10            | The number of records per page.           |

**Example Request:**

```http
GET /Currency/history?fromDate=2024-05-01&toDate=2024-05-17&currency=AUD&page=1&pageSize=10
```
**Example response:**
```json
[
  {
    "key": "2024-05-02T00:00:00",
    "value": {
      "AUD": 1.6386
    }
  },
  {
    "key": "2024-05-03T00:00:00",
    "value": {
      "AUD": 1.633
    }
  }
]

```
**Common Responses:**
+ 200 Success: Returns the value if API calls successful.
+ 400 Bad Request: Returns an error if the parameters are invalid.
+ 404 Not Found: Returns an error if data is not found.
+ 500 Internal Server Error: Returns an error if there is a server-side issue.

## Error Handling
All API endpoints handle errors and return appropriate HTTP status codes along with error messages.

## Caching
This API uses in-memory caching to improve performance. The cache duration can be configured in the appsettings.json file.

## Logging
The API uses ILogger for logging information and errors. Logs can be configured in the appsettings.json file.

## Retry Mechanism
The API uses Polly framework for retrying the http calls if it is not responding. 

## Assumptions
+ The frankfurter API returns InternalServerError or RequestTimeout, if it is not responding the request at first time 

