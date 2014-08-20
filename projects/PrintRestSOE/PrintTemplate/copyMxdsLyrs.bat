@echo copy start
set TMP_ROOT_PATH=%HOME%\SOEPrintConfig
set TMP_LOCALFILES=LocalFiles
set TMP_LYRS=Lyrs
set TMP_MXDS=Mxds
set TMP_CURRENT=%cd%

copy /y %TMP_CURRENT%\%TMP_LYRS%\*.* %TMP_ROOT_PATH%\%TMP_LYRS%
copy /y %TMP_CURRENT%\%TMP_MXDS%\*.* %TMP_ROOT_PATH%\%TMP_MXDS%
@echo copy end