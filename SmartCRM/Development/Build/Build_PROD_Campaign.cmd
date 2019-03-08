echo warning: your are deploying QA environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:Campaign /p:EnvironmentName=PROD;UserName=smartweb;Password=TUNdvjfFWTbar9qa
pause