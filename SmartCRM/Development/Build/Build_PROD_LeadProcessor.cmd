echo warning: your are deploying QA environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:LeadProcessor /p:EnvironmentName=PROD;UserName=smartweb;Password=SmTQl67En
pause