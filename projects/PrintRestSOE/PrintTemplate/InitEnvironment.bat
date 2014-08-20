@echo create folders
set TMP_ROOT_PATH=%HOME%\SOEPrintConfig
set TMP_LOCALFILES=LocalFiles
set TMP_LYRS=Lyrs
set TMP_MXDS=Mxds
set TMP_CURRENT=%cd%
mkdir %TMP_ROOT_PATH%\%TMP_LOCALFILES%
mkdir %TMP_ROOT_PATH%\%TMP_LYRS%
mkdir %TMP_ROOT_PATH%\%TMP_MXDS%
@echo end create folders

copyMxdsLyrs.bat

@echo start create TGisPrint iis root
wscript.exe CreateIISVirtualFolders.vbs Create TGisPrint "%TMP_ROOT_PATH%\LocalFiles"
@echo end create TGisPrint iis root