echo warning: your are deploying PROD environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /p:EnvironmentName=PROD;UserName=smartweb;Password=TUNdvjfFWTbar9qa
pause