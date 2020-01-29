Start-Process -FilePath "WorkerService.exe" -WorkingDirectory ".\release\WorkerService"
Start-Process -FilePath "LoaderClient.exe" -WorkingDirectory ".\release\LoaderClient"