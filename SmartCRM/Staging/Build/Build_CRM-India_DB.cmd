echo warning: your are deploying CRM-India environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:DB /p:EnvironmentName=CRM-India;UserName=stadminin;Password=UP935eugIQnAd
pause