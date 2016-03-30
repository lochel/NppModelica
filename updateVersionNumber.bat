@echo off &setlocal

cd %~dp0

git describe --tags --abbrev=0 --match=v*.* > "PluginVersionNumber.txt"
git describe --tags --dirty --abbrev=7 --match=v*.* > "PluginVersion.txt"

set "search1=$$PluginVersionNumber$$"
set /p replace1=<PluginVersionNumber.txt
set replace1=%replace1:~1%

set "search2=$$PluginVersion$$"
set /p replace2=<PluginVersion.txt

(for /f "delims=" %%i in (VersionNumber.cs.tmpl) do (
    set "line=%%i"
    setlocal enabledelayedexpansion
    set "line=!line:%search1%=%replace1%!"
    set "line=!line:%search2%=%replace2%!"
    echo(!line!
    endlocal
))>"VersionNumber.cs"

del PluginVersionNumber.txt
del PluginVersion.txt
