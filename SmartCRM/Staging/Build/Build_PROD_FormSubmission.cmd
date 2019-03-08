echo warning: your are deploying STAGING environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:FormSubmission /p:EnvironmentName=PROD;UserName=smartweb;Password=f46MHhFZRmLsFQ5w
pause