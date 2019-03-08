echo off
sc query "SmartCrmSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCrmSchedulerService is running.	
) else (
    echo "serviceName is not running"
	sc start "SmartCrmSchedulerService"	
)

echo off
sc query "SmartCRMWebAnalyticsEngine" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "SmartCRMWebAnalyticsEngine is running.	
) else (
    echo "serviceName is not running"
	sc start "SmartCRMWebAnalyticsEngine"	
)

echo off
sc query "LMITSchedulerService" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "LMITSchedulerService is running.	
) else (
    echo "serviceName is not running"
	sc start "LMITSchedulerService"	
)

echo off
sc query "Smart CRM - Action Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Action Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Action Processor"	
)

echo off
sc query "Smart CRM - Bulk Operation Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Bulk Operation Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Bulk Operation Processor"	
)

echo off
sc query "Smart CRM - Campaign Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Campaign Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Campaign Processor"	
)

echo off
sc query "Smart CRM - Import Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Import Lead Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Import Lead Processor"	
)

echo off
sc query "Smart CRM - Index Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Index Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Index Processor"	
)

echo off
sc query "Smart CRM - Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Lead Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Lead Processor"	
)

echo off
sc query "Smart CRM - Form Submission Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Form Submission Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Form Submission Processor"	
)

echo off
sc query "Smart CRM - API Lead Submission Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM APILeadSubmissionProcessor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - API Lead Submission Processor"	
)
