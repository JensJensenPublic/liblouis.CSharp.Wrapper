rem This file is run from <SolutionDir>\LibLouisWrapperTestCmd\bin\Debug
rem It must be called by the PostBuild EventHandler for the LibLouisWrapperTestCmd project
rem to copy all files needed to the bin\debug folder of the consuming application. 


dir ..\..\TestInputFiles 

rmdir TestInputFiles
mkdir TestInputFiles

xcopy ..\..\TestInputFiles TestInputFiles /s/e/v/y

dir TestInputFiles



