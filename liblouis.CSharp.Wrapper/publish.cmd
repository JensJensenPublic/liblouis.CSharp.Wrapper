rem This file is run from <SolutionDir>\liblouis.CSharp.WrapperTestCmd\bin\Debug
rem It must be called by the PostBuild EventHandler of any application consuming the LibLouisWrapper project in order
rem to copy all files needed to the bin\debug folder of the consuming application. 
rem Copy the whole LibLouis directory from liblouis.CSharp.Wrapper\liblouis directory (which is controlled by Git) to the Debug Directory.
rem The LibLouisWrapper project expects to find it there.

rem echo WARNING: -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
rem echo WARNING: Publish.cmd is currently disabled. It must be enabled on first build after the initial pull from GitHub and when the Liblouis installation files have been changed!  
rem echo WARNING: ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- 

rem exit

echo -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
echo The Post-build event handler 'Liblouis.CSharp.Wrapper.Publish.cmd' is currently enabled.
echo It MUST be enabled during the first successful first build after the initial pull from GitHub and when the Liblouis installation files have been changed!
echo After the first successful build it is no longer needed and may be disabled by removing the "rem" from the exit-command above!
echo ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- 

dir ..\..\..\liblouis.CSharp.Wrapper\liblouis

rmdir liblouis
mkdir liblouis

xcopy ..\..\..\liblouis.CSharp.Wrapper\liblouis liblouis /s/e/v/y

dir liblouis

pause

exit