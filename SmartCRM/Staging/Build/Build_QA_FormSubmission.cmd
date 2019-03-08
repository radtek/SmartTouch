echo warning: your are deploying STAGING environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:FormSubmission /p:EnvironmentName=QA;UserName=Administrator;Password=Lmit@1234
pause