echo off
sc query "Smart CRM - Campaign Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Campaign Processor is running.
	sc stop "Smart CRM - Campaign Processor"	
) else (
    echo "serviceName is not running"
)