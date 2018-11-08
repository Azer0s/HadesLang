all:
	@$(MAKE) interpreter
	@$(MAKE) web
	@$(MAKE) runlang

interpreter:
	@echo
	@echo "\033[4m\033[1mBuilding HadesLang\033[0m"
	@echo
	@dotnet build HadesLang/HadesLang.csproj

web:
	@echo
	@echo "\033[4m\033[1mBuilding HadesWeb\033[0m"
	@echo
	@dotnet build HadesWeb/HadesWeb.csproj

runlang:
	@dotnet run --project HadesLang

.ONESHELL:
runweb:
	$(shell cd HadesWeb/bin/Debug/netcoreapp2.0/; dotnet HadesWeb.dll)