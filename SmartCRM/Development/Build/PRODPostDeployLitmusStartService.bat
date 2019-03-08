echo off
sc query "Smart CRM - Litmus Test Processor" | findstr /i "RUNNING"
if not errorlevel 1 (
    echo "Smart CRM - Litmus Test Processor is running.	
) else (
    echo "serviceName is not running"
	sc start "Smart CRM - Litmus Test Processor"	
)
