xcopy /S /I /Q /Y /F "." "../.devcontainer/"
docker-compose up -d

echo "[Enter] to exit"
pause > nul