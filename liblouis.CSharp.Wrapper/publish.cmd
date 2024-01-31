rem This file is run from <SolutionDir>\liblouis.CSharp.WrapperTestCmd\bin\Debug
rem It must be called by the PostBuild EventHandler of any application consuming the LibLouisWrapper project in order
rem to copy all files needed to the bin\debug folder of the consuming application. 
rem Copy the whole LibLouis directory from liblouis.CSharp.Wrapper\liblouis directory (which is controlled by Git) to the Debug Directory.
rem The LibLouisWrapper project expects to find it there.

rem echo WARNING: -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
rem echo WARNING: LibLouisWrapper.Publish.cmd is currently disabled. It must be enabled on first build after initial pull from GitHub and when the Liblouis installation files have been changed!  
rem echo WARNING: ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- 

rem exit

dir ..\..\..\liblouis.CSharp.Wrapper\liblouis

rmdir liblouis
mkdir liblouis

xcopy ..\..\..\liblouis.CSharp.Wrapper\liblouis liblouis /s/e/v/y

dir liblouis

pause

exit