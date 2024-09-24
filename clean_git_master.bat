@echo off
setlocal

REM Define branch names
set NEW_BRANCH=new-branch
set OLD_BRANCH=master

REM Create a backup of the current state
echo Creating backup branch...
git checkout -b backup-%OLD_BRANCH%
if %errorlevel% neq 0 exit /b %errorlevel%

REM Check out a new branch with no history
echo Checking out a new branch with no history...
git checkout --orphan %NEW_BRANCH%
if %errorlevel% neq 0 exit /b %errorlevel%

REM Remove all files from the index (preserve working directory files)
echo Removing all files from index...
git rm -r --cached .
if %errorlevel% neq 0 exit /b %errorlevel%

REM Add all files and make a new initial commit
echo Adding all files and committing...
git add .
git commit -m "Initial commit with clean history"
if %errorlevel% neq 0 exit /b %errorlevel%

REM Push the new branch to the remote
echo Pushing new branch to remote...
git push origin %NEW_BRANCH% --force
if %errorlevel% neq 0 exit /b %errorlevel%

REM Update the remote master branch
echo Updating remote master branch...
git push origin %NEW_BRANCH%:%OLD_BRANCH% --force
if %errorlevel% neq 0 exit /b %errorlevel%

REM Delete the old branch locally
echo Deleting old branch locally...
git branch -D %OLD_BRANCH%
if %errorlevel% neq 0 exit /b %errorlevel%

REM Rename the new branch to master
echo Renaming new branch to master...
git checkout -b %OLD_BRANCH%
if %errorlevel% neq 0 exit /b %errorlevel%

REM Push the renamed master branch to the remote
echo Pushing renamed master branch to remote...
git push origin %OLD_BRANCH% --force
if %errorlevel% neq 0 exit /b %errorlevel%

REM Clean up remote branches
echo Cleaning up remote branches...
git push origin --delete %NEW_BRANCH%
if %errorlevel% neq 0 exit /b %errorlevel%

echo Git history cleanup completed successfully.
endlocal
