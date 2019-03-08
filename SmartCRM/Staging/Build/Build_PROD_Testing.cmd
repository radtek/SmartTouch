echo warning: your are deploying PROD environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:Testing /p:EnvironmentName=PROD;UserName=stadmin;Password=f46MHhFZRmLsFQ5w
pause