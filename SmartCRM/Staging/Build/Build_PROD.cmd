echo warning: your are deploying PROD environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /p:EnvironmentName=PROD;UserName=smartweb;Password=f46MHhFZRmLsFQ5w
pause