echo off
sc query "SmartCrmSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCrmSchedulerService is running.	
) else (
    echo "serviceName is not running"
	sc start "SmartCrmSchedulerService"	
)
