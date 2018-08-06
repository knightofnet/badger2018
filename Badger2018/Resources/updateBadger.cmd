@echo off

if [%1]==[] goto askFile
Set _updFile=%1
echo La mise … jour va commencer
timeout /T 10
goto verifFile

:askFile
Set /P _updFile=Entrez le chemin du fichier de mise … jour || Set _dept=null
if "%_updFile%"=="null" goto exitErrNoFileSel
Set _updFile="%_updFile%"
goto verifFile


:verifFile
echo Fichier : %_updFile%
if exist %_updFile% goto upd
goto exitErrFileNotExist

:upd
tasklist /FI "IMAGENAME eq Badger2018.exe" /NH | find /I "Badger2018.exe">NUL
if "%ERRORLEVEL%"=="0" goto errPrgRunning

echo Mise … jour de l'application
start "Sauvegarde de la configuration" /D ".." /W "../Badger2018.exe" -e tmpFile.xml

echo Extraction de la mise … jour
start "Mise … jour" /D ".." /W %_updFile% -o"." -y -pAlBadger2018
start "R‚cup‚ration de la configuration" /D ".." /W "../Badger2018.exe" -i tmpFile.xml

cd..
del tmpFile.xml

echo Mise … jour termin‚e avec succŠs.

if [%1]==[] goto exit
echo Lancement du programme
start "" "Badger2018.exe"
goto exit

:errPrgRunning
echo Veuillez quitter l'application Badger2018 avant de d‚marrer une mise … jour
goto exit

:errorUpd
echo Une erreur est survenue lors de la mise … jour
goto exit

:exitErrFileNotExist
echo Le fichier selectionn‚ n'existe pas
goto exit

:exitErrNoFileSel
echo Aucun fichier selectionn‚
goto exit


:exit
REM pause
goto:eof