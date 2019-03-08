echo off
sc query "SmartCRMWebAnalyticsEngine" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCRMWebAnalyticsEngine is running.
	sc stop "SmartCRMWebAnalyticsEngine"	
) else (
    echo "serviceName is not running"
)
