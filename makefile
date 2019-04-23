all:
	@$(MAKE) interpreter
	@$(MAKE) run

interpreter:
	@echo
	@echo "\033[4m\033[1mBuilding HadesLang\033[0m"
	@echo
	@dotnet build src/Hades.Core/Hades.Core.csproj

run:
	@echo
	@echo "\033[4m\033[1mRunning HadesLang\033[0m"
	@echo
	@dotnet run --project src/Hades.Core/Hades.Core.csproj
