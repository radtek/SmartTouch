echo off
sc query "Smart CRM - Form Submission Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Form Submission Processor is running.
	sc stop "Smart CRM - Form Submission Processor"	
) else (
    echo "serviceName is not running"
)
