echo off
sc query "Smart CRM - Import Lead Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Import Lead Processor is running.
	sc stop "Smart CRM - Import Lead Processor"	
) else (
    echo "serviceName is not running"
)

