
echo off
sc query "SmartCrmSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCrmSchedulerService is running.
	sc stop "SmartCrmSchedulerService"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "SmartCRMWebAnalyticsEngine" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCRMWebAnalyticsEngine is running.
	sc stop "SmartCRMWebAnalyticsEngine"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "LMITSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "LMITSchedulerService is running.
	sc stop "LMITSchedulerService"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Action Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Action Processor is running.
	sc stop "Smart CRM - Action Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Bulk Operation Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Bulk Operation Processor is running.
	sc stop "Smart CRM - Bulk Operation Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Campaign Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Campaign Processor is running.
	sc stop "Smart CRM - Campaign Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Import Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Import Lead Processor is running.
	sc stop "Smart CRM - Import Lead Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Index Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Index Processor is running.
	sc stop "Smart CRM - Index Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Lead Processor is running.
	sc stop "Smart CRM - Lead Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Form Submission Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Form Submission Processor is running.
	sc stop "Smart CRM - Form Submission Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - API Lead Submission Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - API Lead Submission Processor is running.
	sc stop "Smart CRM - API Lead Submission Processor"	
) else (
    echo "serviceName is not running"
)

echo off
sc query "Smart CRM - Notification Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Notification Processor is running.
	sc stop "Smart CRM - Notification Processor"	
) else (
    echo "serviceName is not running"
)