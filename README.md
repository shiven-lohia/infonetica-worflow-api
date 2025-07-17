# Configurable Workflow Engine (State-Machine API)

Minimal backend service built with C# (.NET 8) for defining workflows as state machines, creating instances, executing actions, and checking state.

## Quick Start Instructions

### Requirements

* .NET 8 SDK
* Visual Studio Code with:

  * C# extension
  * REST Client extension

### Running the Server

```bash
dotnet run
```

Default server URL: `http://localhost:5041/`

### Testing the API

Open the `requests.http` file in VS Code. You can directly run the sample requests using the REST Client extension.

## Assumptions and Notes

* Data is stored in memory. It will reset when the server stops.
* Workflows must have exactly one initial state.
* The API checks that actions are only executed when valid (right source state, not final state, etc.).
* Implemented using ASP.NET Core Minimal API pattern.

## Known Limitations

* No persistence layer like a database or file storage.
* No user authentication.
* No support for concurrent edits or large-scale production use.
* Backend only, no frontend interface.
