echo warning: your are deploying STAGING environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:FormSubmission /p:EnvironmentName=PROD;UserName=smartweb;Password=SmTQl67En
pause