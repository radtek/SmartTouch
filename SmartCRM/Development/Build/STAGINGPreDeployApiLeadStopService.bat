echo off
sc query "Smart CRM - API Lead Submission Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - API Lead Submission Processor is running.
	sc stop "Smart CRM - API Lead Submission Processor"	
) else (
    echo "serviceName is not running"
)
