echo off
sc query "Smart CRM - Action Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Action Processor is running.
	sc stop "Smart CRM - Action Processor"	
) else (
    echo "serviceName is not running"
)
