echo off
sc query "SmartCrmSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCrmSchedulerService is running.
	sc stop "SmartCrmSchedulerService"	
) else (
    echo "serviceName is not running"
)
