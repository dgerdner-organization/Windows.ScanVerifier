rem 	Copy this to the folder you wish to add to GitHub
rem	Then run it, passing the Github link to the repository that you wish to attach this to
rem
rem      NOTE: you will need to create the repository first.  Do not add a Readme file until after
rem            this has been performed.
set /p link= "GithubLink: "
git init
git add .
git commit -m "First Commit"
git remote add origin %link%
git push -u origin master
pause
