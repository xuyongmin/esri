@echo delete start
set TMP_ROOT_PATH=%HOME%\SOEPrintConfig
set TMP_LOCALFILES=LocalFiles

del /a /f /q %TMP_ROOT_PATH%\%TMP_LOCALFILES%\*

@echo delete end