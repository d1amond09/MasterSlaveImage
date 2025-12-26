@echo off
echo Starting Slave Nodes...

cd MasterSlaveImage.SlaveNode

start "Worker Alpha (9001)" dotnet run -- -p 9001 -n "Worker-Alpha"
timeout /t 2 /nobreak >nul

start "Worker Beta (9002)" dotnet run -- -p 9002 -n "Worker-Beta"
timeout /t 2 /nobreak >nul

start "Worker Gamma (9003)" dotnet run -- -p 9003 -n "Worker-Gamma"

echo All workers started.
cd ..