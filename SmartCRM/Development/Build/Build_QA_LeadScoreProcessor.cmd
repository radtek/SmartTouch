echo warning: your are deploying QA environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:LeadScoreProcessor /p:EnvironmentName=QA;UserName=Administrator;Password=Lmit@1234
pause