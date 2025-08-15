# Tests

- Unit tests: `tests/Act.Core.UnitTests`
- Integration tests: `tests/Act.Core.IntegrationTests`

Run all tests with coverage:

```bash
dotnet test ACT.sln --collect:"XPlat Code Coverage" --results-directory tests/TestResults
```

Coverage thresholds:
- Unit tests: 80% line coverage (enforced)
- Integration tests: 75% line coverage (enforced)

Notes:
- Integration tests use an in-memory EF Core provider and in-memory configuration to avoid external dependencies.
- Health and WebHook endpoints are covered to ensure core functionality paths are validated.