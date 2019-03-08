
for /d %%a in ("E:\Staging\windowsservices\smartcrm-campaign-processor*") do xcopy "E:\Staging\windowsservices\Dependencydll" /e /y "%%a"

for /d %%a in ("E:\Staging\windowsservices\crm-scheduler-service*") do xcopy "E:\Staging\windowsservices\Dependencydll" /e /y "%%a"



