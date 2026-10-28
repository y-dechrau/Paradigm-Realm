# Checks

In Unity: **Paradigm Realm > Run Engine Checks** and **Run Graphics Checks**.
The engine command includes StrategyChecks and a real Unity JsonUtility round trip.

With .NET 8 installed, from this project folder:

```sh
dotnet run --project Tests/EngineChecks.csproj
```

This runs the pure engine suites, including campaign simulations and new city/AI
checks. It does not load Unity or render the game.

UnityStubs.cs, UIProgram.cs and VisualRegression.cs are the supplementary API-double
harness sources used during development. They stay OUTSIDE Assets and must not be
copied into Unity's compiled folders. Their JSON test uses System.Text.Json, not
Unity JsonUtility. Render.cs is the CPU before/after rendering harness; it expects
RGB bytes extracted from the original supplied terrain PNGs into Tests/pixels.
LegacyTerrainRendering.txt retains the previous algorithm for the comparison.

From the project root, the supplementary harness is reproducible with:

```sh
dotnet run --project Tests/HarnessChecks.csproj
dotnet build Tests/HarnessChecks.csproj -p:StartupObject=UIProgram -t:Rebuild
dotnet Tests/bin/Debug/net8.0/HarnessChecks.dll
```

Its Resources stub resolves project assets from the project root or development
workspace. Its save folder is a temporary harness path, not your real Unity saves.

Settlement adviser controls and save tests (API doubles):

```sh
dotnet run --project Tests/SettlementUIChecks.csproj
```

## Diplomacy Stage 3

The engine command now includes DiplomacyChecks. The Editor command also seeds diplomacy data before its real Unity JSON round trip. Run the supplementary interface and System.Text.Json tests with:

```sh
dotnet run --project Tests/DiplomacyUIChecks.csproj
```

See Design/DIPLOMACY-STAGE-3.md for scope and limitations. API-double tests are not Unity Play Mode tests.
