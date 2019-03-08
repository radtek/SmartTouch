echo warning: your are deploying PROD environment
pause
echo off
MSBuild.exe SmartTouchBuild.proj /t:SearchEngine /p:EnvironmentName=PROD;UserName=smartweb;Password=SmTQl67En
pause