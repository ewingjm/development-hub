@pushd %~dp0

@where /q msbuild

@IF ERRORLEVEL 1 (
	echo "MSBuild is not in your PATH. Please use a developer command prompt!"
	goto :end
) ELSE (
	MSBuild.exe "Capgemini.DevelopmentHub.Tests.Ui.csproj"
)

@if ERRORLEVEL 1 goto end

@cd ..\packages\SpecRun.Runner.*\tools

@set profile=%1
@if "%profile%" == "" set profile=Default

"..\..\packages\SpecRun.Runner.1.7.2\toolsSpecRun.exe" run %profile%.srprofile "/baseFolder:%~dp0\bin\Debug" /log:specrun.log %2 %3 %4 %5

:end

@popd
