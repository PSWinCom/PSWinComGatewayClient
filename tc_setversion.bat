@echo off
set buildno=%build.counter%
set branch=%teamcity.build.branch%

echo Generating build number for branch %branch%

setlocal ENABLEDELAYEDEXPANSION

set version=0.0.1
if exist version.txt (
  for /f "delims=" %%x in (version.txt) do set version=%%x
)

"%env.TEAMCITY_GIT_PATH%" log --decorate=full --simplify-by-decoration --pretty=oneline --first-parent HEAD | findstr /C:"tag: v%version%-rc" > temptags.dat

set postfix=-dev-b%buildno%
if not x%branch:release-=% == x%branch% (
  echo Determining release candidate number
  set /a rcn=1
  for /f "delims=" %%x in (temptags.dat) do (
    set /a rcn+=1
  )

  set postfix=-rc!rcn!
)
if %branch%==develop set postfix=-next-b%buildno%
if %branch%==master set postfix=

del temptags.dat

set t=!version!!postfix!
echo ##teamcity[buildNumber '%t%']

endlocal