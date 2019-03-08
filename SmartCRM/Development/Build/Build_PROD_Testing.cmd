echo warning: your are deploying PROD environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:Testing /p:EnvironmentName=PROD;UserName=stadmin;Password=S^u*9G4lO
pause