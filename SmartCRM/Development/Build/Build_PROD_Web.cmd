echo warning: your are deploying PROD environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:Web /p:EnvironmentName=PROD;UserName=smartweb;Password=TUNdvjfFWTbar9qa
pause