rem This file is run from <SolutionDir>\LibLouisWrapper\bin\Debug
rem It must be called by the PostBuild EventHandler of any application consuming the LibLouisWrapper project in order
rem to copy all files needed to the bin\debug folder of the consuming application. 
rem Copy the whole LibLouis directory from  ThirdPartyDlls directory (which is controlled by Git) to the Debug Directory.
rem The LibLouisWrapper project expects to find it there.

echo WARNING: -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
echo WARNING: LibLouisWrapper.Publish.cmd is currently disabled. It must be enabled on first build after initial pull from GitHub and when the Liblouis installation files have been changed!  
echo WARNING: ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- 

exit

dir ..\..\..\ThirdPartyDlls\liblouis 

rmdir liblouis
mkdir liblouis

xcopy ..\..\..\ThirdPartyDlls\liblouis liblouis /s/e/v/y

dir liblouis

pause

exit