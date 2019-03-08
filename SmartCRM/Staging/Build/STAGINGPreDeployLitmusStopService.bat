echo off
sc query "Smart CRM - Litmus Test Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Litmus Test Processor is running.
	sc stop "Smart CRM - Litmus Test Processor"	
) else (
    echo "serviceName is not running"
)
