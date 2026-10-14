# Unit Conversion API

Convert a measurement from one unit to another with a small HTTP API. For example, send `100 km` and get approximately `62.1371 mi` back. The API supports length, temperature, and mass conversions, and includes a browser page for trying requests.

## What it does

- Converts between supported length units, such as meters, kilometers, feet, and miles.
- Converts between Celsius, Fahrenheit, and Kelvin.
- Converts between supported mass units, such as kilograms, grams, pounds, and ounces.
- Lists available units so a caller can discover valid symbols.
- Provides a health endpoint at `GET /health` so a local run or hosting service can confirm the API is responding.
- Rejects unknown units and conversions between different categories with a useful error response.

All conversion data is stored in the application. You do not need a database or an external service to run it.

## Run it on your computer

You need the **.NET 10 SDK** ([download it here](https://dotnet.microsoft.com/download/dotnet/10.0)). Open a terminal in the project folder—the folder containing `UnitConversionApi.sln`—and run:

```sh
dotnet restore
dotnet run
```

When the application starts, open [http://localhost:5080](http://localhost:5080) in your browser. The page takes you to Swagger, where you can view the API and try requests. To stop the application, return to the terminal and press **Ctrl+C**.

## Try a conversion

In Swagger, expand `POST /api/conversions`, select **Try it out**, paste this JSON, and select **Execute**:

```json
{
  "value": 25,
  "fromUnit": "C",
  "toUnit": "F"
}
```

The response should have HTTP status `200` and a result of `77` Fahrenheit.

You can also list supported units at `GET /api/units`. The response groups unit symbols and names under `length`, `temperature`, and `mass`.

### Manual test cases

Use these payloads in Swagger's **Try it out** panel for `POST /api/conversions`:

| Case | Request body | Expected result |
| --- | --- | --- |
| Kilometers to miles | `{"value":100,"fromUnit":"km","toUnit":"mi"}` | `200`; result about `62.1371` |
| Celsius to Fahrenheit | `{"value":25,"fromUnit":"C","toUnit":"F"}` | `200`; result `77` |
| Pounds to kilograms | `{"value":10,"fromUnit":"lb","toUnit":"kg"}` | `200`; result about `4.53592` |
| Same unit | `{"value":12.5,"fromUnit":"m","toUnit":"m"}` | `200`; result `12.5` |
| Case-insensitive symbol | `{"value":1,"fromUnit":"KM","toUnit":"m"}` | `200`; result `1000` |
| Unknown unit | `{"value":1,"fromUnit":"foo","toUnit":"m"}` | `400`; the error identifies `foo` |
| Different categories | `{"value":1,"fromUnit":"m","toUnit":"kg"}` | `400`; categories differ |
| Missing unit | `{"value":1,"fromUnit":"","toUnit":"m"}` | `400`; validation error |
| Missing value | `{"fromUnit":"m","toUnit":"cm"}` | `400`; validation error |
| Out-of-range JSON number | `{"value":1e309,"fromUnit":"m","toUnit":"cm"}` | `400`; the invalid number is rejected while parsing the request |
| Finite input that overflows the result | `{"value":1e308,"fromUnit":"km","toUnit":"m"}` | `422`; result is outside the supported numeric range |

## Supported units

| Category | Symbols |
| --- | --- |
| Length | `m`, `km`, `cm`, `mm`, `mi`, `yd`, `ft`, `in` |
| Temperature | `C`, `F`, `K` |
| Mass | `kg`, `g`, `mg`, `lb`, `oz`, `t` |

Unit symbols are case-insensitive. The complete list, including each unit's name, is returned by `GET /api/units`.

## API reference

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/units` | List supported units by category. |
| `POST` | `/api/conversions` | Convert a value between two units in the same category. |
| `GET` | `/health` | Check that the API is running. |

Example request for `POST /api/conversions`:

```json
{
  "value": 100,
  "fromUnit": "km",
  "toUnit": "mi"
}
```

Example response:

```json
{
  "inputValue": 100,
  "fromUnit": "km",
  "result": 62.13711922373339,
  "toUnit": "mi"
}
```

The API returns `400 Bad Request` for an unknown unit, incompatible categories, or invalid request data. It returns `422 Unprocessable Entity` if the result exceeds the numeric range supported by the application. Unexpected server errors return a generic `500 Internal Server Error`; technical details are written to the server log instead of being exposed to callers.

## Run the automated checks

The test project sends requests to an in-memory copy of the API; it does not need the local server to be running. From the project folder, run:

```sh
dotnet test UnitConversionApi.sln
```

The same restore, build, and test steps run automatically in GitHub Actions when code is pushed or a pull request is opened.

## How the project is organized

```text
Controllers/                  HTTP routes and request validation
Models/                       Request and response shapes
Application/                  Conversion service
Domain/                       Unit definitions, catalog, and domain errors
Infrastructure/               Shared exception handling and request logging
UnitConversionApi.Tests/      API integration tests
.github/workflows/            Automated build and test workflow
```

The request flow is **HTTP request → controller → conversion service → unit catalog**. Controllers deal with HTTP input and responses. The service performs the conversion. The catalog owns the supported units and their factors.

## Design choices and trade-offs

- **One conversion formula:** Each unit stores a scale and offset to a canonical unit. Length converts through meters, mass through kilograms, and temperature through Kelvin. This supports the required conversions without separate pair-by-pair formulas.
- **Data kept in code:** The assessment allows hard-coded conversion factors, so the catalog is in memory and the app starts without a database. Adding a unit that follows the same scale-and-offset rule means adding one catalog entry. If unit definitions need frequent non-developer updates, they can later move to validated configuration or a database.
- **Future conversion rules:** The shared formula fits linear and affine conversions. A future category with nonlinear or context-dependent rules should use a separate conversion strategy rather than adding special cases to the current formula.
- **Errors handled in one place:** Middleware translates expected conversion errors into client responses, logs unexpected exceptions, and records request completion in a `finally` block. This keeps controller actions focused on HTTP behavior.
- **Local API explorer:** Swagger is enabled in Development for local exploration and is not exposed by this configuration in other environments.

## Requirements and configuration

- ASP.NET Core targets .NET 10, an active Long Term Support release.
- `global.json` selects the .NET 10 SDK feature band while allowing newer .NET 10 feature-band SDKs.
- `appsettings.json` contains application logging and host configuration. `Properties/launchSettings.json` configures local Development mode and port `5080`.
- `.editorconfig` keeps formatting consistent across contributors, and `.gitignore` excludes build output and IDE files.
- The project is split into controllers, application logic, domain types, infrastructure, tests, and documentation to support team maintenance.

## Assessment scope

This API implements the required length, temperature, and mass categories. The assessment permits conversion factors and other required data to be hard-coded for this version. It asks for a locally runnable ASP.NET Core solution, clear setup instructions, a maintainable project structure, and documentation of key design decisions; those are covered above.
