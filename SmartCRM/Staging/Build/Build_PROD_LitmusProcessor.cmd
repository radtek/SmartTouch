echo warning: your are deploying Production environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:LitmusProcessor /p:EnvironmentName=PROD;UserName=smartweb;Password=f46MHhFZRmLsFQ5w
pause