echo warning: your are deploying Prod environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:DB /p:EnvironmentName=PROD;UserName=smartweb;Password=SmTQl67En
pause