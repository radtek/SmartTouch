echo warning: your are deploying Production environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:SmartSearchProcessor /p:EnvironmentName=PROD;UserName=smartweb;Password=SmTQl67En
pause