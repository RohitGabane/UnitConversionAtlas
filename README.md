# Unit Conversion API

A standalone ASP.NET Core REST API for converting length, temperature, and mass values. It uses a small unit catalog and a shared canonical-unit model, so adding units is a catalog change rather than a new conversion formula.

## Requirements

- .NET 10 SDK

## Run locally

From this directory:

```sh
dotnet restore
dotnet run
```

The launch profile runs in Development on port 5080 and opens Swagger UI at `http://localhost:5080/` (also available directly at `/swagger`). Use **Try it out** to send requests. The generated OpenAPI document is at `http://localhost:5080/swagger/v1/swagger.json`.

## API

`GET /api/units` lists supported symbols and names, grouped by category. Unit symbols are case-insensitive.

`POST /api/conversions` accepts JSON:

```json
{
  "value": 100,
  "fromUnit": "km",
  "toUnit": "mi"
}
```

Successful response:

```json
{
  "inputValue": 100,
  "fromUnit": "km",
  "result": 62.13711922373339,
  "toUnit": "mi"
}
```

Unknown units, non-finite values, and cross-category conversions return a 400 response with an explanatory error. Temperature conversions use Kelvin as the canonical value and include offsets; length and mass use meters and kilograms respectively.

Example:

```sh
curl -X POST http://localhost:5080/api/conversions \
  -H "Content-Type: application/json" \
  -d '{"value":25,"fromUnit":"C","toUnit":"F"}'
```

## Testing with Swagger

Start the API with `dotnet run`, then open `http://localhost:5080/swagger`. Expand `POST /api/conversions`, select **Try it out**, paste one of the request bodies below, and select **Execute**. The expected status and result are listed for each case.

| Case | Request body | Expected result |
| --- | --- | --- |
| Kilometers to miles | `{"value":100,"fromUnit":"km","toUnit":"mi"}` | `200`; result about `62.1371` |
| Celsius to Fahrenheit | `{"value":25,"fromUnit":"C","toUnit":"F"}` | `200`; result `77` |
| Pounds to kilograms | `{"value":10,"fromUnit":"lb","toUnit":"kg"}` | `200`; result about `4.53592` |
| Same unit | `{"value":12.5,"fromUnit":"m","toUnit":"m"}` | `200`; result `12.5` |
| Case-insensitive symbol | `{"value":1,"fromUnit":"KM","toUnit":"m"}` | `200`; result `1000` |
| Unknown unit | `{"value":1,"fromUnit":"foo","toUnit":"m"}` | `400`; error identifies `foo` |
| Different categories | `{"value":1,"fromUnit":"m","toUnit":"kg"}` | `400`; error says the categories differ |
| Missing unit | `{"value":1,"fromUnit":"","toUnit":"m"}` | `400`; validation error |
| Out-of-range JSON number | `{"value":1e309,"fromUnit":"m","toUnit":"cm"}` | `400`; invalid JSON number is rejected during request parsing |

Also try `GET /api/units`; it should return `200` with `length`, `temperature`, and `mass` groups.

## Design notes

- `Controllers` contains the HTTP controllers for conversions and supported units.
- `Models` contains the conversion request and response contracts.
- `Domain` contains unit metadata, the catalog contract, and domain errors.
- `Application` contains the conversion service contract and implementation.
- `Program.cs` registers dependencies, configures middleware, and maps controllers.
- Each unit is represented by a scale and offset to its category's canonical unit. This handles both multiplicative conversions and affine temperature conversions with one algorithm.
- The catalog is registered as a singleton and is immutable after startup. New unit families that need nonlinear or context-dependent rules would warrant a strategy abstraction rather than forcing those rules into the scale-and-offset model.

All conversion data is intentionally in memory for this assessment. No external service or database is required.
